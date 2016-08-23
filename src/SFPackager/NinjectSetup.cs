using Ninject;
using Ninject.Syntax;
using SFPackager.Interfaces;
using SFPackager.Models;
using SFPackager.Services.Cluster;

namespace SFPackager
{
    public static class NinjectSetup
    {
        public static IReadOnlyKernel Configure(BaseConfig config)
        {
            var coreKernel = new KernelConfiguration(new PackagerModule());
            BindClusterConnectionHandler(config, coreKernel);

            var kernel = coreKernel.BuildReadonlyKernel();

            return kernel;
        }

        private static void BindClusterConnectionHandler(BaseConfig config, IBindingRoot coreKernel)
        {
            if (config.UseSecureCluster)
                coreKernel.Bind<IHandleClusterConnection>()
                    .To<SecureClusterConnection>();
            else
                coreKernel.Bind<IHandleClusterConnection>()
                    .To<NonSecureClusterConnection>();
        }
    }
}