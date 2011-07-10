using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Niob;

namespace Cronshop
{
    public class CronshopServer : NiobServer
    {
        public CronshopServer()
        {
            RequestAccepted += IncomingRequestAccepted;
        }

        public CronshopScheduler Scheduler { get; set; }

        private void IncomingRequestAccepted(object sender, RequestEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(_ => HandleRequest(e.Request, e.Response));
        }

        protected virtual void HandleRequest(HttpRequest request, HttpResponse response)
        {
            CronshopContext context;

            if (request.Uri.StartsWith("/cron", StringComparison.OrdinalIgnoreCase))
            {
                context = new CronCronshopContext(request, response) {Scheduler = Scheduler};
            }
            else
            {
                context = new CronshopContext(request, response);
            }

            context.BeginResponse();
        }
    }
}