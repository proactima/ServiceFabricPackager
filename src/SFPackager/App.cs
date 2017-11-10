using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFPackager.Helpers;
using SFPackager.Interfaces;
using SFPackager.Models;
using SFPackager.Services;
using SFPackager.Services.Manifest;

namespace SFPackager
{
    public class App
    {
        private readonly AppConfig _baseConfig;
        private readonly IHandleClusterConnection _fabricRemote;
        private readonly ServiceHashCalculator _hasher;
        private readonly SfLocator _locator;
        private readonly ConsoleWriter _log;
        private readonly ManifestHandler _manifestReader;
        private readonly Packager _packager;
        private readonly SfProjectHandler _projectHandler;
        private readonly DeployScriptCreator _scriptCreator;
        private readonly VersionHandler _versionHandler;
        private readonly VersionMapHandler _versionMapHandler;
        private readonly VersionService _versionService;
        private readonly Hack _hack;

        public App(
            SfLocator locator,
            SfProjectHandler projectHandler,
            ServiceHashCalculator hasher,
            IHandleClusterConnection fabricRemote,
            VersionHandler versionHandler,
            VersionService versionService,
            Packager packager,
            AppConfig baseConfig,
            DeployScriptCreator scriptCreator,
            ConsoleWriter log,
            ManifestHandler manifestReader,
            VersionMapHandler versionMapHandler,
            Hack hack)
        {
            _locator = locator;
            _projectHandler = projectHandler;
            _hasher = hasher;
            _fabricRemote = fabricRemote;
            _versionHandler = versionHandler;
            _versionService = versionService;
            _packager = packager;
            _baseConfig = baseConfig;
            _scriptCreator = scriptCreator;
            _log = log;
            _manifestReader = manifestReader;
            _versionMapHandler = versionMapHandler;
            _hack = hack;
        }

        public async Task RunAsync()
        {
            var sfApplications = await _locator.LocateSfApplications().ConfigureAwait(false);
            await _fabricRemote.Init().ConfigureAwait(false);

            _log.WriteLine("Trying to read app manifest from deployed applications...");
            var deployedApps = await _fabricRemote.GetApplicationManifestsAsync().ConfigureAwait(false);
            var currentVersion = _versionHandler.GetCurrentVersionFromApplications(deployedApps);
            var newVersion = currentVersion.Increment(_baseConfig.UniqueVersionIdentifier);

            _log.WriteLine($"New version is: {newVersion}", LogLevel.Info);

            var hackData = _hack.FindHackableThings(sfApplications.First());

            var versions = new VersionMap
            {
                PackageVersions = new Dictionary<string, GlobalVersion>
                {
                    [Constants.GlobalIdentifier] = new GlobalVersion
                    {
                        VersionType = VersionType.Global,
                        Version = currentVersion
                    }
                }
            };

            var currentHashMap = await _versionMapHandler.GetAsync(currentVersion).ConfigureAwait(false);

            var parsedApplications = new Dictionary<string, ServiceFabricApplicationProject>();

            _log.WriteLine("Parsing Service Fabric Applications and computing hashes");
            foreach (var sfApplication in sfApplications)
            {
                var project = _projectHandler.Parse(sfApplication, _baseConfig.SourcePath);
                parsedApplications.Add(project.ApplicationTypeName, project);
                var serviceVersions = await _hasher.Calculate(project, currentVersion).ConfigureAwait(false);

                serviceVersions.ForEach(service => { versions.PackageVersions.Add(service.Key, service.Value); });
            }

            if (_baseConfig.ForcePackageAll || currentHashMap?.PackageVersions == null)
            {
                _log.WriteLine($"Force package all, setting everything to {newVersion}");
                _versionService.SetVersionIfNoneIsDeployed(versions, newVersion);
            }
            else
            {
                _log.WriteLine($"Setting version of changed packages to {newVersion}");
                _versionService.SetVersionsIfVersionIsDeployed(currentHashMap, versions, newVersion);
            }

            _log.WriteLine("Packaging applications", LogLevel.Info);
            await _packager
                .PackageApplications(versions, parsedApplications, hackData)
                .ConfigureAwait(false);

            _log.WriteLine("Updating manifests");
            _manifestReader.Handle(versions, parsedApplications);

            await _versionMapHandler.PutAsync(versions).ConfigureAwait(false);

            var things = versions
                .PackageVersions
                .Where(x => x.Value.VersionType == VersionType.Application)
                .Where(x => x.Value.IncludeInPackage)
                .Select(x => x.Key)
                .ToList();

            _scriptCreator.Do(newVersion, things);
        }
    }
}