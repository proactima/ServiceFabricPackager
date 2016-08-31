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
        public Task<Response<string>> GetFileAsStringAsync(string fileName)
        {
            try
            {
                if (!File.Exists(fileName))
                    return Task.FromResult(new Response<string>
                    {
                        Operation = BlobOperation.GET,
                        ErrorMessage = $"File does not exist: {fileName}",
                        StatusCode = HttpStatusCode.NotFound
                    });

                var fileContent = File.ReadAllText(fileName);
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
            try
            {
                if (!File.Exists(fileName))
                    return Task.FromResult(new Response<byte[]>
                    {
                        Operation = BlobOperation.GET,
                        ErrorMessage = $"File does not exist: {fileName}",
                        StatusCode = HttpStatusCode.NotFound
                    });

                var fileContent = File.ReadAllBytes(fileName);
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
            try
            {
                File.WriteAllText(fileName, content, Encoding.UTF8);
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