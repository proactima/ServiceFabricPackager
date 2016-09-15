using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SFPackager.Models;

namespace SFPackager.Services
{
    public class SfLocator
    {
        private readonly AppConfig _baseConfig;

        public SfLocator(AppConfig baseConfig)
        {
            _baseConfig = baseConfig;
        }

        public List<ServiceFabricApplicationProject> LocateSfApplications()
        {
            Console.WriteLine("Locating ServiceFabric Applications.");
            var fileList = _baseConfig.SourcePath.GetFiles("*.sfproj", SearchOption.AllDirectories);

            var applications = fileList
                .Select(fileInfo =>
                {
                    Console.WriteLine($"\tFound {fileInfo.Directory.FullName}");
                    return new ServiceFabricApplicationProject
                    {
                        ProjectFile = fileInfo.Name,
                        ProjectFolder = fileInfo.Directory.FullName,
                        BasePath = Path.GetFullPath(_baseConfig.SourcePath.FullName),
                        BuildConfiguration = _baseConfig.BuildConfiguration
                    };
                })
                .ToList();

            Console.WriteLine();
            return applications;
        }
    }
}