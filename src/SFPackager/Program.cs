using System;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using SFPackager.Models;
using SFPackager.Services;

namespace SFPackager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var app = new CommandLineApplication
            {
                Name = "SFPackager",
            };
            app.HelpOption("-?|-h|--help");

            var config = new AppConfigRaw
            {
                UseAzureStorage = app.Option("-a|--azureStorage", "", CommandOptionType.NoValue),
                AzureStorageAccountName = app.Option("-n|--storageAccountName", "", CommandOptionType.SingleValue),
                AzureStorageAccountSecret = app.Option("-k|--storageAccountKey", "", CommandOptionType.SingleValue),
                AzureStorageAccountContainer = app.Option("-c|--storageAccountContainer", "", CommandOptionType.SingleValue),
                UseLocalStorage = app.Option("-l|--localStorage", "", CommandOptionType.NoValue),
                LocalStoragePath = app.Option("-p|--localStoragePath", "", CommandOptionType.SingleValue),
                ConfigFileName = app.Option("-f|--configFileName", "", CommandOptionType.SingleValue),
                SourcePath = app.Option("-s|--sourcePath", "", CommandOptionType.SingleValue),
                BuildConfiguration = app.Option("-b|--buildConfiguration", "Default is Release", CommandOptionType.SingleValue),
                UniqueVersionIdentifier = app.Option("-i|--versionIdentifier", "A reference to the version that is being packaged, i.e. the commit hash", CommandOptionType.SingleValue),
                UseSecureCluster = app.Option("-e|--secureCluster", "", CommandOptionType.NoValue),
                ForcePackageAll = app.Option("-x|--forcePackage", "", CommandOptionType.NoValue),
                CleanOutputFolder = app.Option("-d|--cleanOutput", "Clean packaging folder before packaging", CommandOptionType.NoValue),
                PackageOutputPath = app.Option("-o|--packageOutput", "Path to package to. Defaults to SOURCEPATH\\sfpackaging", CommandOptionType.SingleValue),
            };
            
            var errCode = app.Execute(args);
            if (errCode != 0)
            {
                app.ShowHelp();
                return;
            }

            var appConfig = AppConfig.ValidateAndCreate(config);
            if (appConfig is InvalidAppConfig)
            {
                app.ShowHelp();
                return;
            }

            var parser = new CommandLineParser.CommandLineParser();
            var target = new CmdLineOptions();
            parser.ExtractArgumentAttributes(target);

            try
            {
                parser.ParseCommandLine(args);
            }
            catch (Exception)
            {
                parser.ShowUsage();
                return;
            }

            try
            {
                MainAsync(target).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Done!");
            Console.ReadLine();
        }

        private static async Task MainAsync(CmdLineOptions baseConfig)
        {
            var setupContainer = SimpleInjectorSetup.GetSetupContainer(baseConfig);
            var configManager = setupContainer.GetInstance<ConfigManager>();
            var packageConfig = await configManager.GetPackageConfig().ConfigureAwait(false);

            var container = SimpleInjectorSetup.Configure(baseConfig, packageConfig);

            try
            {
                var app = container.GetInstance<App>();
                await app.RunAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}