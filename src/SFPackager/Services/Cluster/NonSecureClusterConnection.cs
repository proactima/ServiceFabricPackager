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
        public Task Init(ClusterConfig clusterConfig, BaseConfig buildConfig)
        {
            Hostname = clusterConfig.Endpoint;
            Port = clusterConfig.Port;

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