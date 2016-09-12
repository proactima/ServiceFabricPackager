using CommandLineParser.Arguments;
using CommandLineParser.Validation;

namespace SFPackager.Models
{
    [ArgumentGroupCertification("a,l", EArgumentGroupCondition.ExactlyOneUsed)]
    [DistinctGroupsCertification("n,k,c","p")]
    public class CmdLineOptions
    {
        [SwitchArgument('a', "azure", false, Description = "Use Azure Blob as backing store")]
        public bool UseAzureStorage { get; set; }

        [ValueArgument(typeof(string), 'n', "storageaccountname", Description = "Azure Store Account Name")]
        public string AzureStorageAccountName { get; set; }

        [ValueArgument(typeof(string), 'k', "storageaccountkey", Description = "Azure Store Account Key")]
        public string AzureStorageAccountKey { get; set; }

        [ValueArgument(typeof(string), 'c', "storageaccountcontainer", Description = "Azure Store Account Container")]
        public string AzureStorageAccountContainer { get; set; }

        [SwitchArgument('l', "local", false, Description = "Use local folder as backing store")]
        public bool UseLocalStorage { get; set; }

        [ValueArgument(typeof(string), 'p', "localfolder", Description = "Local config folder fullpath")]
        public string LocalConfigFolder { get; set; }

        [ValueArgument(typeof(string), 'f', "configfilename", Description = "Config file to use", Optional = false)]
        public string ConfigFileName { get; set; }

        [ValueArgument(typeof(string), 's', "sourcepath", Description = "Path to source folder", Optional = false)]
        public string SourceBasePath { get; set; }

        [ValueArgument(typeof(string), 'b', "buildconfiguration", Description = "Build configuration", Optional = true, DefaultValue = "Release")]
        public string BuildConfiguration { get; set; }

        [ValueArgument(typeof(string), 'h', "commithash", Description = "Commit Hash", Optional = false)]
        public string CommitHash { get; set; }

        [SwitchArgument('e', "secure", false, Description = "Cluster is secure")]
        public bool UseSecureCluster { get; set; }

        [SwitchArgument('x', "forceall", false, Description = "Force package all items")]
        public bool ForcePackageAll { get; set; }

        [SwitchArgument('d', "cleanpackage", true, Description = "Clean package folder before packaging")]
        public bool CleanPackageFolder { get; set; }
    }
}