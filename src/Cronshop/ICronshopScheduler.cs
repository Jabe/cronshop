using System;
using System.Collections.Generic;
using Quartz;

namespace Cronshop
{
    public interface ICronshopScheduler : IDisposable
    {
        IEnumerable<JobInfo> Jobs { get; }

        void Start();
        void Stop();
        object ExecuteJob(JobKey jobKey);
        void InterruptJob(JobKey jobKey);
        void Pause(JobKey jobKey = null);
        void Resume(JobKey jobKey = null);
    }
}