using System;
using System.Collections.Generic;
using System.Linq;
using Niob.SimpleHtml;

namespace Cronshop
{
    public static class StringExtensions
    {
        public static string Encode(this string str)
        {
            return Html.Encode(str);
        }
    }
}