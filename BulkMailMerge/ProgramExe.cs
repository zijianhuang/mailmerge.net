using System.Text.Json.Nodes;
using Fonlow.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace BulkMailMerge
{
	public class ProgramExe
	{
		public ProgramExe(Options options, ILogger logger)
		{
			this.logger = logger;
			this.options = options;
		}

		readonly Options options;
		readonly ILogger logger;

		public ErrorCode Execute()
		{
			var config = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", false, true)
				.AddUserSecrets(typeof(ProgramExe).Assembly)
				.Build();

			var smtpSection = new SmtpSection(config);
			var mailSender = new MailSender(smtpSection, logger);
			var mailQueue = new MailQueue(mailSender, logger);
			mailQueue.DownToZero += MailQueue_DownToZero;
			mailQueue.MailSendingErrorOccured += (my, msg) =>
			{
				errorMessagesList.Add(msg);
			};

			logger.LogInformation("Reading Email address list...");
			string[] contactList = [];
			JsonArray jsonDataArray = null;
			if (options.ContactList?.Length > 0) //there is valid ContactList defined
			{
				contactList = options.ContactList;
			}
			else if (!string.IsNullOrEmpty(options.ContactFile)) // there is valid ContactFile
			{
				contactList = ReadContactListFromFile(options.ContactFile);
			}
			else if (!string.IsNullOrEmpty(options.DataFile)) // if no explicit contact list defined, get contact list from the merge data file, along with the JsonArray data.
			{
				var dataFileExt = Path.GetExtension(options.DataFile);
				var isJsonDataFile = dataFileExt.Contains("json", StringComparison.OrdinalIgnoreCase);
				var rr = isJsonDataFile ? JsonMerger.ConvertToJsonArrayAndContactList(options.DataFile, options.KeyField) : CsvToJson.ConvertToJsonArrayAndContactList(options.DataFile, options.KeyField);
				contactList = rr.Item2;
				jsonDataArray = rr.Item1;
			}

			if (contactList.Length > 0)
			{
				var contactLen = contactList.Length;
				var pendingCount = 0;
				logger.LogInformation($"Recipients total: {contactLen}");

				try
				{
					var subjectTemplate = "Test Subject";
					if (!string.IsNullOrEmpty(options.SubjectFile))
					{
						subjectTemplate = File.ReadAllText(options.SubjectFile);
					}

					var bodyTemplate = "Test Body";
					var bodyIsHtml = false;
					if (!string.IsNullOrEmpty(options.BodyFile))
					{
						bodyTemplate = File.ReadAllText(options.BodyFile);
						var bodyFileExt = Path.GetExtension(options.BodyFile);
						bodyIsHtml = bodyFileExt.Contains("htm", StringComparison.OrdinalIgnoreCase);
					}

					var inReplyToPrefix = "EmailQueue";
					if (!string.IsNullOrEmpty(options.InReplyToPrefix))
					{
						inReplyToPrefix = options.InReplyToPrefix;
					}

					JsonMerger CreateJsonMerge(string template)
					{
						if (string.IsNullOrEmpty(options.DataFile))
						{
							return null;
						}

						JsonMerger jsonMerger = null;
						if (jsonDataArray != null)
						{
							jsonMerger = new JsonMerger(template, jsonDataArray, options.KeyField, logger);
						}
						else
						{
							var dataFileExt = Path.GetExtension(options.DataFile);
							if (dataFileExt.Contains("json", StringComparison.OrdinalIgnoreCase))
							{
								jsonMerger = new JsonMerger(template, File.ReadAllText(options.DataFile), options.KeyField, logger);
							}
							else
							{
								jsonDataArray = CsvToJson.Convert(options.DataFile);
								jsonMerger = new JsonMerger(template, jsonDataArray, options.KeyField, logger);
							}
						}

						return jsonMerger;
					}

					JsonMerger jsonMergerForBody = CreateJsonMerge(bodyTemplate);
					JsonMerger jsonMergerForSubject = null;
					if (subjectTemplate.Contains("{{") && subjectTemplate.Contains("}}"))
					{
						jsonMergerForSubject = CreateJsonMerge(subjectTemplate);
					}

					var invalidAddresses = ValidateEmailAddresses(contactList);
					if (invalidAddresses.Length > 0)
					{
						Console.WriteLine("The following Email addresses in contact list or data are in invalid format:");
						foreach (var item in invalidAddresses)
						{
							Console.WriteLine($"Item {item.Item1}: {item.Item2}");
						}

						return ErrorCode.EmailAddressParsingError;
					}

					for (int i = 0; i < contactLen; i++)
					{
						var toAddressText = contactList[i];
						var toAddress = MailboxAddress.Parse(contactList[i]);
						var bodyText = bodyTemplate;
						if (jsonMergerForBody != null)
						{
							bodyText = jsonMergerForBody.Merge(toAddressText);
							if (bodyText == null)
							{
								logger.LogWarning($"Not proceeding for {toAddressText} because of missing data in {options.DataFile}");
								continue;
							}
						}

						var subject = subjectTemplate;
						if (jsonMergerForSubject != null)
						{
							var s = jsonMergerForSubject.Merge(toAddressText);
							if (s != null)
							{
								subject = s;
							}
						}

						var message = new MimeKit.MimeMessage
						{

							Subject = subject,
							Body = bodyIsHtml ? new BodyBuilder()
							{
								HtmlBody = bodyText,
							}.ToMessageBody() : new BodyBuilder()
							{
								TextBody = bodyText,
							}.ToMessageBody(),

						};
						message.From.Add(MailboxAddress.Parse(smtpSection.From));
						message.InReplyTo = $"{inReplyToPrefix}_{Guid.NewGuid()}";
						message.To.Add(toAddress);
						mailQueue.Pend(message);
						pendingCount++;
					}
				}
				catch (FileNotFoundException ex)
				{
					logger.LogWarning(ex.Message);
					return ErrorCode.FileIOError;
				}

				if (pendingCount == 0)
				{
					logger.LogWarning("Nothing pending. Exit now.");
					return ErrorCode.NothingPending;
				}
				Console.WriteLine($"Waiting for pending mail messages (total: {pendingCount}) being sent...");
				Console.ReadLine();
				Console.WriteLine("Exit.");
				return ErrorCode.Success;

			}
			else
			{
				return ErrorCode.Success;
			}

		}

		static List<string> errorMessagesList = new List<string>();

		static void MailQueue_DownToZero(object sender, EventArgs e)
		{
			Console.WriteLine("All done.");
			if (errorMessagesList.Count > 0)
			{
				Console.WriteLine($"Total error: {errorMessagesList.Count}");
				var i = 1;
				foreach (var item in errorMessagesList)
				{
					Console.WriteLine(i + ": " + item);
				}
			}

			Environment.Exit(0);
		}

		static string[] ReadContactListFromFile(string filePath)
		{
			var ss = File.ReadAllLines(filePath);
			return [.. ss.Where(d => !string.IsNullOrWhiteSpace(d))];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="addresses"></param>
		/// <returns>Line number and the Email address invalid. And line number start from 1.</returns>
		static Tuple<int, string>[] ValidateEmailAddresses(string[] addresses)
		{
			List<Tuple<int, string>> ss = new List<Tuple<int, string>>();
			var lineNum = 1;
			foreach (var a in addresses)
			{
				if (!MailboxAddress.TryParse(new ParserOptions
				{
					AddressParserComplianceMode = RfcComplianceMode.Strict,
					Rfc2047ComplianceMode = RfcComplianceMode.Strict,
					AllowAddressesWithoutDomain = false
				}, a, out MailboxAddress mailbox))
				{
					ss.Add(Tuple.Create(lineNum, a));
				}

				lineNum++;
			}

			return [.. ss];
		}

	}
}
