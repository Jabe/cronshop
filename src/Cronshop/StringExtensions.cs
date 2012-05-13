using System;
using System.Collections.Generic;
using System.Linq;
using Niob.SimpleHtml;
using Quartz;

namespace Cronshop
{
    public static class StringExtensions
    {
        public static string Encode(this string str)
        {
            return Html.Encode(str);
        }

        public static JobKey ToJobKey(this string str)
        {
            string[] parts = str.Split(new[] {'.'}, 2);

            if (parts.Length == 0) throw new ArgumentException("Invalid JobKey.");
            if (parts.Length == 1) return JobKey.Create(parts[0], CronshopScheduler.CronshopDefaultGroup);

            return JobKey.Create(parts[1], parts[0]);
        }
    }
}