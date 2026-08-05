using Fonlow.Cli;
using Plossum.CommandLine;

namespace BulkMailMerge
{
    [CliManager(Description = "Bulk send Email messages merged with CSV or JSON data to contacts.")]
    public class Options
    {
        [CommandLineOption(Aliases = "CL", Description = "Array of Email addresses, e.g., /CL=some@where.com hello@kitty.net . If CF is also not declared, Email addresses in data file will be used.")]
        public string[] ContactList { get; set; } = [];

        [CommandLineOption(Aliases = "CF", Description = "List of Email addresses line by line, e.g., /CF=EmailAddresses.txt . Optional if mail merge is utilized. If CL is also not declared, Email addresses in data file will be used.")]
        public string ContactFile { get; set; }

        [CommandLineOption(Aliases = "SF", Description = "Email subject or template in text file, e.g., /SF=subject.txt.")]
        public string SubjectFile { get; set; }

        [CommandLineOption(Aliases = "BF", Description = "Email body or template in text file, e.g., /BF=body.html . If the file ext is html or htm, the Email will be in HTML format, otherwise, plain text.")]
        public string BodyFile { get; set; }

        [CommandLineOption(Aliases = "DF", Description = "CSV or array of JSON data in text file to merge with the body template, e.g., /DF=data.json or /DF=data.csv. The data file must have a field for Email address.")]
        public string DataFile { get; set; }

        [CommandLineOption(Aliases = "KF", Description = "For mail merge with data, the key field to match the Email address in the contact file, e.g., /KF=EmailAddress . Optional. If not defined, the first field is used.")]
        public string KeyField { get; set; }

        [CommandLineOption(Aliases = "IRTP", Description = "For recipient to group or filter messages. Default: EmailQueue. Each Email sent has Prefix+GUID as InReplyTo, e.g., /IRTP=MyCompany, then InReplyTo will become MyCompany_GUID for each Email message sent.")]
        public string InReplyToPrefix { get; set; }

        [CommandLineOption(Aliases = "h ?", Name = "Help", Description = "Shows this help text")]
        public bool Help
        {
            get;
            set;
        }

		[CommandLineOption(Aliases = "pl", Name = "ProtocolLog", Description = "Protocol log file for diagnostic purposes at the protocol level. If declared, it overrides what is defined in the configuration file appsettings.json.")]
		public string ProtocolLogFile
        {
            get; set;
        }
    }
}
