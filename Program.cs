using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;
using System.IO;

namespace nugetsync
{
    class Program
    {
        static void Main()
        {
            //Connect to the official package repository
            var repo = new DataServicePackageRepository(new Uri("https://www.nuget.org/api/v2/curated-feeds/microsoftdotnet/"));
            //Get the list of all NuGet packages 
           
            var pd = new PackageDownloader();
            var skip = 0;
            for (;;){
                var packages = repo.GetPackages().Skip(skip).Take(10).ToList();
                if (packages.Count() == 0)
                {
                    break;
                }
                foreach (DataServicePackage p in packages)
                {
                    Console.Write(p);
                    var sb = new StringBuilder();
                    var pkgFilename = sb.Append(p.Id).Append(".").Append(p.Version).Append(".nupkg");
                    var packageFile = Path.Combine("G:\\NuGetLocal", pkgFilename.ToString());
                    Console.Write(packageFile);
                    if (!File.Exists(packageFile))
                    {
                        Console.WriteLine(" does not exists. Downloading...");
                    
                        using (var fs = new FileStream(packageFile, FileMode.Create))
                        {
                            try
                            {
                                pd.DownloadPackage(p.DownloadUrl, p, fs);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                               // break;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine(" exists");
                    }
                }
                skip += 10;
            }
        }
           
    }
}
