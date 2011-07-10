using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Cronshop.Catalogs;
using Niob;

namespace Cronshop.Shell
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var binding = new Binding(IPAddress.Loopback, 8080) {SupportsKeepAlive = false};
            var server = new CronshopServer();
            server.Bindings.Add(binding);

            server.Start();

            var catalog = new DirectoryCatalog(@"..\..\..\..\jobs");
            var scheduler = new CronshopScheduler(catalog);

            server.Scheduler = scheduler;

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
            server.Scheduler = null;
            catalog.Dispose();

            server.Dispose();
        }
    }
}