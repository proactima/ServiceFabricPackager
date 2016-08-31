using System.Threading.Tasks;
using SFPackager.Models;

namespace SFPackager.Interfaces
{
    public interface IHandleFiles
    {
        Task<Response<string>> GetFileAsStringAsync(string fileName);
        Task<Response<byte[]>> GetFileAsBytesAsync(string fileName);
        Task<Response<string>> SaveFileAsync(string fileName, string content);
    }
}