using System.Collections.Generic;
using System.IO;

namespace SFPackager.Models
{
    public class ServiceFabricApplicationProject
    {
        public string ProjectFolder { get; set; }
        public string ProjectFile { get; set; }
        public string ProjectFileFullPath => Path.Combine(ProjectFolder, ProjectFile);
        public string ApplicationManifestFile => "ApplicationManifest.xml";
        public string ApplicationManifestPath { get; set; }
        public string ApplicationManifestFileFullPath => Path.Combine(ApplicationManifestPath, ApplicationManifestFile);
        public Dictionary<string, ServiceFabricServiceProject> Services { get; set; } = new Dictionary<string, ServiceFabricServiceProject>();
        public string ApplicationTypeName { get; set; }
        public string ApplicationTypeVersion { get; set; }
        public string BasePath { get; set; }
        public string BuildConfiguration { get; set; }
        public string BuildOutputPathSuffix => Path.Combine("bin", "x64", BuildConfiguration);

        public DirectoryInfo GetPackagePath(DirectoryInfo baseOutputPath)
        {
            var path = Path.Combine(baseOutputPath.FullName, ApplicationTypeName);
            return new DirectoryInfo(path);
        }

        public FileInfo GetAppManifestTargetFile(DirectoryInfo applicationPackagePath)
        {
            var path = Path.Combine(applicationPackagePath.FullName, ApplicationManifestFile);
            return new FileInfo(path);
        }
    }
}