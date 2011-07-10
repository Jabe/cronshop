using System;
using System.IO;
using Niob;
using Niob.SimpleHtml;
using RazorEngine;

namespace Cronshop
{
    public class CronshopContext
    {
        public CronshopContext(HttpRequest request, HttpResponse response)
        {
            Request = request;
            Response = response;
        }

        public HttpResponse Response { get; private set; }
        public HttpRequest Request { get; private set; }

        public virtual void BeginResponse()
        {
            Response.StatusCode = 200;
            Response.ContentType = "text/html";

            Response.ContentStream = new MemoryStream();

            Response.StartHtml("Cronshop");

            MainResponse();
            EndResponse();
        }

        protected virtual void MainResponse()
        {
            AppendTemplate("Hello, @Model.Dave.", new {Dave = "Dave"});
        }

        protected virtual void EndResponse()
        {
            Response.EndHtml();
            Response.Send();
        }

        protected string Encode(string str)
        {
            return str.Encode();
        }

        protected void AppendHtml(string rawHtml, params object[] format)
        {
            Response.AppendHtml(rawHtml, format);
        }

        protected void AppendTemplate(string template, dynamic model = null)
        {
            var writer = new StreamWriter(Response.ContentStream);
            writer.Write(Razor.Parse(template, model));

            writer.Flush();
        }
    }
}