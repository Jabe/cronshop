using System;
using System.Collections.Generic;
using System.Linq;
using Quartz;

namespace Cronshop
{
    public class JobInfo
    {
        public JobInfo(CronshopScript script, JobDetail jobDetail, ICollection<Trigger> triggers)
        {
            Script = script;
            JobDetail = jobDetail;
            Triggers = triggers;
        }

        public string FriendlyName
        {
            get { return (string) JobDetail.JobDataMap[CronshopScheduler.InternalFriendlyName]; }
        }

        public CronshopScript Script { get; private set; }
        public JobDetail JobDetail { get; private set; }
        public ICollection<Trigger> Triggers { get; private set; }

        public bool IsRunning { get; set; }
        public object LastResult { get; set; }

        public DateTimeOffset LastStarted { get; set; }
        public DateTimeOffset LastEnded { get; set; }
        public TimeSpan LastDuration { get; set; }

        public DateTimeOffset NextExecution
        {
            get
            {
                if (Triggers.Count == 0) return DateTimeOffset.MinValue;

                DateTime time = Triggers.Min(x => x.GetFireTimeAfter(DateTime.UtcNow) ?? DateTime.MinValue);
                return new DateTimeOffset(time, TimeSpan.Zero);
            }
        }
    }
}