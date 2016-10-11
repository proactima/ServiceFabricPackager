using System.IO;

namespace SFPackager.Models
{
    public class SubPackage
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Path { get; set; }
        public PackageType PackageType { get; set; }

        public DirectoryInfo GetSubPackageTargetPath(DirectoryInfo servicePackagePath)
        {
            var path = System.IO.Path.Combine(servicePackagePath.FullName, Name);
            return new DirectoryInfo(path);
        }
    }
}