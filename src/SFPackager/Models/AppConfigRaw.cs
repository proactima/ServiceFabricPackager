using Microsoft.Extensions.CommandLineUtils;

namespace SFPackager.Models
{
    public class AppConfigRaw
    {
        public CommandOption UseAzureStorage { get; set; }
        public CommandOption AzureStorageAccountName { get; set; }
        public CommandOption AzureStorageAccountSecret { get; set; }
        public CommandOption AzureStorageAccountContainer { get; set; }
        public CommandOption UseLocalStorage { get; set; }
        public CommandOption LocalStoragePath { get; set; }
        public CommandOption ConfigFileName { get; set; }
        public CommandOption SourcePath { get; set; }
        public CommandOption BuildConfiguration { get; set; }
        public CommandOption UniqueVersionIdentifier { get; set; }
        public CommandOption UseSecureCluster { get; set; }
        public CommandOption ForcePackageAll { get; set; }
        public CommandOption CleanOutputFolder { get; set; }
        public CommandOption PackageOutputPath { get; set; }
    }
}