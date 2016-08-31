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
        private readonly BaseConfig _baseConfig;

        public AzureBlobService(BaseConfig baseConfig)
        {
            _baseConfig = baseConfig;
        }

        public async Task<Response<string>> GetFileAsStringAsync(string fileName)
        {
            return await ExecuteFileOperationAsync(BlobOperation.GET, fileName)
                .ConfigureAwait(false);
        }

        public async Task<Response<byte[]>> GetFileAsBytesAsync(string fileName)
        {
            return await GetBytesFromBlob(fileName)
                .ConfigureAwait(false);
        }

        public async Task<Response<string>> SaveFileAsync(string fileName, string content)
        {
            return await ExecuteFileOperationAsync(BlobOperation.PUT, fileName, content)
                .ConfigureAwait(false);
        }

        private async Task<Response<string>> ExecuteFileOperationAsync(BlobOperation operation,
            string targetFilename, string payload = "")
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

            var challengeString = CreateChallengeString(verb, clientRequestId, requestTime, contentLength,
                targetFilename, operation == BlobOperation.PUT);
            var sharedKey = CreateSharedKey( challengeString);
            var uri =
                new Uri(
                    $"https://{_baseConfig.AzureStorageAccountName}.blob.core.windows.net/{_baseConfig.AzureStorageContainerName}/{targetFilename}");

            var httpClient = new HttpClient();
            AddHeaders(httpClient, clientRequestId, requestTime, sharedKey, blobType);

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

        private async Task<Response<byte[]>> GetBytesFromBlob(string targetFilename)
        {
            var clientRequestId = Guid.NewGuid().ToString();
            var requestTime = DateTime.Now.ToString("R", CultureInfo.InvariantCulture);
            var blobType = "";
            var verb = Enum.GetName(typeof(BlobOperation), BlobOperation.GET);
            var contentLength = 0;

            var challengeString = CreateChallengeString(verb, clientRequestId, requestTime, contentLength,
                targetFilename);
            var sharedKey = CreateSharedKey(challengeString);
            var uri =
                new Uri(
                    $"https://{_baseConfig.AzureStorageAccountName}.blob.core.windows.net/{_baseConfig.AzureStorageContainerName}/{targetFilename}");

            var httpClient = new HttpClient();
            AddHeaders(httpClient, clientRequestId, requestTime, sharedKey, blobType);

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

        private string CreateSharedKey(string challengeString)
        {
            var accessKey = Convert.FromBase64String(_baseConfig.AzureStorageAccountKey);
            var hasher = new HMACSHA256(accessKey);

            var hashed = hasher.ComputeHash(Encoding.UTF8.GetBytes(challengeString));
            var base64 = Convert.ToBase64String(hashed);

            return base64;
        }

        private string CreateChallengeString(string verb, string requestId, string requestTime,
            int contentLength, string fileName, bool isPut = false)
        {
            var canonicalizedResourceUri =
                $"/{_baseConfig.AzureStorageAccountName}/{_baseConfig.AzureStorageContainerName}/{fileName}";

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

        private void AddHeaders(HttpClient client, string clientRequestId, string requestTime,
            string signedHeader, string blobType = "")
        {
            client.DefaultRequestHeaders.Add("x-ms-client-request-id", new[] {clientRequestId});
            client.DefaultRequestHeaders.Add("x-ms-date", new[] {requestTime});
            client.DefaultRequestHeaders.Add("x-ms-version", "2015-07-08");
            if (!string.IsNullOrWhiteSpace(blobType))
                client.DefaultRequestHeaders.Add("x-ms-blob-type", blobType);

            client.DefaultRequestHeaders.Authorization =
                AuthenticationHeaderValue.Parse($"SharedKey {_baseConfig.AzureStorageAccountName}:{signedHeader}");
        }
        
        private static void AppendIfEmpty(StringBuilder builder, int value)
        {
            builder.Append(value == 0 ? "\n" : $"{value}\n");
        }
    }
}