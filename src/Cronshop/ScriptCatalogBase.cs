using System;
using System.Collections.Generic;

namespace Cronshop
{
    public abstract class ScriptCatalogBase : IScriptCatalog
    {
        #region IScriptCatalog Members

        public abstract IEnumerable<CronshopScript> Scripts { get; }
        public abstract event EventHandler<CatalogChangedEventArgs> CatalogChanged;
        public abstract void Dispose();

        #endregion

        protected abstract void OnCatalogChanged(CatalogChangedEventArgs args);
    }
}