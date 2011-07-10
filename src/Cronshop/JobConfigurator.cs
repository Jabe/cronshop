using System;
using System.Collections.Generic;

namespace Cronshop
{
    public class JobConfigurator
    {
        public JobConfigurator()
        {
            Crons = new List<string>();
        }

        public IList<string> Crons { get; private set; }

        public JobConfigurator AddCron(string cron)
        {
            Crons.Add(cron);
            return this;
        }
    }
}