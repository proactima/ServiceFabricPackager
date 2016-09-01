using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SFPackager.Interfaces;
using SFPackager.Models;

namespace SFPackager.Services.FileStorage
{
    public class LocalFileService : IHandleFiles
    {
        private readonly CmdLineOptions _baseConfig;

        public LocalFileService(CmdLineOptions baseConfig)
        {
            _baseConfig = baseConfig;
        }

        public Task<Response<string>> GetFileAsStringAsync(string fileName)
        {
            var filePath = $"{_baseConfig.LocalConfigFolder}\\{fileName}";

            try
            {
                if (!File.Exists(filePath))
                    return Task.FromResult(new Response<string>
                    {
                        Operation = BlobOperation.GET,
                        ErrorMessage = $"File does not exist: {filePath}",
                        StatusCode = HttpStatusCode.NotFound
                    });

                var fileContent = File.ReadAllText(filePath);
                return Task.FromResult(new Response<string>
                {
                    Operation = BlobOperation.GET,
                    ResponseContent = fileContent,
                    StatusCode = HttpStatusCode.OK
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new Response<string>
                {
                    Operation = BlobOperation.GET,
                    ErrorMessage = ex.Message,
                    StatusCode = HttpStatusCode.InternalServerError
                });
            }
        }

        public Task<Response<byte[]>> GetFileAsBytesAsync(string fileName)
        {
            var filePath = $"{_baseConfig.LocalConfigFolder}\\{fileName}";

            try
            {
                if (!File.Exists(filePath))
                    return Task.FromResult(new Response<byte[]>
                    {
                        Operation = BlobOperation.GET,
                        ErrorMessage = $"File does not exist: {filePath}",
                        StatusCode = HttpStatusCode.NotFound
                    });

                var fileContent = File.ReadAllBytes(filePath);
                return Task.FromResult(new Response<byte[]>
                {
                    Operation = BlobOperation.GET,
                    ResponseContent = fileContent,
                    StatusCode = HttpStatusCode.OK
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new Response<byte[]>
                {
                    Operation = BlobOperation.GET,
                    ErrorMessage = ex.Message,
                    StatusCode = HttpStatusCode.InternalServerError
                });
            }
        }

        public Task<Response<string>> SaveFileAsync(string fileName, string content)
        {
            var filePath = $"{_baseConfig.LocalConfigFolder}\\{fileName}";

            try
            {
                File.WriteAllText(filePath, content, Encoding.UTF8);
                return Task.FromResult(new Response<string>
                {
                    Operation = BlobOperation.PUT,
                    StatusCode = HttpStatusCode.OK,
                    ResponseContent = string.Empty
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new Response<string>
                {
                    Operation = BlobOperation.PUT,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessage = ex.Message
                });
            }
        }
    }
}