using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using SFPackager.Models;

namespace SFPackager.Services.Cluster
{
    public class BaseClusterConnection
    {
        protected const string ApiVersion = "1.0";
        protected string Hostname;
        protected int Port;

        protected async Task<string> GetApplicationManifest(HttpClient httpClient, ServiceFabricApplication application)
        {
            var uri = new UriBuilder
            {
                Scheme = "https",
                Host = Hostname,
                Port = Port,
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

        protected async Task<List<ServiceFabricApplication>> GetApplicationListAsync(HttpClient httpClient)
        {
            var uri = new UriBuilder
            {
                Scheme = "https",
                Host = Hostname,
                Port = Port,
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