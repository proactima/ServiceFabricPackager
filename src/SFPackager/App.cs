using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFPackager.Helpers;
using SFPackager.Interfaces;
using SFPackager.Models;
using SFPackager.Services;
using SFPackager.Services.Manifest;

namespace SFPackager
{
    public class App
    {
        private readonly CmdLineOptions _baseConfig;
        private readonly IHandleFiles _blobService;
        private readonly IHandleClusterConnection _fabricRemote;
        private readonly ServiceHashCalculator _hasher;
        private readonly SfLocator _locator;
        private readonly ManifestWriter _manifestWriter;
        private readonly Packager _packager;
        private readonly SfProjectHandler _projectHandler;
        private readonly VersionHandler _versionHandler;
        private readonly VersionService _versionService;

        public App(
            IHandleFiles blobService,
            SfLocator locator,
            SfProjectHandler projectHandler,
            ServiceHashCalculator hasher,
            IHandleClusterConnection fabricRemote,
            VersionHandler versionHandler,
            VersionService versionService,
            Packager packager,
            ManifestWriter manifestWriter,
            CmdLineOptions baseConfig)
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
            _baseConfig = baseConfig;
        }

        public async Task RunAsync()
        {
            var sfApplications = _locator.LocateSfApplications();
            await _fabricRemote.Init().ConfigureAwait(false);

            Console.WriteLine("Trying to read app manifest from deployed applications...");
            var deployedApps = await _fabricRemote.GetApplicationManifestsAsync().ConfigureAwait(false);
            //var currentVersion = _versionHandler.GetCurrentVersionFromApplications(deployedApps);
            var currentVersion = VersionNumber.Create(1, "908bb4dd64");
            var newVersion = currentVersion.Increment(_baseConfig.CommitHash);

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
                .GetFileAsStringAsync(currentVersion.FileName)
                .ConfigureAwait(false);

            var parsedApplications = new Dictionary<string, ServiceFabricApplicationProject>();

            Console.WriteLine("Parsing Service Fabric Applications and computing hashes");
            foreach (var sfApplication in sfApplications)
            {
                var project = _projectHandler.Parse(sfApplication, _baseConfig.SourceBasePath);
                parsedApplications.Add(project.ApplicationTypeName, project);
                var serviceVersions = _hasher.Calculate(project, currentVersion);

                serviceVersions.ForEach(service =>
                {
                    versions.Add(service.Key, service.Value);
                });
            }

            if (_baseConfig.ForcePackageAll || !currentHashMapResponse.IsSuccessful)
            {
                Console.WriteLine($"Force package all, setting everything to {newVersion}");
                _versionService.SetVersionIfNoneIsDeployed(versions, newVersion);
            }
            else
            {
                Console.WriteLine($"Setting version of changed packages to {newVersion}");
                _versionService.SetVersionsIfVersionIsDeployed(currentHashMapResponse, versions, newVersion);
            }

            Console.WriteLine("Packaging applications");
            await _packager
                .PackageApplications(versions, parsedApplications)
                .ConfigureAwait(false);

            Console.WriteLine("Updating manifests");
            _manifestWriter.UpdateManifests(versions, parsedApplications);

            Console.WriteLine($"Storing version map for {newVersion}");
            var versionJson = JsonConvert.SerializeObject(versions);
            var fileName = versions[Constants.GlobalIdentifier].Version.FileName;
            await _blobService
                .SaveFileAsync(fileName, versionJson)
                .ConfigureAwait(false);
        }
    }
}