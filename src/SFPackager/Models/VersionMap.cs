using System.Collections.Generic;

namespace SFPackager.Models
{
    public class VersionMap
    {
        public Dictionary<string, GlobalVersion> PackageVersions { get; set; }
        public int MapFormatVersion { get; set; } = 1;
    }
}