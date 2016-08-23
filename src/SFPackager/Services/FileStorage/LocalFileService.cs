using System;
using System.Threading.Tasks;
using SFPackager.Interfaces;
using SFPackager.Models;

namespace SFPackager.Services.FileStorage
{
    public class LocalFileService : IHandleFiles
    {
        public Task<Response<string>> GetFileAsStringAsync(string fileName, BaseConfig baseConfig)
        {
            throw new NotImplementedException();
        }

        public Task<Response<byte[]>> GetFileAsBytesAsync(string fileName, BaseConfig baseConfig)
        {
            throw new NotImplementedException();
        }

        public Task<Response<string>> SaveFileAsync(string fileName, string content, BaseConfig baseConfig)
        {
            throw new NotImplementedException();
        }
    }
}