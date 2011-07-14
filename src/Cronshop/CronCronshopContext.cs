using System;
using System.Linq;
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
            AppendHtml(@"<tr>");
            AppendHtml(@"<th style=""width: 1em;""></th>");
            AppendHtml(@"<th style=""text-align: left;"">Name</th>");
            AppendHtml(@"<th style=""text-align: left;"">Last Execution</th>");
            AppendHtml(@"<th style=""text-align: left;"">Next Execution</th>");
            AppendHtml(@"<th style=""text-align: left;"">Last Duration</th>");
            AppendHtml(@"<th style=""text-align: left;"">Last Result</th>");
            AppendHtml(@"</tr>");

            foreach (JobInfo job in Scheduler.Jobs.OrderBy(x => x.FriendlyName))
            {
                string result = (job.LastResult != null) ? job.LastResult.ToString() : "n/a";

                AppendHtml("<tr>");

                AppendHtml("<td>{0}</td>", job.IsRunning ? @"R" : "");
                AppendHtml("<td>{0}</td>", Encode(job.FriendlyName));
                AppendHtml("<td>{0}</td>", Encode(ConvertTime(job.LastStarted)));
                AppendHtml("<td>{0}</td>", Encode(ConvertTime(job.NextExecution)));
                AppendHtml("<td>{0}</td>", Encode(ConvertDuration(job)));
                AppendHtml("<td>{0}</td>", Encode(result));

                AppendHtml("</tr>");
            }

            AppendHtml("</table>");
        }

        private string ConvertDuration(JobInfo info)
        {
            if (info.LastStarted == DateTimeOffset.MinValue) return "n/a";

            return info.LastDuration.ToStringAuto();
        }

        private static string ConvertTime(DateTimeOffset time)
        {
            if (time == DateTimeOffset.MinValue) return "n/a";

            return time.ToLocalTime().ToString("s").Replace("T", " ");
        }
    }
}