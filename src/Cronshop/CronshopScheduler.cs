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
        private const string InternalScriptKey = "__InternalScript";
        private readonly IScriptCatalog _catalog;
        private readonly List<JobInfo> _jobs = new List<JobInfo>();
        private readonly ReadOnlyCollection<JobInfo> _readOnlyJobs;
        private readonly IScheduler _scheduler;

        static CronshopScheduler()
        {
            // for now -- this is less painful on temp files and file locks.
            CSScript.CacheEnabled = false;
            CSScript.KeepCompilingHistory = false;
            CSScript.ShareHostRefAssemblies = false;
            CSScript.GlobalSettings.InMemoryAsssembly = true;
        }

        public CronshopScheduler(IScriptCatalog catalog)
        {
            _catalog = catalog;
            _catalog.CatalogChanged += CatalogChanged;

            _readOnlyJobs = new ReadOnlyCollection<JobInfo>(_jobs);

            ISchedulerFactory factory = new StdSchedulerFactory();
            _scheduler = factory.GetScheduler();
            _scheduler.JobFactory = new SafeJobFactory();
            _scheduler.AddGlobalJobListener(new MainJobMonitor(_readOnlyJobs));

            LoadJobsFromCatalog();
        }

        public ReadOnlyCollection<JobInfo> Jobs
        {
            get { return _readOnlyJobs; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Stop();

            _catalog.CatalogChanged -= CatalogChanged;
        }

        #endregion

        private void CatalogChanged(object sender, CatalogChangedEventArgs e)
        {
            Console.WriteLine("event");

            foreach (var change in e.Changes)
            {
                Console.Write("change: " + change.Item1);
                Console.WriteLine(" -> " + change.Item2);
            }

            while (e.Changes.Count > 0)
            {
                var change = e.Changes.Dequeue();

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
            Assembly assembly = SaveLoad(script);

            if (assembly == null)
            {
                return;
            }

            Console.WriteLine("ScheduleScript: " + script);

            // find implementations of CronshopJob
            IEnumerable<Type> types = assembly.GetTypes()
                .Where(t => typeof (CronshopJob).IsAssignableFrom(t));

            foreach (Type type in types)
            {
                string name = script.FullPath;

                var detail = new JobDetail(name, null, type);
                detail.JobDataMap[InternalScriptKey] = script;

                var trigger = new CronTrigger(name + "-" + Guid.NewGuid(), null, "*/5 * * * * ?");

                var triggers = new Trigger[] {trigger};
                _jobs.Add(new JobInfo(script, detail, triggers));

                _scheduler.ScheduleJob(detail, trigger);
            }
        }

        private static Assembly SaveLoad(CronshopScript script)
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
            bool success = _scheduler.DeleteJob(script.FullPath, null);
            Console.WriteLine("UnscheduleScript: " + script.FullPath + " = " + success);
        }

        public void Start()
        {
            _scheduler.Start();
        }

        public void Stop()
        {
            if (!_scheduler.IsShutdown)
            {
                _scheduler.Shutdown(true);
            }
        }

        public void InterruptJob(string jobName)
        {
            _scheduler.Interrupt(jobName, null);
        }

        public void Pause(string jobName = null)
        {
            if (jobName == null)
            {
                _scheduler.PauseAll();
            }
            else
            {
                _scheduler.PauseJob(jobName, null);
            }
        }

        public void Resume(string jobName = null)
        {
            if (jobName == null)
            {
                _scheduler.ResumeAll();
            }
            else
            {
                _scheduler.ResumeJob(jobName, null);
            }
        }
    }
}