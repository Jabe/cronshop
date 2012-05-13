using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Quartz;

namespace Cronshop
{
    public class JobInfo
    {
        public JobInfo(CronshopScript script, IJobDetail jobDetail, ICollection<ITrigger> triggers)
        {
            Script = script;
            JobDetail = jobDetail;
            Triggers = triggers;
        }

        public CronshopScript Script { get; private set; }
        public IJobDetail JobDetail { get; private set; }
        public ICollection<ITrigger> Triggers { get; private set; }

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

                return Triggers.Min(x => x.GetFireTimeAfter(DateTime.UtcNow) ?? DateTime.MinValue);
            }
        }

        internal static string BuildJobName(CronshopScript script, Type type)
        {
            return Path.GetFileNameWithoutExtension(script.FullPath) + @"." + type.Name;
        }
    }
}