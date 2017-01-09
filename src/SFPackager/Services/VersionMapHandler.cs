using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFPackager.Interfaces;
using SFPackager.Models;

namespace SFPackager.Services
{
    public class VersionMapHandler
    {
        private readonly IHandleFiles _blobService;
        private readonly ConsoleWriter _log;

        public VersionMapHandler(
            IHandleFiles blobService,
            ConsoleWriter log)
        {
            _blobService = blobService;
            _log = log;
        }

        public async Task<VersionMap> GetAsync(VersionNumber version)
        {
            _log.WriteLine($"Loading version manifest for {version}");

            try
            {
                var currentHashMapResponse = await _blobService
                        .GetFileAsStringAsync(version.FileName)
                        .ConfigureAwait(false);

                var currentVersionMap = JsonConvert.DeserializeObject<VersionMap>
                    (currentHashMapResponse.ResponseContent);

                return currentVersionMap;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task PutAsync(VersionMap version)
        {
            var versionNumber = version.PackageVersions[Constants.GlobalIdentifier].Version;

            _log.WriteLine($"Storing version map for {versionNumber}", LogLevel.Info);
            var versionJson = JsonConvert.SerializeObject(version);
            await _blobService
                .SaveFileAsync(versionNumber.FileName, versionJson)
                .ConfigureAwait(false);
        }
    }
}