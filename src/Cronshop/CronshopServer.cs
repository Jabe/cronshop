using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Niob;
using Niob.SimpleRouting;

namespace Cronshop
{
    public class CronshopServer : NiobServer
    {
        public CronshopServer()
        {
            Router = new Router
                          {
                              {"root", "/"},
                              {"cron", "/cron/{action}/{token}/{id}"},
                              {"all", "*"},
                          };

            RequestAccepted += IncomingRequestAccepted;
        }

        public Router Router { get; private set; }
        public CronshopScheduler Scheduler { get; set; }

        private void IncomingRequestAccepted(object sender, RequestEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(_ => HandleRequest(e.Request, e.Response));
        }

        protected virtual void HandleRequest(HttpRequest request, HttpResponse response)
        {
            RouteMatch rm = Router.GetFirstHit(request.Url);

            CronshopContext context;

            if (rm.Route.Name == "cron")
            {
                context = new CronCronshopContext(request, response) {Scheduler = Scheduler};
            }
            else
            {
                context = new CronshopContext(request, response);
            }

            context.RouteMatch = rm;
            context.BeginResponse();
        }
    }
}