using System.Collections.Generic;
using System.IO;

namespace SFPackager.Models
{
    public class ServiceFabricServiceProject
    {
        public DirectoryInfo ProjectFolder { get; set; }
        public FileInfo ProjectFile { get; set; }
        public string ServiceManifestFile => "ServiceManifest.xml";
        public DirectoryInfo PackageRoot => new DirectoryInfo(Path.Combine(ProjectFolder.FullName, "PackageRoot"));
        public string SourceServiceManifestPath => Path.Combine(PackageRoot.FullName, ServiceManifestFile);
        public string ServiceName { get; set; }
        public string ServiceVersion { get; set; }
        public bool IsAspNetCore { get; set; }
        public List<SubPackage> SubPackages { get; set; } = new List<SubPackage>();

        public DirectoryInfo GetServicePackageFolder(DirectoryInfo basePackagePath)
        {
            var path = Path.Combine(basePackagePath.FullName, ServiceName);
            return new DirectoryInfo(path);
        }

        public FileInfo GetServiceManifestTargetFile(DirectoryInfo serviceManifestFolder)
        {
            var path = Path.Combine(serviceManifestFolder.FullName, ServiceManifestFile);
            return new FileInfo(path);
        }
    }
}