using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using SFPackager.Interfaces;
using SFPackager.Models;

namespace SFPackager.Services.Cluster
{
    public class SecureClusterConnection : BaseClusterConnection, IHandleClusterConnection
    {
        private readonly IHandleFiles _blobService;
        private X509Certificate2 _cert;

        public SecureClusterConnection(IHandleFiles blobService)
        {
            _blobService = blobService;
        }

        public async Task Init(ClusterConfig clusterConfig, BaseConfig buildConfig)
        {
            Hostname = clusterConfig.Endpoint;
            Port = clusterConfig.Port;

            var pfxResponse = await _blobService
                .GetFileAsBytesAsync(clusterConfig.PfxFile, buildConfig)
                .ConfigureAwait(false);

            if (!pfxResponse.IsSuccessful)
                throw new InvalidOperationException("Problems loading cert file from blob...");

            _cert = new X509Certificate2(pfxResponse.ResponseContent, clusterConfig.PfxKey);
        }

        public async Task<List<ServiceFabricApplication>> GetApplicationManifestsAsync()
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(_cert);
            handler.ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true;

            var httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            var applications = await GetApplicationListAsync(httpClient).ConfigureAwait(false);
            return applications;
        }
    }
}