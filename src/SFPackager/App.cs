using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFPackager.Helpers;
using SFPackager.Models;
using SFPackager.Services;

namespace SFPackager
{
    public class App
    {
        private readonly AzureBlobService _blobService;
        private readonly ServiceFabricRemote _fabricRemote;
        private readonly ServiceHashCalculator _hasher;
        private readonly SfLocator _locator;
        private readonly SfProjectHandler _projectHandler;
        private readonly VersionHandler _versionHandler;
        private readonly VersionService _versionService;
        private readonly Packager _packager;

        public App(
            AzureBlobService blobService,
            SfLocator locator,
            SfProjectHandler projectHandler,
            ServiceHashCalculator hasher,
            ServiceFabricRemote fabricRemote,
            VersionHandler versionHandler,
            VersionService versionService,
            Packager packager)
        {
            _blobService = blobService;
            _locator = locator;
            _projectHandler = projectHandler;
            _hasher = hasher;
            _fabricRemote = fabricRemote;
            _versionHandler = versionHandler;
            _versionService = versionService;
            _packager = packager;
        }

        public async Task RunAsync(BaseConfig baseConfig)
        {
            var result = await _blobService
                .ExecuteBlobOperationAsync(BlobOperation.GET, baseConfig, baseConfig.AzureStorageConfigFileName)
                .ConfigureAwait(false);

            var packageConfig = JsonConvert.DeserializeObject<PackageConfig>(result.ResponseContent);
            var sfApplications = _locator.LocateSfApplications(baseConfig.SourceBasePath, baseConfig.BuildConfiguration);


            await _fabricRemote.Init(packageConfig.Cluster, baseConfig).ConfigureAwait(false);
            var deployedApps = await _fabricRemote.GetApplicationManifestsAsync().ConfigureAwait(false);
            //var currentVersion = _versionHandler.GetCurrentVersionFromApplications(deployedApps);
            var currentVersion = VersionNumber.Create(2, "mysupercommithash");
            var newVersion = currentVersion.Increment(baseConfig.CommitHash);

            var versions = new Dictionary<string, GlobalVersion>
            {
                [Constants.GlobalIdentifier] = new GlobalVersion
                {
                    VersionType = VersionType.Global,
                    Version = currentVersion
                }
            };

            var currentHashMapResponse = await _blobService
                .ExecuteBlobOperationAsync(BlobOperation.GET, baseConfig, currentVersion.FileName)
                .ConfigureAwait(false);

            var parsedApplications = new List<ServiceFabricApplicationProject>();

            foreach (var sfApplication in sfApplications)
            {
                var project = _projectHandler.Parse(sfApplication, baseConfig.SourceBasePath);
                parsedApplications.Add(project);
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
                _versionService.SetVersionIfNoneIsDeployed(versions, newVersion);
            }
            else
            {
                _versionService.SetVersionsIfVersionIsDeployed(currentHashMapResponse, versions, newVersion);
            }

            var versionJson = JsonConvert.SerializeObject(versions);

            await _blobService
                .ExecuteBlobOperationAsync(BlobOperation.PUT, baseConfig,
                    versions[Constants.GlobalIdentifier].Version.FileName,
                    versionJson)
                .ConfigureAwait(false);

            _packager.PackageApplications(versions, parsedApplications, packageConfig);
        }
    }
}