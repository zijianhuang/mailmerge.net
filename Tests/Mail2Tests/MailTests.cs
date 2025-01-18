using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fonlow.Mail;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;

namespace MailUnitTests
{
	public class MailTests : IClassFixture<MailFixture>
	{
		public MailTests(MailFixture fixture)
		{
			mailQueue = fixture.ServiceProvider.GetService<MailQueue>();
			sender = fixture.ServiceProvider.GetService<MailSender>();
			config = fixture.Config;
		}

		readonly MailQueue mailQueue;
		readonly MailSender sender;
		readonly IConfiguration config;
#pragma warning disable CA2000 // Dispose objects before losing scope

		[Fact]
		public void TestPend()
		{
			var bodyBuilder = new BodyBuilder();
			bodyBuilder.TextBody = "You will see me.";
			MimeMessage message = new MimeMessage
			{
				Subject = "Pend Just from unit test" + DateTime.Now.ToString(),
				Body = (new BodyBuilder() { TextBody = "You will see me" }).ToMessageBody(),
			};
			message.From.Add(new MailboxAddress(mailQueue.From, mailQueue.From));
			message.To.Add(new MailboxAddress("Stephen", "stephen.strange100@gmail.com"));
			mailQueue.Pend(message);

			System.Threading.Thread.Sleep(2500);
		}

		[Fact]
		public async Task TestSend()
		{
			MimeMessage message = new MimeMessage
			{
				Subject = "Send Just from unit test" + DateTime.Now.ToString(),
				Body = (new BodyBuilder() { TextBody = "You will see me" }).ToMessageBody(),
			};
			message.From.Add(new MailboxAddress(mailQueue.From, mailQueue.From));
			message.To.Add(new MailboxAddress("Stephen", "stephen.strange100@gmail.com"));
			await sender.SendAsync(message);
		}

		[Fact]
		public async Task TestSendWithoutName()
		{
			MimeMessage message = new MimeMessage
			{
				Subject = "Send Just from unit test" + DateTime.Now.ToString(),
				Body = (new BodyBuilder() { TextBody = "You will see me" }).ToMessageBody(),
			};
			message.From.Add(new MailboxAddress(mailQueue.From, mailQueue.From));
			message.To.Add(new MailboxAddress(null, "stephen.strange100@gmail.com"));
			await sender.SendAsync(message);
		}

		[Fact]
		public async Task TestSendWithNameAddress()
		{
			MimeMessage message = new MimeMessage
			{
				Subject = "Send Just from unit test" + DateTime.Now.ToString(),
				Body = (new BodyBuilder() { TextBody = "You will see me" }).ToMessageBody(),
			};
			message.From.Add(new MailboxAddress(mailQueue.From, mailQueue.From));
			message.To.Add(MailboxAddress.Parse("Stephan <stephen.strange100@gmail.com>"));
			await sender.SendAsync(message);
		}

		[Fact]
		public async Task TestSendThrow()
		{
			SmtpSection smtpSection = new SmtpSection(config);
			smtpSection.Password = "XXX"+ smtpSection.Password;
			//smtpSection.Host = "kkkkk";
			MailSender mySender = new MailSender(smtpSection);

			MimeMessage message = new MimeMessage
			{
				Subject = "Send Just from unit test" + DateTime.Now.ToString(),
				Body = (new BodyBuilder() { TextBody = "You will see me" }).ToMessageBody(),
			};

			message.From.Add(new MailboxAddress(mailQueue.From, mailQueue.From));
			message.To.Add(new MailboxAddress("Stephen", "stephen.strange100@gmail.com"));
			await Assert.ThrowsAsync<MailKit.Security.AuthenticationException>(() => mySender.SendAsync(message));
		}

		[Fact]
		public async Task TestSendAsync()
		{
			MimeMessage message = new MimeMessage
			{
				Subject = "SendAsync Just from unit test" + DateTime.Now.ToString(),
				Body = (new BodyBuilder() { TextBody = "You will see me" }).ToMessageBody(),
			};
			message.From.Add(new MailboxAddress(mailQueue.From, mailQueue.From));
			message.To.Add(new MailboxAddress("Stephen", "stephen.strange100@gmail.com"));
			await sender.SendAsync(message);
		}

		[Fact]
		public async Task TestSendAsyncThrow()
		{
			SmtpSection smtpSection = new SmtpSection(config);
			smtpSection.Password = "XXX" + smtpSection.Password;
			//smtpSection.Host = "kkkkk";
			MailSender mySender = new MailSender(smtpSection);

			MimeMessage message = new MimeMessage
			{
				Subject = "Send Just from unit test" + DateTime.Now.ToString(),
				Body = (new BodyBuilder() { TextBody = "You will see me" }).ToMessageBody(),
			};

			message.From.Add(new MailboxAddress(mailQueue.From, mailQueue.From));
			message.To.Add(new MailboxAddress("Stephen", "stephen.strange100@gmail.com"));
			await Assert.ThrowsAsync<MailKit.Security.AuthenticationException>(() => mySender.SendAsync(message));
		}

#pragma warning restore CA2000 // Dispose objects before losing scope

	}




}

public class MailFixture
{
	public MailFixture()
	{
		ServiceCollection services = new ServiceCollection();
		//  var config = services.Configure<IConfiguration>(new ConfigurationBuilder()
		//.AddJsonFile("appsettings.json")
		//.Build());

		Config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

		services.AddSingleton<IConfiguration>(provider => Config)
			.AddSingleton<SmtpSection>()
			.AddSingleton<MailSender>()
			.AddSingleton<MailQueue>();

		ServiceProvider = services.BuildServiceProvider();
	}

	public ServiceProvider ServiceProvider { get; private set; }

	public IConfiguration Config { get; }
}