using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFPackager.Helpers;
using SFPackager.Interfaces;
using SFPackager.Models;
using SFPackager.Services;
using SFPackager.Services.FileStorage;

namespace SFPackager
{
    public class App
    {
        private readonly IHandleFiles _blobService;
        private readonly IHandleClusterConnection _fabricRemote;
        private readonly ServiceHashCalculator _hasher;
        private readonly SfLocator _locator;
        private readonly SfProjectHandler _projectHandler;
        private readonly VersionHandler _versionHandler;
        private readonly VersionService _versionService;
        private readonly Packager _packager;
        private readonly ManifestWriter _manifestWriter;

        public App(
            IHandleFiles blobService,
            SfLocator locator,
            SfProjectHandler projectHandler,
            ServiceHashCalculator hasher,
            IHandleClusterConnection fabricRemote,
            VersionHandler versionHandler,
            VersionService versionService,
            Packager packager,
            ManifestWriter manifestWriter)
        {
            _blobService = blobService;
            _locator = locator;
            _projectHandler = projectHandler;
            _hasher = hasher;
            _fabricRemote = fabricRemote;
            _versionHandler = versionHandler;
            _versionService = versionService;
            _packager = packager;
            _manifestWriter = manifestWriter;
        }

        public async Task RunAsync(BaseConfig baseConfig)
        {
            Console.WriteLine("Loading config file...");
            var result = await _blobService
                .GetFileAsStringAsync(baseConfig.AzureStorageConfigFileName, baseConfig)
                .ConfigureAwait(false);
            
            var packageConfig = JsonConvert.DeserializeObject<PackageConfig>(result.ResponseContent);

            
            var sfApplications = _locator.LocateSfApplications(baseConfig.SourceBasePath, baseConfig.BuildConfiguration);

            await _fabricRemote.Init(packageConfig.Cluster, baseConfig).ConfigureAwait(false);

            Console.WriteLine("Trying to read app manifest from deployed applications...");
            var deployedApps = await _fabricRemote.GetApplicationManifestsAsync().ConfigureAwait(false);
            var currentVersion = _versionHandler.GetCurrentVersionFromApplications(deployedApps);
            var newVersion = currentVersion.Increment(baseConfig.CommitHash);

            Console.WriteLine($"New version is: {newVersion}");

            var versions = new Dictionary<string, GlobalVersion>
            {
                [Constants.GlobalIdentifier] = new GlobalVersion
                {
                    VersionType = VersionType.Global,
                    Version = currentVersion
                }
            };

            Console.WriteLine($"Loading version manifest for {currentVersion}");
            var currentHashMapResponse = await _blobService
                .GetFileAsStringAsync(currentVersion.FileName, baseConfig)
                .ConfigureAwait(false);

            var parsedApplications = new Dictionary<string, ServiceFabricApplicationProject>();

            Console.WriteLine("Parsing Service Fabric Applications and computing hashes");
            foreach (var sfApplication in sfApplications)
            {
                var project = _projectHandler.Parse(sfApplication, baseConfig.SourceBasePath, baseConfig);
                parsedApplications.Add(project.ApplicationTypeName, project);
                var serviceVersions = _hasher.Calculate(project);

                versions.Add(project.ApplicationTypeName, new GlobalVersion
                {
                    VersionType = VersionType.Application,
                    Version = currentVersion
                });

                serviceVersions.ForEach(x => { versions.Add(x.Key, x.Value); });
            }

            if (!currentHashMapResponse.IsSuccessful)
            {
                Console.WriteLine($"No remote version map found, setting everything to {newVersion}");
                _versionService.SetVersionIfNoneIsDeployed(versions, newVersion);
            }
            else
            {
                Console.WriteLine($"Setting version of changed packages to {newVersion}");
                _versionService.SetVersionsIfVersionIsDeployed(currentHashMapResponse, versions, newVersion);
            }

            Console.WriteLine("Packaging applications");
            await _packager.PackageApplications(versions, parsedApplications, packageConfig, baseConfig).ConfigureAwait(false);

            Console.WriteLine("Updating manifests");
            _manifestWriter.UpdateManifests(versions, parsedApplications, packageConfig);

            Console.WriteLine($"Storing version map for {newVersion}");
            var versionJson = JsonConvert.SerializeObject(versions);
            var fileName = versions[Constants.GlobalIdentifier].Version.FileName;
            await _blobService
                .SaveFileAsync(fileName, versionJson, baseConfig)
                .ConfigureAwait(false);
        }
    }
}