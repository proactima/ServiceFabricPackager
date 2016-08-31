using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SFPackager.Models;

namespace SFPackager.Services
{
    public class SfLocator
    {
        private readonly CmdLineOptions _baseConfig;

        public SfLocator(CmdLineOptions baseConfig)
        {
            _baseConfig = baseConfig;
        }

        public List<ServiceFabricApplicationProject> LocateSfApplications()
        {
            Console.WriteLine("Locating ServiceFabric Applications.");
            var directory = new DirectoryInfo(_baseConfig.SourceBasePath);
            var fileList = directory.GetFiles("*.sfproj", SearchOption.AllDirectories);

            var applications = fileList
                //.Where(x => !string.Equals(x.Name, "SignupWeb.sfproj")) 
                .Select(fileInfo =>
                {
                    Console.WriteLine($"\tFound {fileInfo.Directory.FullName}");
                    return new ServiceFabricApplicationProject
                    {
                        ProjectFile = fileInfo.Name,
                        ProjectFolder = fileInfo.Directory.FullName,
                        BasePath = _baseConfig.SourceBasePath,
                        BuildConfiguration = _baseConfig.BuildConfiguration
                    };
                })
                .ToList();

            Console.WriteLine();
            return applications;
        }
    }
}