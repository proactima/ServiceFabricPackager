using SFPackager.Interfaces;
using SFPackager.Models;
using SFPackager.Services.Cluster;
using SFPackager.Services.FileStorage;
using SimpleInjector;

namespace SFPackager
{
    public static class SimpleInjectorSetup
    {
        public static Container Configure(BaseConfig config)
        {
            var container = new Container();

            container.Register<IHandleFiles, AzureBlobService>();
            
            if (config.UseSecureCluster)
                container.Register<IHandleClusterConnection, SecureClusterConnection>();
            else
                container.Register<IHandleClusterConnection, NonSecureClusterConnection>();

            container.Verify();

            return container;
        }
    }
}