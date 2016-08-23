using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SFPackager.Interfaces;
using SFPackager.Models;

namespace SFPackager.Services.FileStorage
{
    public class AzureBlobService : IHandleFiles
    {
        public async Task<Response<string>> GetFileAsStringAsync(string fileName, BaseConfig baseConfig)
        {
            return await ExecuteFileOperationAsync(BlobOperation.GET, baseConfig, fileName)
                .ConfigureAwait(false);
        }

        public async Task<Response<byte[]>> GetFileAsBytesAsync(string fileName, BaseConfig baseConfig)
        {
            return await GetBytesFromBlob(baseConfig, fileName)
                .ConfigureAwait(false);
        }

        public async Task<Response<string>> SaveFileAsync(string fileName, string content, BaseConfig baseConfig)
        {
            return await ExecuteFileOperationAsync(BlobOperation.PUT, baseConfig, fileName, content)
                .ConfigureAwait(false);
        }

        private async Task<Response<string>> ExecuteFileOperationAsync(BlobOperation operation,
            BaseConfig baseConfig, string targetFilename, string payload = "")
        {
            var clientRequestId = Guid.NewGuid().ToString();
            var requestTime = DateTime.Now.ToString("R", CultureInfo.InvariantCulture);
            var blobType = operation == BlobOperation.PUT ? "BlockBlob" : "";
            var verb = Enum.GetName(typeof(BlobOperation), operation);
            var contentLength = 0;
            StringContent content = null;

            if (operation == BlobOperation.PUT)
            {
                content = new StringContent(payload);
                contentLength = payload.Length;
            }

            var challengeString = CreateChallengeString(verb, clientRequestId, requestTime, baseConfig, contentLength,
                targetFilename, operation == BlobOperation.PUT);
            var sharedKey = CreateSharedKey(baseConfig, challengeString);
            var uri =
                new Uri(
                    $"https://{baseConfig.AzureStorageAccountName}.blob.core.windows.net/{baseConfig.AzureStorageContainerName}/{targetFilename}");

            var httpClient = new HttpClient();
            AddHeaders(httpClient, clientRequestId, requestTime, sharedKey, baseConfig, blobType);

            try
            {
                HttpResponseMessage response;
                switch (operation)
                {
                    case BlobOperation.GET:
                        response = await httpClient.GetAsync(uri).ConfigureAwait(false);
                        var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        return new Response<string>
                        {
                            Operation = operation,
                            ResponseContent = result,
                            StatusCode = response.StatusCode
                        };
                    case BlobOperation.PUT:
                        response = await httpClient.PutAsync(uri, content).ConfigureAwait(false);
                        return new Response<string>
                        {
                            Operation = operation,
                            ResponseContent = string.Empty,
                            StatusCode = response.StatusCode
                        };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return new Response<string>
            {
                Operation = operation,
                ResponseContent = "Something fucked up",
                StatusCode = HttpStatusCode.InternalServerError
            };
        }

        private async Task<Response<byte[]>> GetBytesFromBlob(BaseConfig baseConfig, string targetFilename)
        {
            var clientRequestId = Guid.NewGuid().ToString();
            var requestTime = DateTime.Now.ToString("R", CultureInfo.InvariantCulture);
            var blobType = "";
            var verb = Enum.GetName(typeof(BlobOperation), BlobOperation.GET);
            var contentLength = 0;

            var challengeString = CreateChallengeString(verb, clientRequestId, requestTime, baseConfig, contentLength,
                targetFilename);
            var sharedKey = CreateSharedKey(baseConfig, challengeString);
            var uri =
                new Uri(
                    $"https://{baseConfig.AzureStorageAccountName}.blob.core.windows.net/{baseConfig.AzureStorageContainerName}/{targetFilename}");

            var httpClient = new HttpClient();
            AddHeaders(httpClient, clientRequestId, requestTime, sharedKey, baseConfig, blobType);

            try
            {
                var response = await httpClient.GetAsync(uri).ConfigureAwait(false);
                var result = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                return new Response<byte[]>
                {
                    Operation = BlobOperation.GET,
                    ResponseContent = result,
                    StatusCode = response.StatusCode
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return new Response<byte[]>
            {
                Operation = BlobOperation.GET,
                ResponseContent = new byte[0],
                StatusCode = HttpStatusCode.InternalServerError
            };
        }

        private string CreateSharedKey(BaseConfig baseConfig, string challengeString)
        {
            var accessKey = Convert.FromBase64String(baseConfig.AzureStorageAccountKey);
            var hasher = new HMACSHA256(accessKey);

            var hashed = hasher.ComputeHash(Encoding.UTF8.GetBytes(challengeString));
            var base64 = Convert.ToBase64String(hashed);

            return base64;
        }

        private string CreateChallengeString(string verb, string requestId, string requestTime, BaseConfig baseConfig,
            int contentLength, string fileName, bool isPut = false)
        {
            var canonicalizedResourceUri =
                $"/{baseConfig.AzureStorageAccountName}/{baseConfig.AzureStorageContainerName}/{fileName}";

            var challengeString = new StringBuilder();
            challengeString.Append(verb.ToUpper() + "\n");
            challengeString.Append("\n"); /*Content-Encoding*/
            challengeString.Append("\n"); /*Content-Language*/
            AppendIfEmpty(challengeString, contentLength); /*Content-Length*/
            challengeString.Append("\n"); /*Content-MD5*/
            challengeString.Append(isPut ? "text/plain; charset=utf-8\n" : "\n");
            challengeString.Append("\n"); /*Date*/
            challengeString.Append("\n"); /*If-Modified-Since*/
            challengeString.Append("\n"); /*If-Match*/
            challengeString.Append("\n"); /*If-None-Match*/
            challengeString.Append("\n"); /*If-Unmodified-Since*/
            challengeString.Append("\n"); /*Range*/
            if (isPut)
                challengeString.Append("x-ms-blob-type:BlockBlob\n");
            challengeString.Append($"x-ms-client-request-id:{requestId}\n");
            challengeString.Append($"x-ms-date:{requestTime}\n");
            challengeString.Append($"x-ms-version:2015-07-08\n");
            challengeString.Append(canonicalizedResourceUri);

            return challengeString.ToString();
        }

        private static void AddHeaders(HttpClient client, string clientRequestId, string requestTime,
            string signedHeader, BaseConfig baseConfig, string blobType = "")
        {
            client.DefaultRequestHeaders.Add("x-ms-client-request-id", new[] {clientRequestId});
            client.DefaultRequestHeaders.Add("x-ms-date", new[] {requestTime});
            client.DefaultRequestHeaders.Add("x-ms-version", "2015-07-08");
            if (!string.IsNullOrWhiteSpace(blobType))
                client.DefaultRequestHeaders.Add("x-ms-blob-type", blobType);

            client.DefaultRequestHeaders.Authorization =
                AuthenticationHeaderValue.Parse($"SharedKey {baseConfig.AzureStorageAccountName}:{signedHeader}");
        }

        //public async Task<string> GetBlobAsync(Config config, string fileName)
        //{
        //    var clientRequestId = Guid.NewGuid().ToString();
        //    var requestTime = DateTime.Now.ToString("R", CultureInfo.InvariantCulture);
        //    var challengeString = CreateChallengeString("GET", clientRequestId, requestTime, config, 0, fileName, false);
        //    var signedKey = CreateSharedKey(config, challengeString);

        //    var httpClient = new HttpClient();
        //    httpClient.DefaultRequestHeaders.Add("x-ms-client-request-id", new[] {clientRequestId});
        //    httpClient.DefaultRequestHeaders.Add("x-ms-date", new[] {requestTime});
        //    httpClient.DefaultRequestHeaders.Add("x-ms-version", "2015-07-08");
        //    httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"SharedKey {config.AzureStorageAccountName}:{signedKey}");

        //    var uri = new Uri($"https://{config.AzureStorageAccountName}.blob.core.windows.net/{config.AzureStorageContainerName}/{fileName}");

        //    try
        //    {
        //        var response = await httpClient.GetAsync(uri).ConfigureAwait(false);
        //        var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }

        //    return string.Empty;
        //}

        //public async Task SaveBlob(Config config, string fileName)
        //{
        //    var content = new StringContent(fileName);
        //    var contentLength = fileName.Length;

        //    var clientRequestId = Guid.NewGuid().ToString();
        //    var requestTime = DateTime.Now.ToString("R", CultureInfo.InvariantCulture);

        //    var challengeString = CreateChallengeString("PUT", clientRequestId, requestTime, config, contentLength, fileName, true);
        //    var signedKey = CreateSharedKey(config, challengeString);

        //    var httpClient = new HttpClient();
        //    httpClient.DefaultRequestHeaders.Add("x-ms-client-request-id", new[] {clientRequestId});
        //    httpClient.DefaultRequestHeaders.Add("x-ms-date", new[] {requestTime});
        //    httpClient.DefaultRequestHeaders.Add("x-ms-version", "2015-07-08");
        //    httpClient.DefaultRequestHeaders.Add("x-ms-blob-type", "BlockBlob");

        //    httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"SharedKey {config.AzureStorageAccountName}:{signedKey}");

        //    var uri = new Uri($"https://{config.AzureStorageAccountName}.blob.core.windows.net/{config.AzureStorageContainerName}/{fileName}");

        //    try
        //    {
        //        var response = await httpClient.PutAsync(uri, content).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //}


        //private void AppendIfEmpty(StringBuilder builder, string value)
        //{
        //    builder.Append(string.IsNullOrWhiteSpace(value) ? "\n" : $"{value}\n");
        //}

        private void AppendIfEmpty(StringBuilder builder, int value)
        {
            builder.Append(value == 0 ? "\n" : $"{value}\n");
        }
    }
}