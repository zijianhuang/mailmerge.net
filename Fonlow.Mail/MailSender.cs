using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fonlow.Mail
{
	/// <summary>
	/// Category of ILogger
	/// </summary>
	public class EmailTrace
	{
	}

	/// <summary>
	/// SmtpClieent static wrapper
	/// </summary>
	public class MailSender : IDisposable
	{
		public MailSender(SmtpSection smtpSection, ILogger<EmailTrace> logger)
		{
			this.logger = logger;
			this.smtpSection = smtpSection;
			this.smtpClient = new SmtpClient();
			smtpClient.Disconnected += (object sender, DisconnectedEventArgs e) =>
			{
				logger.LogInformation("SmtpClient Disconnected");
			};
		}

		public MailSender(SmtpSection smtpSection, ILogger logger)
		{
			this.logger = logger;
			this.smtpSection = smtpSection;
			this.smtpClient = new SmtpClient();
			smtpClient.Disconnected += (object sender, DisconnectedEventArgs e) =>
			{
				logger.LogInformation("SmtpClient Disconnected");
			};
		}

		readonly SmtpSection smtpSection;
		readonly ILogger logger;
		readonly SmtpClient smtpClient;
		readonly SemaphoreSlim smtpClientLock = new SemaphoreSlim(1, 1);

		/// <summary>
		/// Connect to Email server, authenticate, send, then dispose message.
		/// </summary>
		/// <param name="message">It is up to the client to dispose the message</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public async Task<string> SendAsync(MimeMessage message)
		{
			ArgumentNullException.ThrowIfNull(message);

			async Task<string> SendMessageAsync()
			{
				if (message.From.Count == 0)
				{
					message.From.Add(MailboxAddress.Parse(smtpSection.From));
				}

				logger.LogInformation("Sending Email {Subject} to {Recipient}", message.Subject, string.Join("; ", message.To.Mailboxes.Select(d => string.IsNullOrEmpty(d.Name) ? d.Address : $"{d.Name} <{d.Address}>")));
				var r = await smtpClient.SendAsync(message).ConfigureAwait(false);
				logger.LogInformation("Sent Email {Subject}", message.Subject);
				return r;
			}

			await smtpClientLock.WaitAsync().ConfigureAwait(false);
			var resendOnce = false;
			try
			{
				await EnsureSmtpClientAuthenticated().ConfigureAwait(false);
				return await SendMessageAsync().ConfigureAwait(false);
			}
			catch (SmtpCommandException ex)
			{
				logger.LogError(ex.ToString());

				if (ex.Message.Contains("Throttling failure")) // Throttling failure: Daily message quota exceeded.
				{
					throw;
				}

				await EnsureSmtpClientAuthenticated().ConfigureAwait(false);
				if (!resendOnce)
				{
					logger.LogInformation("Try sending mail once again after reconnecting and authentication...");
					resendOnce = true;
					return await SendMessageAsync().ConfigureAwait(false);
				}
				else
				{
					return "Failed to send even after resend once.";
				}
			}
			catch (SmtpProtocolException ex)
			{
				logger.LogError(ex.ToString());
				throw;
			}
			catch (AuthenticationException ex)
			{
				logger.LogError(ex.Message);
				throw;
			}
			catch (Exception ex)
			{
				logger.LogError($"{ex.GetType().FullName} ~ {ex}");
				throw;
			}
			finally
			{
				smtpClientLock.Release();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		/// <remarks>May throws MailKit.Security.AuthenticationException</remarks>
		async Task EnsureSmtpClientAuthenticated()
		{
			SecureSocketOptions options = SecureSocketOptions.None;
			if (smtpSection.EnableSsl)
			{
				options |= SecureSocketOptions.SslOnConnect;
			}

			if (smtpSection.EnableTls)
			{
				options |= SecureSocketOptions.StartTls;
			}

			if (!smtpClient.IsConnected)
			{
				logger.LogDebug("SmtpClient Connecting...");
				await smtpClient.ConnectAsync(smtpSection.Host, smtpSection.Port, options).ConfigureAwait(false);
				logger.LogDebug("SmtpClient Connected.");
			}

			if (!smtpClient.IsAuthenticated)
			{
				logger.LogDebug("SmtpClient Authenticating...");
				await smtpClient.AuthenticateAsync(smtpSection.Username, smtpSection.Password).ConfigureAwait(false);
				logger.LogDebug("SmtpClient Authenticated");
			}
		}

		/// <summary>
		/// A From value from app.config at system.net/mailSettings/smtp
		/// </summary>
		public string From => smtpSection.From;
		//public string FromName => smtpSection.FromName;

		#region IDisposable pattern
		bool disposed = false;

		void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					smtpClient.Dispose();
				}

				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
		#endregion

	}

}
