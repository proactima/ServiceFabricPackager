using System.Collections.Generic;

namespace SFPackager.Models
{
    public class PackageConfig
    {
        public List<HttpsConfig> Https { get; set; }
        public ClusterConfig Cluster { get; set; }
    }

    public class HttpsConfig
    {
        public string ServiceName { get; set; }
        public string Endpoint { get; set; }
        public string CertThumbprint { get; set; }
    }

    public class ClusterConfig
    {
        public string Endpoint { get; set; }
        public int Port { get; set; }
        public string PfxFile { get; set; }
        public string PfxKey { get; set; }
    }
}