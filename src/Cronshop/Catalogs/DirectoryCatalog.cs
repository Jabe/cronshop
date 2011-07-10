using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;

namespace Cronshop.Catalogs
{
    public class DirectoryCatalog : ScriptCatalogBase, IDisposable
    {
        private const int ForceScanEvery = 60*1000;
        private const int DeferWatcherEventsFor = 100;

        private readonly object _syncLock = new object();
        private readonly object _watcherTimerLock = new object();

        private CronshopScript[] _scripts;
        private Timer _timer;
        private FileSystemWatcher _watcher;
        private Timer _watcherTimer;

        public DirectoryCatalog(string path,
                                string searchPattern = "*.ccs",
                                SearchOption searchOption = SearchOption.AllDirectories,
                                bool watchChanges = true)
        {
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException("Catalog directory not found: " + Path);
            }

            Path = System.IO.Path.GetFullPath(path);
            SearchPattern = searchPattern;
            SearchOption = searchOption;
            WatchChanges = watchChanges;

            if (watchChanges)
            {
                _watcherTimer = new Timer(DeferWatcherEventsFor) {AutoReset = false};
                _watcherTimer.Elapsed += TimerElapsed;

                _watcher = new FileSystemWatcher(Path)
                               {
                                   IncludeSubdirectories = (SearchOption == SearchOption.AllDirectories),
                                   EnableRaisingEvents = true,
                                   Filter = SearchPattern,
                               };

                // increase buffer size (pretty expensive -- don't go too big)
                // default is 8k which easily gets full when pasting ~30 files ...
                // the buffer holds file names so this number may varies greatly.
                // if the buffer is full, we'll miss events.
                // 64k should be enough :-)
                _watcher.InternalBufferSize = 0x10000;

                _watcher.Created += DirectoryChanged;
                _watcher.Changed += DirectoryChanged;
                _watcher.Deleted += DirectoryChanged;
                _watcher.Renamed += DirectoryChanged;
            }

            _timer = new Timer(ForceScanEvery) {AutoReset = true};
            _timer.Elapsed += TimerElapsed;

            _scripts = LoadScriptsFromPath();

            _timer.Start();
        }

        public string Path { get; private set; }
        public string SearchPattern { get; private set; }
        public SearchOption SearchOption { get; private set; }
        public bool WatchChanges { get; private set; }

        public override IEnumerable<CronshopScript> Scripts
        {
            get { return _scripts; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            // tidy up events too, helps gc.
            if (_watcherTimer != null)
            {
                _watcherTimer.Elapsed -= TimerElapsed;

                _watcherTimer.Dispose();
                _watcherTimer = null;
            }

            if (_watcher != null)
            {
                _watcher.Created -= DirectoryChanged;
                _watcher.Changed -= DirectoryChanged;
                _watcher.Deleted -= DirectoryChanged;
                _watcher.Renamed -= DirectoryChanged;

                _watcher.Dispose();
                _watcher = null;
            }

            if (_timer != null)
            {
                _timer.Elapsed -= TimerElapsed;

                _timer.Dispose();
                _timer = null;
            }
        }

        #endregion

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            SyncScripts();
        }

        private void DirectoryChanged(object sender, FileSystemEventArgs e)
        {
            lock (_watcherTimerLock)
            {
                // restart the timer
                _watcherTimer.Stop();
                _watcherTimer.Start();
            }
        }

        private CronshopScript[] LoadScriptsFromPath()
        {
            return Directory
                .EnumerateFiles(Path, SearchPattern, SearchOption)
                .Select(path => new CronshopScript(path))
                .ToArray();
        }

        private void SyncScripts()
        {
            lock (_syncLock)
            {
                CronshopScript[] oldScripts = _scripts;
                CronshopScript[] newScripts = LoadScriptsFromPath();

                // the following stuff should be optimized...
                string[] oldPaths = oldScripts.Select(x => x.FullPath).ToArray();
                string[] newPaths = newScripts.Select(x => x.FullPath).ToArray();

                var diff = new Queue<Tuple<CronshopScript, CatalogChange>>();

                // find deleted scripts
                foreach (CronshopScript script in oldScripts)
                {
                    if (!newPaths.Contains(script.FullPath))
                    {
                        diff.Enqueue(Tuple.Create(script, CatalogChange.Deleted));
                    }
                }

                // find new scripts
                foreach (CronshopScript script in newScripts)
                {
                    if (!oldPaths.Contains(script.FullPath))
                    {
                        diff.Enqueue(Tuple.Create(script, CatalogChange.Created));
                    }
                }

                // find modified scripts
                foreach (CronshopScript script in oldScripts)
                {
                    if (newPaths.Contains(script.FullPath))
                    {
                        if (script.ScriptHash != newScripts.First(x => x.FullPath == script.FullPath).ScriptHash)
                        {
                            diff.Enqueue(Tuple.Create(script, CatalogChange.Modified));
                        }
                    }
                }

                if (diff.Count > 0)
                {
                    _scripts = newScripts;

                    OnCatalogChanged(new CatalogChangedEventArgs(diff));
                }
            }
        }

        public override event EventHandler<CatalogChangedEventArgs> CatalogChanged;

        protected override void OnCatalogChanged(CatalogChangedEventArgs args)
        {
            EventHandler<CatalogChangedEventArgs> handler = CatalogChanged;

            if (handler != null)
            {
                handler(this, args);
            }
        }
    }
}