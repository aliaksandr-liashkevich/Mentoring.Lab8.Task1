using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using Mentoring.Lab8.Task1.Downloader.Models;
using Mentoring.Lab8.Task1.Downloader.Services;

namespace Mentoring.Lab8.Task1.App
{
    public class Program
    {
        [Option(CommandOptionType.SingleValue, ShortName = ("v"))]
        public bool Verbose { get; set; } = true;

        [Option(CommandOptionType.SingleValue, ShortName = ("u"))]
        public string Url { get; set; }

        [Option(CommandOptionType.SingleValue, ShortName = ("o"))]
        public string OutputDirectory { get; set; }

        [Option(CommandOptionType.SingleValue, ShortName = ("l"))]
        public int DeepLevel { get; set; } = 0;

        [Option(CommandOptionType.SingleValue, ShortName = ("c"))]
        public CrossDomainConstraintType CrossDomainConstraintType { get; set; } =
            CrossDomainConstraintType.OnlyCurrentDomain;

        [Option(CommandOptionType.MultipleValue, ShortName = ("a"))]
        public IEnumerable<string> AllowedExtensions { get; set; } = new List<string>();

        static void Main(string[] args)
        {
            CommandLineApplication.Execute<Program>(args);
        }

        private void OnExecute()
        {
            try
            {
                var logger = new Logger(Verbose);

                var constraint = new List<IConstraint>
                {
                    new FileTypesConstraint(AllowedExtensions),
                    new CrossDomainConstraint(Url, CrossDomainConstraintType)
                };
                var saver = new Saver(OutputDirectory, logger);

                var downloader = new SiteDownloader(constraint, saver, logger, DeepLevel);

                downloader.DownloadAsync(Url).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to execute application.");
                Console.WriteLine($"Message: {ex.Message}");
            }
        }
    }
}
