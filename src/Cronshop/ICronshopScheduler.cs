using System;
using System.Collections.Generic;

namespace Cronshop
{
    public interface ICronshopScheduler : IDisposable
    {
        IEnumerable<JobInfo> Jobs { get; }

        void Start();
        void Stop();
        object ExecuteJob(string jobName);
        void InterruptJob(string jobName);
        void Pause(string jobName = null);
        void Resume(string jobName = null);
    }
}