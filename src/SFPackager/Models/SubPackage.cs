namespace SFPackager.Models
{
    public class SubPackage
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Path { get; set; }
        public PackageType PackageType { get; set; }
    }
}