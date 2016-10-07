using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFPackager.Interfaces;
using SFPackager.Models;

namespace SFPackager.Services
{
    public class ConfigManager
    {
        private readonly AppConfig _baseConfig;
        private readonly IHandleFiles _blobService;
        private readonly ConsoleWriter _log;

        public ConfigManager(IHandleFiles blobService, AppConfig baseConfig, ConsoleWriter log)
        {
            _blobService = blobService;
            _baseConfig = baseConfig;
            _log = log;
        }

        public async Task<PackageConfig> GetPackageConfig()
        {
            _log.WriteLine("Loading config file...");
            var result = await _blobService
                .GetFileAsStringAsync(_baseConfig.ConfigFileName)
                .ConfigureAwait(false);

            var packageConfig = JsonConvert.DeserializeObject<PackageConfig>(result.ResponseContent);

            return packageConfig;
        }
    }
}