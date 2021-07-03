using Microsoft.ApplicationInsights;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace SamirBoulema.TSVN.Helpers
{
    public static class LogHelper
    {
        private static TelemetryClient _client;
        private const string InstrumentationKey = "3baac5da-a1cb-461f-a82b-ff3d96ddef68";

        private static void GetClient()
        {
            _client = new TelemetryClient();
            _client.Context.Session.Id = Guid.NewGuid().ToString();
            _client.InstrumentationKey = InstrumentationKey;
            _client.Context.Component.Version = GetExecutingAssemblyVersion().ToString();

            var enc = Encoding.UTF8.GetBytes(Environment.UserName + Environment.MachineName);
            using (var crypto = new MD5CryptoServiceProvider())
            {
                var hash = crypto.ComputeHash(enc);
                _client.Context.User.Id = Convert.ToBase64String(hash);
            }
        }

        public static void Log(Exception e)
        {
            if (_client == null)
            {
                GetClient();
            }

            _client.TrackException(e);
        }

        public static void Log(Exception e, object context)
        {
            if (_client == null)
            {
                GetClient();
            }

            var properties = new Dictionary<string, string>
            {
                { "version", GetExecutingAssemblyVersion().ToString() },
                { "context", JsonConvert.SerializeObject(context) }
            };

            _client.TrackException(e, properties);
        }

        public static void Log(string message, Exception e)
        {
            if (_client == null)
            {
                GetClient();
            }

            var properties = new Dictionary<string, string>
            {
                { "version", GetExecutingAssemblyVersion().ToString() },
                { "message", message }
            };

            _client.TrackException(e, properties);
        }

        public static void Log(object log)
        {
            if (_client == null)
            {
                GetClient();
            }

            var properties = new Dictionary<string, string>
            {
                { "version", GetExecutingAssemblyVersion().ToString() }
            };

            _client.TrackEvent(JsonConvert.SerializeObject(log), properties);
        }

        private static Version GetExecutingAssemblyVersion()
        {
            var ver = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

            // read what's defined in [assembly: AssemblyFileVersion("1.2.3.4")]
            return new Version(ver.ProductMajorPart, ver.ProductMinorPart, ver.ProductBuildPart, ver.ProductPrivatePart);
        }
    }
}
