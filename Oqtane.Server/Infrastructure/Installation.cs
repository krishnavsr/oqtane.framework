﻿using System.Reflection;
using System.IO;
using System.IO.Compression;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;

namespace Oqtane.Infrastructure
{
    public class Installation  : IInstallation
    {
        private readonly IHostApplicationLifetime HostApplicationLifetime;
        private readonly IWebHostEnvironment environment;

        public Installation(IHostApplicationLifetime HostApplicationLifetime, IWebHostEnvironment environment)
        {
            this.HostApplicationLifetime = HostApplicationLifetime;
            this.environment = environment;
        }

        public void Install(string Folders)
        {
            bool install = false;
            string binfolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            foreach (string Folder in Folders.Split(','))
            {
                string folder = Path.Combine(environment.WebRootPath, Folder);

                // iterate through theme packages
                foreach (string packagename in Directory.GetFiles(folder, "*.nupkg"))
                {
                    string name = Path.GetFileNameWithoutExtension(packagename);
                    string[] segments = name.Split('.');
                    name = string.Join('.', segments, 0, segments.Length - 3);

                    // iterate through files and deploy to appropriate locations
                    using (ZipArchive archive = ZipFile.OpenRead(packagename))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            string filename = Path.GetFileName(entry.FullName);
                            switch (Path.GetExtension(filename))
                            {
                                case ".dll":
                                    entry.ExtractToFile(Path.Combine(binfolder, filename), true);
                                    break;
                                case ".png":
                                case ".jpg":
                                case ".jpeg":
                                case ".gif":
                                case ".svg":
                                case ".js":
                                case ".css":
                                    entry.ExtractToFile(folder + "\\" + entry.FullName.Replace("wwwroot", name).Replace("/","\\"), true);
                                    break;
                            }
                        }
                    }
                    // remove package
                    File.Delete(packagename);
                    install = true;
                }
            }

            if (install)
            {
                // restart application
                HostApplicationLifetime.StopApplication();
            }
        }
    }
}
