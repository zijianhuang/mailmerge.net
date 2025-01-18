using Microsoft.Extensions.Configuration;

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

		public string Host { get; set; }
		public int Port { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public string From { get; set; }
		//public string FromName { get; set; }
		public bool EnableSsl { get; set; }
		public bool EnableTls { get; set; }

	}
}
