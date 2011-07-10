using System;
using System.Collections.Generic;
using System.Linq;
using Cronshop.Catalogs;

namespace Cronshop.Shell
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var catalog = new DirectoryCatalog(@"..\..\..\..\jobs");
            var scheduler = new CronshopScheduler(catalog);

            scheduler.Start();

            string line = "";

            while (line != "quit")
            {
                Console.WriteLine("commands:");
                Console.WriteLine("quit    pauseall    resumeall");

                line = Console.ReadLine();

                Console.WriteLine(scheduler.Jobs.First().IsRunning);

                if (line == "pauseall")
                {
                    scheduler.Pause();
                }
                else if (line == "resumeall")
                {
                    scheduler.Resume();
                }
            }

            // stop
            scheduler.Dispose();
            catalog.Dispose();
        }
    }
}