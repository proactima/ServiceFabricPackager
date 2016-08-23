namespace SFPackager.Models
{
    public class BaseConfig
    {
        public string AzureStorageAccountKey { get; set; }
        public string AzureStorageAccountName { get; set; }
        public string AzureStorageContainerName { get; set; }
        public string AzureStorageConfigFileName { get; set; }
        public string SourceBasePath { get; set; }
        public string BuildConfiguration { get; set; }
        public string CommitHash { get; set; }
    }
}