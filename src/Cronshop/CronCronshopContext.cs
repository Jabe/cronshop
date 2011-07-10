using System;
using Niob;

namespace Cronshop
{
    public class CronCronshopContext : CronshopContext
    {
        public CronCronshopContext(HttpRequest request, HttpResponse response) : base(request, response)
        {
        }

        public CronshopScheduler Scheduler { get; set; }

        protected override void MainResponse()
        {
            AppendHtml(@"<h1>Cron</h1>");

            if (Scheduler == null)
            {
                AppendHtml(@"No scheduler.");
                return;
            }

            AppendHtml(@"<table style=""width: 100%;"">");
            AppendHtml(@"<tr style=""text-align: left;"">");
            AppendHtml(@"<th style=""width: 1em;""></th><th>Name</th><th>Last Started</th>");
            AppendHtml(@"<th>Last Duration</th><th>Next Exec</th><th>Last Result</th>");
            AppendHtml(@"</tr>");

            foreach (JobInfo job in Scheduler.Jobs)
            {
                string result = (job.LastResult != null) ? job.LastResult.ToString() : "n/a";

                AppendHtml("<tr>");

                AppendHtml("<td>{0}</td>", job.IsRunning ? @"R" : "");
                AppendHtml("<td>{0}</td>", Encode(job.JobDetail.Name));
                AppendHtml("<td>{0}</td>", Encode(job.LastStarted.ToString()));
                AppendHtml("<td>{0}</td>", Encode(job.LastDuration.ToString()));
                AppendHtml("<td>{0}</td>", Encode(job.NextExecution.ToString()));
                AppendHtml("<td>{0}</td>", Encode(result));

                AppendHtml("</tr>");
            }

            AppendHtml("</table>");
        }
    }
}