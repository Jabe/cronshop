using System;
using System.Linq;
using Quartz;

namespace Cronshop
{
    public class JobInfo
    {
        private TimeSpan _lastDuration;
        private DateTimeOffset _lastEnded;

        public JobInfo(CronshopScript script, JobDetail jobDetail, Trigger[] triggers)
        {
            Script = script;
            JobDetail = jobDetail;
            Triggers = triggers;
        }

        public CronshopScript Script { get; private set; }
        public JobDetail JobDetail { get; private set; }
        public Trigger[] Triggers { get; private set; }

        public bool IsRunning { get; set; }
        public object LastResult { get; set; }

        public DateTimeOffset LastStarted { get; set; }
        public DateTimeOffset LastEnded { get; set; }
        public TimeSpan LastDuration { get; set; }

        public DateTimeOffset NextExecution
        {
            get
            {
                if (Triggers.Length == 0) return DateTimeOffset.MinValue;

                DateTime time = Triggers.Min(x => x.GetFireTimeAfter(DateTime.UtcNow) ?? DateTime.MinValue);
                return new DateTimeOffset(time, TimeSpan.Zero);
            }
        }
    }
}