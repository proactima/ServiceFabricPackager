using SFPackager.Interfaces;
using SFPackager.Models;
using SFPackager.Services.Cluster;
using SFPackager.Services.FileStorage;
using SimpleInjector;

namespace SFPackager
{
    public static class SimpleInjectorSetup
    {
        public static Container Configure(BaseConfig baseConfig, PackageConfig packageConfig)
        {
            var container = new Container();

            container.Register<IHandleFiles, AzureBlobService>();
            
            if (baseConfig.UseSecureCluster)
                container.Register<IHandleClusterConnection, SecureClusterConnection>();
            else
                container.Register<IHandleClusterConnection, NonSecureClusterConnection>();

            container.RegisterSingleton(baseConfig);
            container.RegisterSingleton(packageConfig);

            container.Verify();

            return container;
        }

        public static void RegisterPackageConfig(Container container, PackageConfig config)
        {
            container.RegisterSingleton(config);
        }

        public static Container GetSetupContainer(BaseConfig baseConfig)
        {
            var container = new Container();
            container.Register<IHandleFiles, AzureBlobService>();
            container.RegisterSingleton(baseConfig);

            return container;
        }
    }
}