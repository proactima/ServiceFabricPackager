using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using SFPackager.Interfaces;
using SFPackager.Models;

namespace SFPackager.Services.Cluster
{
    public class NonSecureClusterConnection : BaseClusterConnection, IHandleClusterConnection
    {
        private readonly PackageConfig _packageConfig;

        public NonSecureClusterConnection(PackageConfig packageConfig)
        {
            _packageConfig = packageConfig;
        }

        public Task Init()
        {
            Hostname = _packageConfig.Cluster.Endpoint;
            Port = _packageConfig.Cluster.Port;
            Scheme = "http";

            return Task.FromResult(0);
        }

        public async Task<List<ServiceFabricApplication>> GetApplicationManifestsAsync()
        {
            var handler = new HttpClientHandler();

            var httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            var applications = await GetApplicationListAsync(httpClient).ConfigureAwait(false);
            return applications;
        }
    }
}