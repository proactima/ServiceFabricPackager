using System.Collections.Generic;

namespace SFPackager.Models
{
    public class ServiceFabricApplicationProject
    {
        public string ProjectFolder { get; set; }
        public string ProjectFile { get; set; }
        public string ProjectFileFullPath => $"{ProjectFolder}\\{ProjectFile}";
        public string ApplicationManifestFile => "ApplicationManifest.xml";
        public string ApplicationManifestPath { get; set; }
        public string ApplicationManifestFileFullPath => $"{ApplicationManifestPath}{ApplicationManifestFile}";
        public Dictionary<string, ServiceFabricServiceProject> Services { get; set; } = new Dictionary<string, ServiceFabricServiceProject>();
        public string ApplicationTypeName { get; set; }
        public string ApplicationTypeVersion { get; set; }
        public string BasePath { get; set; }
        public string BuildConfiguration { get; set; }
        public string PackageBasePath => $"{BasePath}\\sfpackaging\\";
        public string PackagePath => $"{PackageBasePath}{ApplicationTypeName}";
        public string AppManifestPackageTarget => $"{PackagePath}\\{ApplicationManifestFile}";
        public string BuildOutputPathSuffix => $"bin\\x64\\{BuildConfiguration}";
    }
}