using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Cronshop
{
    public class CronshopScript
    {
        public CronshopScript(string fullPath)
        {
            FullPath = fullPath;
            ScriptHash = GetScriptHash(FullPath);
        }

        public string FullPath { get; private set; }
        public string ScriptHash { get; private set; }

        public override string ToString()
        {
            return FullPath;
        }

        private static string GetScriptHash(string fullPath)
        {
            string content;

            try
            {
                using (FileStream file = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new StreamReader(file))
                {
                    content = reader.ReadToEnd();
                }
            }
            catch
            {
                content = "";
            }

            using (SHA1 algo = SHA1.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(content);
                byte[] hash = algo.ComputeHash(bytes);

                return BitConverter
                    .ToString(hash)
                    .ToLowerInvariant()
                    .Replace("-", "");
            }
        }
    }
}