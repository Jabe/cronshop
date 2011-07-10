using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Quartz;

namespace Cronshop
{
    public class MainJobMonitor : IJobListener
    {
        private readonly ReadOnlyCollection<JobInfo> _jobResults;

        public MainJobMonitor(ReadOnlyCollection<JobInfo> jobResults)
        {
            _jobResults = jobResults;
        }

        #region IJobListener Members

        public void JobToBeExecuted(JobExecutionContext context)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;

            JobInfo info = GetJobInfo(context);
            info.IsRunning = true;
            info.LastStarted = now;
        }

        public void JobExecutionVetoed(JobExecutionContext context)
        {
        }

        public void JobWasExecuted(JobExecutionContext context, JobExecutionException jobException)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            JobInfo info = GetJobInfo(context);

            info.IsRunning = false;
            info.LastResult = jobException ?? context.Result;
            info.LastEnded = now;
            info.LastDuration = now - info.LastStarted;

            // cleanup instance
            using (context.JobInstance as IDisposable)
            {
            }
        }

        public string Name
        {
            get { return GetType().Name; }
        }

        #endregion

        private JobInfo GetJobInfo(JobExecutionContext context)
        {
            JobInfo info = _jobResults.First(x => x.JobDetail.Name == context.JobDetail.Name);
            return info;
        }
    }
}