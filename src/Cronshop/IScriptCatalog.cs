using System;
using System.Collections.Generic;

namespace Cronshop
{
    public interface IScriptCatalog
    {
        IEnumerable<CronshopScript> Scripts { get; }
        event EventHandler<CatalogChangedEventArgs> CatalogChanged;
    }
}