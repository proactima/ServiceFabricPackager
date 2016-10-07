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
        private readonly ConsoleWriter _log;

        public SfLocator(AppConfig baseConfig, ConsoleWriter log)
        {
            _baseConfig = baseConfig;
            _log = log;
        }

        public List<ServiceFabricApplicationProject> LocateSfApplications()
        {
            _log.WriteLine("Locating ServiceFabric Applications.");
            var fileList = _baseConfig.SourcePath.GetFiles("*.sfproj", SearchOption.AllDirectories);

            var applications = fileList
                .Select(fileInfo =>
                {
                    _log.WriteLine($"\tFound {fileInfo.Directory.FullName}");
                    return new ServiceFabricApplicationProject
                    {
                        ProjectFile = fileInfo.Name,
                        ProjectFolder = fileInfo.Directory.FullName,
                        BasePath = Path.GetFullPath(_baseConfig.SourcePath.FullName),
                        BuildConfiguration = _baseConfig.BuildConfiguration
                    };
                })
                .ToList();

            return applications;
        }
    }
}