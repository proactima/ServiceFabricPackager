using System.Collections.Generic;

namespace SFPackager.Models
{
    public class Package
    {
        public List<PackageApplication> PackageApplications { get; set; } = new List<PackageApplication>();
    }

    public class PackageApplication
    {
        public VersionNumber Version { get; set; }
        public string ApplicationTypeName { get; set; }
        public List<PackageService> PackageServices { get; set; } = new List<PackageService>();
    }

    public class PackageService
    {
        public VersionNumber Version { get; set; }
        public bool IncludeInPackage { get; set; } = false;
        public string ServiceTypeName { get; set; }
    }
}