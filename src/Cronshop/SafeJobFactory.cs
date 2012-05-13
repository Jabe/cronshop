using System;
using Quartz;
using Quartz.Spi;

namespace Cronshop
{
    public class SafeJobFactory : IJobFactory
    {
        #region IJobFactory Members

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            try
            {
                return (IJob) Activator.CreateInstance(bundle.JobDetail.JobType);
            }
            catch
            {
            }

            throw new InvalidOperationException("Cannot construct job of type " + bundle.JobDetail.GetType().FullName);
        }

        #endregion
    }
}