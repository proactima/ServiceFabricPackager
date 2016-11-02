using System.IO;

namespace SFPackager.Models
{
    public class AppConfig
    {
        public bool UseAzureStorage { get; set; }
        public string AzureStorageAccountName { get; set; }
        public string AzureStorageAccountSecret { get; set; }
        public string AzureStorageAccountContainer { get; set; }
        public bool UseLocalStorage { get; set; }
        public DirectoryInfo LocalStoragePath { get; set; }
        public string ConfigFileName { get; set; }
        public FileInfo SolutionFile { get; set; }
        public DirectoryInfo SourcePath { get; set; }
        public string BuildConfiguration { get; set; }
        public string UniqueVersionIdentifier { get; set; }
        public bool UseSecureCluster { get; set; }
        public bool ForcePackageAll { get; set; }
        public bool CleanOutputFolder { get; set; }
        public DirectoryInfo PackageOutputPath { get; set; }
        public bool VerboseOutput { get; set; }
        public DirectoryInfo SelfPath { get; set; }

        internal static AppConfig ValidateAndCreate(AppConfigRaw rawConfig)
        {
            if(IsNoStorageOptionsSet(rawConfig) || IsBothStorageOptionsSet(rawConfig))
                return new InvalidAppConfig("Invalid storage settings");

            if(rawConfig.UseAzureStorage.HasValue() && !AllAzureSettingsHasValue(rawConfig))
                return new InvalidAppConfig("Missing Azure Storage settings");

            if (rawConfig.UseLocalStorage.HasValue() && !AllLocalSettingsHasValue(rawConfig))
                return new InvalidAppConfig("Missing Local Storage settings");

            if(!rawConfig.ConfigFileName.HasValue())
                return new InvalidAppConfig("Missing config file name");

            if (!rawConfig.SolutionFile.HasValue())
                return new InvalidAppConfig("Missing solution file name");

            if (!rawConfig.UniqueVersionIdentifier.HasValue())
                return new InvalidAppConfig("Missing Unique Version Identifier");
            
            var config = new AppConfig
            {
                UseAzureStorage = rawConfig.UseAzureStorage.HasValue(),
                AzureStorageAccountName = rawConfig.AzureStorageAccountName.Value(),
                AzureStorageAccountSecret = rawConfig.AzureStorageAccountSecret.Value(),
                AzureStorageAccountContainer = rawConfig.AzureStorageAccountContainer.Value(),
                UseLocalStorage = rawConfig.UseLocalStorage.HasValue(),
                LocalStoragePath = rawConfig.LocalStoragePath.HasValue() ? new DirectoryInfo(rawConfig.LocalStoragePath.Value()) : new DirectoryInfo("c:"),
                ConfigFileName = rawConfig.ConfigFileName.Value(),
                SolutionFile = new FileInfo(rawConfig.SolutionFile.Value()),
                BuildConfiguration = rawConfig.BuildConfiguration.HasValue() ? rawConfig.BuildConfiguration.Value() : "Release",
                UniqueVersionIdentifier = rawConfig.UniqueVersionIdentifier.Value(),
                UseSecureCluster = rawConfig.UseSecureCluster.HasValue(),
                ForcePackageAll = rawConfig.ForcePackageAll.HasValue(),
                CleanOutputFolder = rawConfig.CleanOutputFolder.HasValue(),
                PackageOutputPath = GetPackageOutputPath(rawConfig),
                VerboseOutput = rawConfig.VerboseOutput.HasValue(),
                SelfPath = new DirectoryInfo(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location)),
            };
            
            config.SourcePath = config.SolutionFile.Directory;

            return config;
        }

        private static DirectoryInfo GetPackageOutputPath(AppConfigRaw rawConfig)
        {
            return rawConfig.PackageOutputPath.HasValue()
                ? new DirectoryInfo(rawConfig.PackageOutputPath.Value())
                : new DirectoryInfo(Path.Combine(rawConfig.SolutionFile.Value(), "sfpackaging"));
        }

        private static bool AllLocalSettingsHasValue(AppConfigRaw rawConfig)
        {
            return rawConfig.LocalStoragePath.HasValue();
        }

        private static bool AllAzureSettingsHasValue(AppConfigRaw rawConfig)
        {
            return rawConfig.AzureStorageAccountContainer.HasValue()
                && rawConfig.AzureStorageAccountName.HasValue()
                && rawConfig.AzureStorageAccountSecret.HasValue();
        }

        private static bool IsBothStorageOptionsSet(AppConfigRaw rawConfig)
        {
            return rawConfig.UseAzureStorage.HasValue()
                && rawConfig.UseLocalStorage.HasValue();
        }

        private static bool IsNoStorageOptionsSet(AppConfigRaw rawConfig)
        {
            return !rawConfig.UseAzureStorage.HasValue()
                && !rawConfig.UseLocalStorage.HasValue();
        }
    }

    internal class InvalidAppConfig : AppConfig
    {
        public InvalidAppConfig(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
    }
}