using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CSScriptLibrary;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;

namespace Cronshop
{
    public class CronshopScheduler : ICronshopScheduler
    {
        internal const string CronshopDefaultGroup = "Job";

        private readonly IScriptCatalog _catalog;
        private readonly IDictionary<JobKey, JobInfo> _jobs = new Dictionary<JobKey, JobInfo>();
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

            ISchedulerFactory factory = new StdSchedulerFactory();
            _scheduler = factory.GetScheduler();
            Scheduler.JobFactory = new SafeJobFactory();
            Scheduler.ListenerManager.AddJobListener(new MainJobMonitor(_jobs));

            LoadJobsFromCatalog();
        }

        protected internal IScheduler Scheduler
        {
            get { return _scheduler; }
        }

        #region ICronshopScheduler Members

        public void Dispose()
        {
            Scheduler.Shutdown(true);

            _catalog.CatalogChanged -= CatalogChanged;
        }

        public IEnumerable<JobInfo> Jobs
        {
            get { return _jobs.Values; }
        }

        public void Start()
        {
            Scheduler.Start();
        }

        public void Stop()
        {
            Scheduler.Standby();
        }

        public object ExecuteJob(JobKey jobKey)
        {
            IJobDetail detail = Scheduler.GetJobDetail(jobKey);

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

        public void InterruptJob(JobKey jobKey)
        {
            Scheduler.Interrupt(jobKey);
        }

        public void Pause(JobKey jobKey = null)
        {
            if (jobKey == null)
            {
                Scheduler.PauseAll();
            }
            else
            {
                Scheduler.PauseJob(jobKey);
            }
        }

        public void Resume(JobKey jobKey = null)
        {
            if (jobKey == null)
            {
                Scheduler.ResumeAll();
            }
            else
            {
                Scheduler.ResumeJob(jobKey);
            }
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

                string name = JobInfo.BuildJobName(script, type);

                var detail = new JobDetailImpl(name, CronshopDefaultGroup, type) {Durable = true};

                Scheduler.AddJob(detail, false);
                Console.WriteLine("ScheduleJob: " + detail.Key);

                var triggers = new List<ITrigger>();

                foreach (string cron in configurator.Crons)
                {
                    var trigger = new CronTriggerImpl(name + @"." + Guid.NewGuid(), CronshopDefaultGroup, cron)
                                      {
                                          JobKey = detail.Key,
                                          MisfireInstruction = MisfireInstruction.CronTrigger.DoNothing,
                                      };

                    triggers.Add(trigger);

                    Scheduler.ScheduleJob(trigger);
                }

                _jobs.Add(detail.Key, new JobInfo(script, detail, triggers));
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
            KeyValuePair<JobKey, JobInfo>[] keys = _jobs
                .Where(x => x.Value.Script.FullPath == script.FullPath)
                .ToArray();

            foreach (KeyValuePair<JobKey, JobInfo> tuple in keys)
            {
                bool success = Scheduler.DeleteJob(tuple.Value.JobDetail.Key);
                Console.WriteLine("DeleteJob: " + tuple.Value.JobDetail.Key + " " + (success ? "success" : "FAILED"));

                _jobs.Remove(tuple.Key);
            }
        }
    }
}