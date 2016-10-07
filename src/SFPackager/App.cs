﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFPackager.Helpers;
using SFPackager.Interfaces;
using SFPackager.Models;
using SFPackager.Services;

namespace SFPackager
{
    public class App
    {
        private readonly AppConfig _baseConfig;
        private readonly IHandleFiles _blobService;
        private readonly IHandleClusterConnection _fabricRemote;
        private readonly ServiceHashCalculator _hasher;
        private readonly SfLocator _locator;
        private readonly ManifestWriter _manifestWriter;
        private readonly Packager _packager;
        private readonly SfProjectHandler _projectHandler;
        private readonly VersionHandler _versionHandler;
        private readonly VersionService _versionService;
        private readonly DeployScriptCreator _scriptCreator;
        private readonly ConsoleWriter _log;

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
            AppConfig baseConfig,
            DeployScriptCreator scriptCreator,
            ConsoleWriter log)
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
            _scriptCreator = scriptCreator;
            _log = log;
        }

        public async Task RunAsync()
        {
            var sfApplications = _locator.LocateSfApplications();
            await _fabricRemote.Init().ConfigureAwait(false);

            _log.WriteLine("Trying to read app manifest from deployed applications...");
            var deployedApps = await _fabricRemote.GetApplicationManifestsAsync().ConfigureAwait(false);
            var currentVersion = _versionHandler.GetCurrentVersionFromApplications(deployedApps);
            var newVersion = currentVersion.Increment(_baseConfig.UniqueVersionIdentifier);

            _log.WriteLine($"New version is: {newVersion}", LogLevel.Info);

            var versions = new Dictionary<string, GlobalVersion>
            {
                [Constants.GlobalIdentifier] = new GlobalVersion
                {
                    VersionType = VersionType.Global,
                    Version = currentVersion
                }
            };

            _log.WriteLine($"Loading version manifest for {currentVersion}");
            var currentHashMapResponse = await _blobService
                .GetFileAsStringAsync(currentVersion.FileName)
                .ConfigureAwait(false);

            var parsedApplications = new Dictionary<string, ServiceFabricApplicationProject>();

            _log.WriteLine("Parsing Service Fabric Applications and computing hashes");
            foreach (var sfApplication in sfApplications)
            {
                var project = _projectHandler.Parse(sfApplication, _baseConfig.SourcePath);
                parsedApplications.Add(project.ApplicationTypeName, project);
                var serviceVersions = _hasher.Calculate(project, currentVersion);

                serviceVersions.ForEach(service => { versions.Add(service.Key, service.Value); });
            }

            if (_baseConfig.ForcePackageAll || !currentHashMapResponse.IsSuccessful)
            {
                _log.WriteLine($"Force package all, setting everything to {newVersion}");
                _versionService.SetVersionIfNoneIsDeployed(versions, newVersion);
            }
            else
            {
                _log.WriteLine($"Setting version of changed packages to {newVersion}");
                _versionService.SetVersionsIfVersionIsDeployed(currentHashMapResponse, versions, newVersion);
            }

            _log.WriteLine("Packaging applications", LogLevel.Info);
            await _packager
                .PackageApplications(versions, parsedApplications)
                .ConfigureAwait(false);

            _log.WriteLine("Updating manifests");
            _manifestWriter.UpdateManifests(versions, parsedApplications);

            _log.WriteLine($"Storing version map for {newVersion}", LogLevel.Info);
            var versionJson = JsonConvert.SerializeObject(versions);
            var fileName = versions[Constants.GlobalIdentifier].Version.FileName;
            await _blobService
                .SaveFileAsync(fileName, versionJson)
                .ConfigureAwait(false);

            var basePackagePath = new DirectoryInfo(Path.Combine(_baseConfig.SourcePath.FullName, "sfpackaging"));
            var things = versions
                .Where(x => x.Value.VersionType == VersionType.Application)
                .Where(x => x.Value.IncludeInPackage)
                .Select(x => x.Key)
                .ToList();

            _scriptCreator.Do(newVersion, basePackagePath, things);
        }
    }
}