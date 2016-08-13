using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;
using System.IO;
using CommandLine;

namespace nugetsync
{
    class Program
    {  
        
        static void RunPackageDownloader(BlockingCollection<DataServicePackage> packages,string localDirectory)
        {
            while (!packages.IsCompleted)
            {               
                DataServicePackage p;
                try
                {
                    p = packages.Take();
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("Adding was completed!");
                    return;
                }
                var sb = new StringBuilder();
                var pkgFilename = sb.Append(p.Id).Append(".").Append(p.Version).Append(@".nupkg");
                var packageFile = Path.Combine(localDirectory, pkgFilename.ToString());

                if (!File.Exists(packageFile))
                {
                    Console.WriteLine("Package {0} does not exists. Downloading...", p);

                    using (var fs = new FileStream(packageFile, FileMode.Create))
                    {
                        try
                        {
                            var pd = new PackageDownloader();
                            pd.DownloadPackage(p.DownloadUrl, p, fs);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Package {0} exists", p);
                }               
            }
            Console.WriteLine("\r\nNo more items to take. Press the Enter key to exit.");
        }    

        static void Main(string []args)
        {
            var options = new CliOptions();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                // consume Options instance properties
                if (options.Verbose)
                {
                    Console.WriteLine(options.Repository);
                    Console.WriteLine(options.LocalDir);
                    Console.WriteLine(options.MaxConcurrentDownloads);
                }
                else
                    Console.WriteLine("working ...");
            }
            else
            {
                // Display the default usage information
                Console.WriteLine(options.GetUsage());
                return;
            }
      
            //Connect to the official package repository
            var repo = new DataServicePackageRepository(new Uri(options.Repository));
            
            //Get the list of all NuGet packages
                     
            var skip = 0;
                
            var packages = new BlockingCollection<DataServicePackage>(5);

            var packageDownloaders = new Task[options.MaxConcurrentDownloads];

            for (int i = 0; i < options.MaxConcurrentDownloads; i++)
            {
                packageDownloaders[i] = Task.Run(() => RunPackageDownloader(packages,options.LocalDir));
            }
            
            // reading list of packages and add it to collection
            for (;;){
                var packagesList = repo.GetPackages().Skip(skip).Take(10).ToList();
                if (packagesList.Count() == 0)
                {
                    packages.CompleteAdding();
                    break;
                }
                foreach (DataServicePackage p in packagesList)
                {
                    packages.Add(p);
                }
                skip += 10;
            }
        }                  
    }
}
