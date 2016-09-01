using System;
using System.Threading.Tasks;
using SFPackager.Models;
using SFPackager.Services;

namespace SFPackager
{
    public class Program
    {
        public static void Main(string[] args)
        {
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