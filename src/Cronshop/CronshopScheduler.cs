using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using CSScriptLibrary;
using Quartz;
using Quartz.Impl;

namespace Cronshop
{
    public class CronshopScheduler : IDisposable
    {
        internal const string InternalFriendlyName = "__InternalFriendlyName";

        private readonly IScriptCatalog _catalog;
        private readonly List<JobInfo> _jobs = new List<JobInfo>();
        private readonly ReadOnlyCollection<JobInfo> _readOnlyJobs;
        private readonly IScheduler _scheduler;

        static CronshopScheduler()
        {
            // for now -- this is less painful on temp files and file locks.
            CSScript.CacheEnabled = false;
            CSScript.GlobalSettings.InMemoryAsssembly = true;
        }

        public CronshopScheduler(IScriptCatalog catalog)
        {
            _catalog = catalog;
            _catalog.CatalogChanged += CatalogChanged;

            _readOnlyJobs = new ReadOnlyCollection<JobInfo>(_jobs);

            ISchedulerFactory factory = new StdSchedulerFactory();
            _scheduler = factory.GetScheduler();
            Scheduler.JobFactory = new SafeJobFactory();
            Scheduler.AddGlobalJobListener(new MainJobMonitor(_readOnlyJobs));

            LoadJobsFromCatalog();
        }

        public ReadOnlyCollection<JobInfo> Jobs
        {
            get { return _readOnlyJobs; }
        }

        protected internal IScheduler Scheduler
        {
            get { return _scheduler; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Scheduler.Shutdown(true);

            _catalog.CatalogChanged -= CatalogChanged;
        }

        #endregion

        private void CatalogChanged(object sender, CatalogChangedEventArgs e)
        {
            foreach (var change in e.Changes)
            {
                Console.Write("change: " + change.Item1);
                Console.WriteLine(" -> " + change.Item2);
            }

            while (e.Changes.Count > 0)
            {
                Tuple<CronshopScript, CatalogChange> change = e.Changes.Dequeue();

                if (change.Item2 == CatalogChange.Deleted)
                {
                    UnscheduleScript(change.Item1);
                }
                else if (change.Item2 == CatalogChange.Created)
                {
                    ScheduleScript(change.Item1);
                }
                else if (change.Item2 == CatalogChange.Modified)
                {
                    UnscheduleScript(change.Item1);
                    ScheduleScript(change.Item1);
                }
            }
        }

        private void LoadJobsFromCatalog()
        {
            CronshopScript[] scripts = _catalog.Scripts.ToArray();

            foreach (CronshopScript script in scripts)
            {
                ScheduleScript(script);
            }
        }

        private void ScheduleScript(CronshopScript script)
        {
            Assembly assembly = LoadAssembly(script);

            if (assembly == null)
            {
                return;
            }

            // find implementations of CronshopJob
            IEnumerable<Type> types = assembly.GetTypes()
                .Where(t => typeof (CronshopJob).IsAssignableFrom(t) && !t.IsAbstract);

            foreach (Type type in types)
            {
                JobConfigurator configurator;

                // create the instance to get the schedule
                using (var instance = (CronshopJob) Activator.CreateInstance(type))
                {
                    configurator = new JobConfigurator();
                    instance.Configure(configurator);
                }

                string name = script.Name + "/" + type.Name;
                string friendlyName = script.FriendlyName + "/" + type.Name;

                var detail = new JobDetail(name, null, type) {Durable = true};
                detail.JobDataMap[InternalFriendlyName] = friendlyName;

                Scheduler.AddJob(detail, false);
                Console.WriteLine("ScheduleJob: " + detail.Name);

                var triggers = new List<Trigger>();

                foreach (string cron in configurator.Crons)
                {
                    var trigger = new CronTrigger(name + "/" + Guid.NewGuid(), null, cron)
                                      {
                                          JobName = name,
                                          MisfireInstruction = MisfireInstruction.CronTrigger.DoNothing,
                                      };

                    triggers.Add(trigger);

                    Scheduler.ScheduleJob(trigger);
                }

                _jobs.Add(new JobInfo(script, detail, triggers));
            }
        }

        private static Assembly LoadAssembly(CronshopScript script)
        {
            try
            {
                return CSScript.Load(script.FullPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        private void UnscheduleScript(CronshopScript script)
        {
            JobInfo[] infos = _jobs
                .Where(x => x.Script.FullPath == script.FullPath)
                .ToArray();

            foreach (JobInfo info in infos)
            {
                bool success = Scheduler.DeleteJob(info.JobDetail.Name, null);
                Console.WriteLine("DeleteJob: " + info.JobDetail.Name + " " + (success ? "success" : "FAILED"));

                _jobs.Remove(info);
            }
        }

        public void Start()
        {
            Scheduler.Start();
        }

        public void Stop()
        {
            Scheduler.Standby();
        }

        public object ExecuteJob(string jobName)
        {
            JobDetail detail = Scheduler.GetJobDetail(jobName, null);
            
            if (detail == null)
            {
                return null;
            }

            try
            {
                using (var instance = (CronshopJob) Activator.CreateInstance(detail.JobType))
                {
                    return instance.ExecuteJob(null);
                }
            }
            catch
            {
                return null;
            }
        }

        public void InterruptJob(string jobName)
        {
            Scheduler.Interrupt(jobName, null);
        }

        public void Pause(string jobName = null)
        {
            if (jobName == null)
            {
                Scheduler.PauseAll();
            }
            else
            {
                Scheduler.PauseJob(jobName, null);
            }
        }

        public void Resume(string jobName = null)
        {
            if (jobName == null)
            {
                Scheduler.ResumeAll();
            }
            else
            {
                Scheduler.ResumeJob(jobName, null);
            }
        }
    }
}