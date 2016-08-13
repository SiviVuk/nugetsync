using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace nugetsync
{
    class CliOptions
    {
        [Option('r', "repository", Required = true, DefaultValue = "https://www.nuget.org/api/v2/curated-feeds/microsoftdotnet/", HelpText = "Nuget feed URL")]
        public string Repository { get; set; }

        [Option('f', "folder", Required = true, DefaultValue = @"E:\NuGetLocal", HelpText = "Local folder to store files")]
        public string LocalDir { get; set; }

        [Option('m', "max-concurrent-downloads",  DefaultValue = 5 , HelpText = "The maximum number of concurrent downloads")]
        public int MaxConcurrentDownloads { get; set; }

        [Option('v', null, HelpText = "Print details during execution.")]
        public bool Verbose { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            // this without using CommandLine.Text
            //  or using HelpText.AutoBuild
            var usage = new StringBuilder();
            usage.AppendLine("NugetSync 1.0");
            usage.AppendLine("Read user manual for usage instructions...");
            return usage.ToString();
        }
    }
}
