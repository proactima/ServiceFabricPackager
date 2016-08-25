using System.Collections.Generic;

namespace SFPackager.Models
{
    public class ServiceFabricServiceProject
    {
        public string ProjectFolder { get; set; }
        public string ProjectFile { get; set; }
        public string ProjectFileFullPath => $"{ProjectFolder}{ProjectFile}";
        public string ServiceManifestFile => "ServiceManifest.xml";
        public string PackageRoot { get; set; }
        public string ServiceManifestFileFullPath => $"{PackageRoot}{ServiceManifestFile}";
        public string ServiceName { get; set; }
        public string ServiceVersion { get; set; }
        public bool IsAspNetCore { get; set; }
        public List<SubPackage> SubPackages { get; set; } = new List<SubPackage>();
    }
}