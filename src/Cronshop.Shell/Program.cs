using System;
using System.Net;
using Cronshop.Catalogs;
using Niob;

namespace Cronshop.Shell
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            IScriptCatalog catalog = new DirectoryCatalog(@"..\..\..\..\jobs");
            var server = new CronshopServer {Bindings = {new Binding(IPAddress.Loopback, 8080)}};
            var system = new CronshopSystem(catalog, server);

            using (catalog)
            using (server)
            using (system)
            {
                system.StartAll();

                string line = "";

                while (line != "quit")
                {
                    Console.WriteLine("commands:");
                    Console.WriteLine("quit    pauseall    resumeall");

                    line = Console.ReadLine();

                    if (line == "pauseall")
                    {
                        system.PauseJobs();
                    }
                    else if (line == "resumeall")
                    {
                        system.ResumeJobs();
                    }
                }

                system.StopAll();
            }
        }
    }
}