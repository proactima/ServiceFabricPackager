using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SFPackager.Models;

namespace SFPackager.Interfaces
{
    public interface IHandleClusterConnection
    {
        Task Init(ClusterConfig clusterConfig, BaseConfig buildConfig);
        Task<List<ServiceFabricApplication>> GetApplicationManifestsAsync();
    }
}