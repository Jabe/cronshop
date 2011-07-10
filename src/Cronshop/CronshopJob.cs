using System;
using System.Threading;
using Quartz;

namespace Cronshop
{
    public abstract class CronshopJob : IInterruptableJob, IDisposable
    {
        private readonly CancellationTokenSource _source = new CancellationTokenSource();

        protected CronshopJob()
        {
            CancellationToken = _source.Token;
        }

        protected CancellationToken CancellationToken { get; private set; }

        #region IDisposable Members

        public virtual void Dispose()
        {
            _source.Dispose();
        }

        #endregion

        #region IInterruptableJob Members

        public void Interrupt()
        {
            _source.Cancel();
        }

        public void Execute(JobExecutionContext context)
        {
            context.Result = ExecuteJob(context);
        }

        #endregion

        public abstract object ExecuteJob(JobExecutionContext context);
    }
}