using Fonlow.Cli;
using Microsoft.Extensions.Logging;

namespace BulkMailMerge
{

    internal class Program
    {
        static int Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            })); //https://learn.microsoft.com/en-us/dotnet/core/extensions/console-log-formatter
            var logger = loggerFactory.CreateLogger("Program");
            var options = new Options();
            var parser = new CommandLineParser(options);
            Console.WriteLine(parser.ApplicationDescription);
            parser.Parse();
            if (args.Length == 0 || options.Help)
            {
                Console.WriteLine(parser.UsageInfo.ToString());
                Console.WriteLine(
@"Examples:
BulkMailMerge.exe /CL=Hello@Kitty.net ~ The recepient will receive a test Email if the appsettings.json has correct SMTP config.
BulkMailMerge.exe /CF=ContactList.txt /SF=Subject.txt /BF=Body.txt ~ The recipients in ContactList.txt will receive an Email message based on the subject and the body.
BulkMailMerge.exe /CF=Profiles/ProductUpdate/ContactList.txt /SF=Profiles/ProductUpdate/Subject.txt /BF=Profiles/ProductUpdate/Body.html ~ The recipients in ContactList.txt will receive an Email message based on the subject and the HTML body.
BulkMailMerge.exe /CF=Profiles/ProductUpdate/ContactList.txt /SF=Profiles/ProductUpdate/Subject.txt /BF=Profiles/ProductUpdate/Body.html /DF=data.json  ~ What in data.json will be merged with the body template, and the first field of JSON objects is expected to be Email address.
../../App/BulkMailMerge.exe /SF=Subject.txt /BF=Body.html /DF=data.csv /KF=mailbox ~ the mailbox field of the CSV will be used as recipient.
"
                );

                return 0;
            }

            if (parser.HasErrors)
            {
                logger.LogWarning(parser.ErrorMessage);
                Console.WriteLine(parser.UsageInfo.GetOptionsAsString());
                return (int)ErrorCode.InvalidArguments;
            }

            var validationMessage = ValidateOptions(options);
            if (!string.IsNullOrEmpty(validationMessage))
            {
                logger.LogWarning(validationMessage);
                return (int)ErrorCode.InvalidOptions;
            }

            ProgramExe exe = new ProgramExe(options, logger);
            var r = exe.Execute();
            return (int)r;
        }

        static string ValidateOptions(Options options)
        {
            if (!(options.ContactList?.Length > 0 ^ options.ContactFile != null))
            {
                if (options.DataFile == null)
                {
                    return "Must have either ContactList or ContactFile, but not both when doing mail merge with data file that contain Email addresses.";
                }
            }

            return string.Empty;
        }



    }


}
