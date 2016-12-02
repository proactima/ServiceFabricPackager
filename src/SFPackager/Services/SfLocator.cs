using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SFPackager.Models;

namespace SFPackager.Services
{
    public class SfLocator
    {
        private readonly AppConfig _baseConfig;
        private readonly ConsoleWriter _log;
        private readonly SolutionParser _solutionParser;

        public SfLocator(AppConfig baseConfig, ConsoleWriter log, SolutionParser solutionParser)
        {
            _baseConfig = baseConfig;
            _log = log;
            _solutionParser = solutionParser;
        }

        public async Task<List<ServiceFabricApplicationProject>> LocateSfApplications()
        {
            _log.WriteLine("Locating ServiceFabric Applications.");
            var fileList = await _solutionParser.ExtractSfProjects(_baseConfig.SolutionFile).ConfigureAwait(false);

            var applications = fileList
                .Select(fileInfo =>
                {
                    _log.WriteLine($"\tFound {fileInfo.Directory.FullName}");
                    return new ServiceFabricApplicationProject
                    {
                        ProjectFile = fileInfo.Name,
                        ProjectFolder = fileInfo.Directory.FullName,
                        BuildConfiguration = _baseConfig.BuildConfiguration
                    };
                })
                .ToList();

            return applications;
        }
    }
}