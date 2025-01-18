using Fonlow.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace MailUnitTests
{
	public class MailTests : IClassFixture<MailFixture>
	{
		public MailTests(MailFixture fixture)
		{
			mailQueue = fixture.ServiceProvider.GetService<MailQueue>();
			sender = fixture.ServiceProvider.GetService<SMTPSender>();
			config = fixture.Config;
		}

		readonly MailQueue mailQueue;
		readonly SMTPSender sender;
		readonly IConfiguration config;
#pragma warning disable CA2000 // Dispose objects before losing scope

		[Fact]
		public void TestPend()
		{
			System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage
			{
				Subject = "Pend Just from unit test" + DateTime.Now.ToString(),
				Body = "You will see me",
				From = new System.Net.Mail.MailAddress(mailQueue.From),
			};
			message.To.Add("stephen.strange100@gmail.com");
			mailQueue.Pend(message);

			System.Threading.Thread.Sleep(2500);
		}

		[Fact]
		public void TestSend()
		{
			System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage
			{
				Subject = "Send Just from unit test" + DateTime.Now.ToString(),
				Body = "You will see me",
				From = new System.Net.Mail.MailAddress(sender.From),
			};
			message.To.Add("stephen.strange100@gmail.com");
			sender.Send(message);
		}

		[Fact]
		public void TestSendThrow()
		{
			SmtpSection smtpSection = new SmtpSection(config);
			smtpSection.Password = "XXX"+ smtpSection.Password;
			//smtpSection.Host = "kkkkk";
			SMTPSender mySender = new SMTPSender(smtpSection);

			System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage
			{
				Subject = "Send Just from unit test" + DateTime.Now.ToString(),
				Body = "You will see me",
				From = new System.Net.Mail.MailAddress(smtpSection.From),
			};

			message.To.Add("stephen.strange100@gmail.com");
			Assert.Throws<System.Net.Mail.SmtpException>(() => mySender.Send(message));
		}

		[Fact]
		public async void TestSendAsync()
		{
			System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage
			{
				Subject = "SendAsync Just from unit test" + DateTime.Now.ToString(),
				Body = "You will see me",
				From = new System.Net.Mail.MailAddress(sender.From),
			};
			message.To.Add("stephen.strange100@gmail.com");
			await sender.SendAsync(message);
		}

		[Fact]
		public async void TestSendAsyncThrow()
		{
			SmtpSection smtpSection = new SmtpSection(config);
			smtpSection.Password = "XXX" + smtpSection.Password;
			//smtpSection.Host = "kkkkk";
			SMTPSender mySender = new SMTPSender(smtpSection);

			System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage
			{
				Subject = "Send Just from unit test" + DateTime.Now.ToString(),
				Body = "You will see me",
				From = new System.Net.Mail.MailAddress(smtpSection.From),
			};

			message.To.Add("stephen.strange100@gmail.com");
			await Assert.ThrowsAsync<System.Net.Mail.SmtpException>(() => mySender.SendAsync(message));
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
			.AddSingleton<SMTPSender>()
			.AddSingleton<MailQueue>();

		ServiceProvider = services.BuildServiceProvider();
	}

	public ServiceProvider ServiceProvider { get; private set; }

	public IConfiguration Config { get; }
}