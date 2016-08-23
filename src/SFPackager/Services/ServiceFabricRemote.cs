using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using SFPackager.Models;

namespace SFPackager.Services
{
    public class ServiceFabricRemote
    {
        private const string ApiVersion = "1.0";
        private string _hostname;
        private int _port;
        private X509Certificate2 _cert;
        private readonly AzureBlobService _blobService;

        public ServiceFabricRemote(AzureBlobService blobService)
        {
            _blobService = blobService;
        }

        public async Task Init(ClusterConfig clusterConfig, BaseConfig buildConfig)
        {
            _hostname = clusterConfig.Endpoint;
            _port = clusterConfig.Port;

            var pfxResponse = await _blobService
                .GetBytesFromBlob(buildConfig, clusterConfig.PfxFile)
                .ConfigureAwait(false);

            if(!pfxResponse.IsSuccessful)
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

        private async Task<string> GetApplicationManifest(HttpClient httpClient, ServiceFabricApplication application)
        {
            var uri = new UriBuilder
            {
                Scheme = "https",
                Host = _hostname,
                Port = _port,
                Path = $"/ApplicationTypes/{application.TypeName}/$/GetApplicationManifest"
            }.Uri.ToString();

            var queryParams = new Dictionary<string, string>
            {
                ["ApplicationTypeVersion"] = application.TypeVersion,
                ["api-version"] = ApiVersion
            };

            var final = QueryHelpers.AddQueryString(uri, queryParams);

            try
            {
                var response = await httpClient.GetAsync(final).ConfigureAwait(false);
                var data = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var output = JsonConvert.DeserializeObject<TempManifest>(data);
                return output.Manifest;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        private async Task<List<ServiceFabricApplication>> GetApplicationListAsync(HttpClient httpClient)
        {
            var uri = new UriBuilder
            {
                Scheme = "https",
                Host = _hostname,
                Port = _port,
                Path = "/Applications"
            }.Uri.ToString();

            var queryParams = new Dictionary<string, string>
            {
                ["api-version"] = ApiVersion
            };

            var final = QueryHelpers.AddQueryString(uri, queryParams);

            try
            {
                var response = await httpClient.GetAsync(final).ConfigureAwait(false);
                var data = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var output = JsonConvert.DeserializeObject<List<ServiceFabricApplication>>(data);
                return output;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return new List<ServiceFabricApplication>();
        }

        private class TempManifest
        {
            public string Manifest { get; set; }
        }
    }
}