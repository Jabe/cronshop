using System;
using System.Collections.Generic;
using System.Linq;
using Quartz;

namespace Cronshop
{
    public class MainJobMonitor : IJobListener
    {
        private readonly IDictionary<JobKey, JobInfo> _jobInfo;

        public MainJobMonitor(IDictionary<JobKey, JobInfo> jobInfo)
        {
            _jobInfo = jobInfo;
        }

        #region IJobListener Members

        public void JobToBeExecuted(IJobExecutionContext context)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;

            JobInfo info = GetJobInfo(context);
            info.IsRunning = true;
            info.LastStarted = now;
        }

        public void JobExecutionVetoed(IJobExecutionContext context)
        {
        }

        public void JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
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

        private JobInfo GetJobInfo(IJobExecutionContext context)
        {
            JobInfo info;
            _jobInfo.TryGetValue(context.JobDetail.Key, out info);
            return info;
        }
    }
}