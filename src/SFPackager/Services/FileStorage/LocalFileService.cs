using System;
using System.Threading.Tasks;
using SFPackager.Interfaces;
using SFPackager.Models;

namespace SFPackager.Services.FileStorage
{
    public class LocalFileService : IHandleFiles
    {
        public Task<Response<string>> GetFileAsStringAsync(string fileName)
        {
            throw new NotImplementedException();
        }

        public Task<Response<byte[]>> GetFileAsBytesAsync(string fileName)
        {
            throw new NotImplementedException();
        }

        public Task<Response<string>> SaveFileAsync(string fileName, string content)
        {
            throw new NotImplementedException();
        }
    }
}