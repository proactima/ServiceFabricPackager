using Ninject.Modules;
using SFPackager.Services;

namespace SFPackager
{
    public class PackagerModule : NinjectModule
    {
        public override void Load()
        {
            Bind<App>().ToSelf();
            Bind<AzureBlobService>().ToSelf();
            Bind<ServiceFabricApplicationManifestHandler>().ToSelf();
            Bind<ServiceFabricRemote>().ToSelf().InSingletonScope();
            Bind<ServiceFabricServiceManifestHandler>().ToSelf();
            Bind<ServiceHashCalculator>().ToSelf();
            Bind<SfLocator>().ToSelf();
            Bind<SfProjectHandler>().ToSelf();
            Bind<VersionHandler>().ToSelf();
            Bind<VersionService>().ToSelf();
            Bind<Packager>().ToSelf();
        }
    }
}