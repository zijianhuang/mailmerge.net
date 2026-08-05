using Microsoft.Extensions.Configuration;
using System;

namespace Fonlow.Mail
{
    public sealed class SmtpSection
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config">Expect IConfigurationRoot generally from appsetting.json</param>
        public SmtpSection(IConfiguration config)
        {
            config.GetSection("mailSettings").Bind("smtp", this);
        }

        string host;
        public string Host
        {
            get { return host; }
            set
            {
                if (OperatingSystem.IsWindows() && !string.IsNullOrEmpty(value) && value.StartsWith("{{") && value.EndsWith("}}"))
                {
                    var ev = value.Substring(2, value.Length - 4);
                    host = Environment.GetEnvironmentVariable(ev);
                }
                else
                {
                    host = value;
                }
            }
        }

        public int Port { get; set; }

        string username;
        public string Username
        {
            get { return username; }
            set
            {
                if (OperatingSystem.IsWindows() && !string.IsNullOrEmpty(value) && value.StartsWith("{{") && value.EndsWith("}}"))
                {
                    var ev = value.Substring(2, value.Length - 4);
                    username = Environment.GetEnvironmentVariable(ev);
                }
                else
                {
                    username = value;
                }
            }
        }


        string password;
        public string Password
        {
            get { return password; }
            set
            {
                if (OperatingSystem.IsWindows() && !string.IsNullOrEmpty(value) && value.StartsWith("{{") && value.EndsWith("}}"))
                {
                    var ev = value.Substring(2, value.Length - 4);
                    password = Environment.GetEnvironmentVariable(ev);
                }
                else
                {
                    password = value;
                }
            }
        }

        string from;
        public string From
        {
            get { return from; }
            set
            {
                if (OperatingSystem.IsWindows() && !string.IsNullOrEmpty(value) && value.StartsWith("{{") && value.EndsWith("}}"))
                {
                    var ev = value.Substring(2, value.Length - 4);
                    from = Environment.GetEnvironmentVariable(ev);
                }
                else
                {
                    from = value;
                }
            }
        }

        public bool EnableSsl { get; set; }
        public bool EnableTls { get; set; }

        /// <summary>
        /// If declared, protocol log will be available.
        /// </summary>
        public string ProtocolLogFile { get; set; }
	}
}
