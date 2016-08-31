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
        private readonly PackageConfig _packageConfig;

        public SecureClusterConnection(IHandleFiles blobService, PackageConfig packageConfig)
        {
            _blobService = blobService;
            _packageConfig = packageConfig;
        }

        public async Task Init()
        {
            Hostname = _packageConfig.Cluster.Endpoint;
            Port = _packageConfig.Cluster.Port;

            var pfxResponse = await _blobService
                .GetFileAsBytesAsync(_packageConfig.Cluster.PfxFile)
                .ConfigureAwait(false);

            if (!pfxResponse.IsSuccessful)
                throw new InvalidOperationException("Problems loading cert file from blob...");

            _cert = new X509Certificate2(pfxResponse.ResponseContent, _packageConfig.Cluster.PfxKey);
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