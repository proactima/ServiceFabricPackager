using System.Collections.Generic;

namespace SFPackager.Models
{
    public class GlobalVersion
    {
        public VersionNumber Version { get; set; }
        public string Hash { get; set; } = string.Empty;
        public VersionType VersionType { get; set; }
        public PackageType PackageType { get; set; } = PackageType.None;
        public string ParentRef { get; set; } = string.Empty;
        public bool IncludeInPackage { get; set; } = false;
        public string Name { get; set; } = string.Empty;
    }
}