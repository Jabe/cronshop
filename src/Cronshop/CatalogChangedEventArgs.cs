using System;
using System.Collections.Generic;

namespace Cronshop
{
    public class CatalogChangedEventArgs : EventArgs
    {
        public CatalogChangedEventArgs(Queue<Tuple<CronshopScript, CatalogChange>> changes)
        {
            Changes = changes;
        }

        public Queue<Tuple<CronshopScript, CatalogChange>> Changes { get; private set; }
    }
}