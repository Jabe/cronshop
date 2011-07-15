using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using Niob.SimpleRouting;

namespace Cronshop
{
    public static class Token
    {
        private static readonly ConcurrentDictionary<string, string> Store =
            new ConcurrentDictionary<string, string>();

        public static string GetNew()
        {
            var buffer = new byte[8];

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(buffer);
            }

            string token = BitConverter.ToString(buffer).Replace("-", "");

            Store.TryAdd(token, token);

            return token;
        }

        public static bool IsValid(RouteMatch routeMatch)
        {
            if (routeMatch == null) return false;

            string token;

            if (!routeMatch.Values.TryGetValue("token", out token))
                return false;

            return IsValid(token);
        }

        public static bool IsValid(string token)
        {
            return Store.TryRemove(token, out token);
        }
    }
}