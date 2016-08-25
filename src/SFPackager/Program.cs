using System;
using System.Threading.Tasks;
using SFPackager.Models;

namespace SFPackager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                if (args.Length != 7)
                {
                    Console.WriteLine("You must pass the following args for it to work:");
                    Console.WriteLine("Azure Storage Account Name");
                    Console.WriteLine("Azure Storage Account Key");
                    Console.WriteLine("Azure Storage Container Name");
                    Console.WriteLine("Azure Storage Config File Name");
                    Console.WriteLine("Source base path");
                    Console.WriteLine("Build configuration");
                    Console.WriteLine("Commit Hash");
                    Console.WriteLine();
                    Console.WriteLine(
                        "Example: sfpackager.exe myaccount \"mysupersecretkey\" mycontainer myconfigfile \"c:\\temp\\mystuff\" Debug fdjfs8af3ah88f");
                    return;
                }

                var config = new BaseConfig
                {
                    AzureStorageAccountName = args[0],
                    AzureStorageAccountKey = args[1],
                    AzureStorageContainerName = args[2],
                    AzureStorageConfigFileName = args[3],
                    SourceBasePath = args[4],
                    BuildConfiguration = args[5],
                    CommitHash = args[6]
                };

                MainAsync(config).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Done!");
            Console.ReadLine();
        }

        private static async Task MainAsync(BaseConfig baseConfig)
        {
            var container = SimpleInjectorSetup.Configure(baseConfig);
            var app = container.GetInstance<App>();

            try
            {
                await app.RunAsync(baseConfig).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}