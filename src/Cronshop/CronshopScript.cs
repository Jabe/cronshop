using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Cronshop
{
    public class CronshopScript
    {
        private string _friendlyName;

        public CronshopScript(string fullPath)
        {
            FullPath = fullPath;
            ScriptHash = GetScriptHash(FullPath);
        }

        public string Name
        {
            get { return FullPath.Replace('\\', '/'); }
        }

        public string FullPath { get; private set; }
        public string ScriptHash { get; private set; }

        public string FriendlyName
        {
            get { return _friendlyName ?? Name; }
            set { _friendlyName = value; }
        }

        public override string ToString()
        {
            return FriendlyName;
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