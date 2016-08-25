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
        public string ApplicationTypeName { get; set; }
        public string ServiceTypeName { get; set; }
        public string EndpointName { get; set; }
        public string CertThumbprint { get; set; }
        public string CertificateName { get; set; }
    }

    public class ClusterConfig
    {
        public string Endpoint { get; set; }
        public int Port { get; set; }
        public string PfxFile { get; set; }
        public string PfxKey { get; set; }
    }
}