using System.Threading.Tasks;
using SFPackager.Models;

namespace SFPackager.Interfaces
{
    public interface IHandleFiles
    {
        Task<Response<string>> GetFileAsStringAsync(string fileName, BaseConfig baseConfig);
        Task<Response<byte[]>> GetFileAsBytesAsync(string fileName, BaseConfig baseConfig);
        Task<Response<string>> SaveFileAsync(string fileName, string content, BaseConfig baseConfig);
    }
}