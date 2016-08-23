using System.IO;

namespace SFPackager
{
    public enum BlobOperation
    {
        GET,
        PUT
    }

    public enum VersionType
    {
        Global,
        Application,
        Service,
        ServicePackage
    }

    public enum PackageType
    {
        None,
        Code,
        Config,
        Data
    }

    public static class Constants
    {
        public const string GlobalIdentifier = "##GLOBAL##";

        public static bool IncludeFileFilter(FileInfo x)
        {
            return x.FullName.ToLowerInvariant().EndsWith(".dll")
                   || x.FullName.ToLowerInvariant().EndsWith(".exe")
                   || x.FullName.ToLowerInvariant().EndsWith(".config");
        }

        public static bool IgnoreList(FileInfo x)
        {
            return !x.Name.ToLowerInvariant().Equals("common.resources.dll");
        }
    }
}