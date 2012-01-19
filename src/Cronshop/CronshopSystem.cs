using System;

namespace Cronshop
{
    public class CronshopSystem : IDisposable
    {
        private readonly IScriptCatalog _catalog;
        private readonly ICronshopScheduler _scheduler;
        private readonly CronshopServer _server;
        private bool _disposed;

        public CronshopSystem(IScriptCatalog catalog, CronshopServer server = null)
        {
            _server = server;
            _catalog = catalog;

            _scheduler = new CronshopScheduler(catalog);

            if (_server != null)
            {
                // todo: nicer interface
                _server.Scheduler = _scheduler;
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            if (_server != null)
            {
                // todo: nicer interface
                _server.Scheduler = null;
            }

            _scheduler.Stop();
            _scheduler.Dispose();

            _disposed = true;
        }

        #endregion

        /// <summary>
        /// Starts all available components.
        /// </summary>
        public void StartAll()
        {
            if (_server != null)
            {
                _server.Start();
            }

            _scheduler.Start();
        }

        /// <summary>
        /// Stops all available components.
        /// </summary>
        public void StopAll()
        {
            if (_server != null)
            {
                _server.Stop();
            }

            _scheduler.Stop();
        }

        /// <summary>
        /// Pauses all jobs.
        /// </summary>
        public void PauseJobs()
        {
            _scheduler.Pause();
        }

        /// <summary>
        /// Resumes all jobs.
        /// </summary>
        public void ResumeJobs()
        {
            _scheduler.Resume();
        }
    }
}