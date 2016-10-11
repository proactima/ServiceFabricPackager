using System.IO;

namespace SFPackager
{
    public enum BlobOperation
    {
        GET,
        PUT
    }

    public enum StorageType
    {
        Azure,
        Local
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

    public enum LogLevel
    {
        Error,
        Warning,
        Info,
        Debug
    }

    public static class Constants
    {
        public const string GlobalIdentifier = "##GLOBAL##";
    }
}