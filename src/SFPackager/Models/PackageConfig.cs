using System.Collections.Generic;

namespace SFPackager.Models
{
    public class PackageConfig
    {
        public List<HttpsConfig> Https { get; set; }
        public List<ExternalInclude> ExternalIncludes { get; set; }
        public ClusterConfig Cluster { get; set; }
        public List<EndpointConfig> Endpoints { get; set; }
        public List<string> HashIncludeExtensions { get; set; }
        public List<string> HashSpecificExludes { get; set; }
        public List<Encipherment> Encipherment { get; set; }
        public List<GuestExecutable> GuestExecutables { get; set; }
    }

    public class GuestExecutable
    {
        public string PackageName { get; set; }
        public string ApplicationTypeName { get; set; }
        public GuestRunAs GuestRunAs { get; set; }
    }

    public class GuestRunAs
    {
        public string UserName { get; set; }
        public string AccountType { get; set; }
    }

    public class Encipherment
    {
        public string ApplicationTypeName { get; set; }
        public string Name { get; set; }
        public string CertName { get; set; }
        public string CertThumbprint { get; set; }
    }

    public class EndpointConfig
    {
        public string ApplicationTypeName { get; set; }
        public string ServiceManifestName { get; set; }
        public string EndpointName { get; set; }
        public int Port { get; set; }
        public string Protocol { get; set; }
        public string Type { get; set; }
    }

    public class ExternalInclude
    {
        public string ApplicationTypeName { get; set; }
        public string ServiceManifestName { get; set; }
        public string PackageName { get; set; }
        public string SourceFileName { get; set; }
        public string TargetFileName { get; set; }
    }

    public class HttpsConfig
    {
        public string ApplicationTypeName { get; set; }
        public string ServiceManifestName { get; set; }
        public string EndpointName { get; set; }
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