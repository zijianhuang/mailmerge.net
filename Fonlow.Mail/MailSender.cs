using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Collections.Concurrent;

namespace Fonlow.Mail
{
	/// <summary>
	/// SmtpClieent static wrapper
	/// </summary>
	public class MailSender
	{
		public MailSender(SmtpSection smtpSection)
		{
			this.smtpSection = smtpSection;
		}

		readonly SmtpSection smtpSection;

		/// <summary>
		/// Send, then dispose message.
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public async Task<string> SendAsync(MimeMessage msg)
		{
			if (msg == null)
			{
				throw new ArgumentNullException(nameof(msg));
			}

			try
			{
				using var client = new SmtpClient();
				SecureSocketOptions options= SecureSocketOptions.None;
				if (smtpSection.EnableSsl)
				{
					options |= SecureSocketOptions.SslOnConnect;
				}

				if (smtpSection.EnableTls)
				{
					options |= SecureSocketOptions.StartTls;
				}

				await client.ConnectAsync(smtpSection.Host, smtpSection.Port, MailKit.Security.SecureSocketOptions.StartTls);
				await client.AuthenticateAsync(smtpSection.UserName, smtpSection.Password);
				if (msg.From.Count == 0)
				{
					msg.From.Add(MailboxAddress.Parse(smtpSection.From));
				}
				client.Disconnected += (object sender, DisconnectedEventArgs e) =>
				{
					msg.Dispose();
				};
				return await client.SendAsync(msg);

			}
			catch (SmtpCommandException ex)
			{
				System.Diagnostics.Trace.TraceWarning(ex.ToString());
				throw;
			}
			catch (SmtpProtocolException ex)
			{
				System.Diagnostics.Trace.TraceWarning(ex.ToString());
				throw;
			}
			catch (Exception ex)
			{
				string s = ex.ToString();
				System.Diagnostics.Trace.TraceError(s);
				System.Diagnostics.Trace.TraceInformation("Throw again: " + ex.GetType().FullName);
				throw;
			}
		}

		/// <summary>
		/// A From value from app.config at system.net/mailSettings/smtp
		/// </summary>
		public string From => smtpSection.From;
		//public string FromName => smtpSection.FromName;

	}

	/// <summary>
	/// Queue mail for sending later. If mail system has problems, only 100 messages will be stored.
	/// This class is intend to be use in scenarios where not being able to send mail is not a critical problem. And fire and forget is more important.
	/// </summary>
	public sealed class MailQueue : IDisposable
	{
		readonly ConcurrentQueue<MimeMessage> pendingQueue;
		public MailQueue(MailSender sender)
		{
			this.sender = sender;
			pendingQueue = new ConcurrentQueue<MimeMessage>();
			timer = new Timer(TimerCallback, null, 1000, Timeout.Infinite);
		}

		readonly MailSender sender;
		readonly Timer timer;

		public void Pend(MimeMessage tm)
		{
			if (pendingQueue.Count >= 100)
			{
				System.Diagnostics.Trace.TraceError("There could be serious problem in the mail system since the MailQueue has 100 messages pending.");
				return;
			}

			pendingQueue.Enqueue(tm);
		}

		async Task SendAsync()
		{
			while (pendingQueue.TryDequeue(out MimeMessage tm))
			{
				try
				{
					await sender.SendAsync(tm);
				}
				catch (CommandException)
				{
					//do nothing
				}
				finally
				{
					tm.Dispose();
				}
			}
		}


		async void TimerCallback(Object stateInfo)
		{
			await SendAsync();
			timer.Change(1000, Timeout.Infinite);// 1 second is a good number, with optimal performance, 0.5 second does not make performance noticablly better.
		}

		public string From => sender.From;
		//public string FromName => sender.FromName;

		#region IDisposable pattern
		bool disposed = false;

		void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					timer.Dispose();
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
