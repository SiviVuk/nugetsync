using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;
using System.IO;

namespace nugetsync
{
    class Program
    {  
        
        static void RunPackageDownloader(BlockingCollection<DataServicePackage> packages)
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
                var packageFile = Path.Combine(@"G:\NuGetLocal", pkgFilename.ToString());

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

        static void Main()
        {
            //Connect to the official package repository
            var repo = new DataServicePackageRepository(new Uri("https://www.nuget.org/api/v2/curated-feeds/microsoftdotnet/"));
            //Get the list of all NuGet packages
                     
            var skip = 0;
                
            var packages = new BlockingCollection<DataServicePackage>(5);

            var packageDownloaders = new Task[5];

            for (int i = 0; i < 5; i++)
            {
                packageDownloaders[i] = Task.Run(() => RunPackageDownloader(packages));
            }

           // Task.Fac
            
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
