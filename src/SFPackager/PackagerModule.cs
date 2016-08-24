using Ninject.Modules;
using SFPackager.Interfaces;
using SFPackager.Services;
using SFPackager.Services.FileStorage;

namespace SFPackager
{
    public class PackagerModule : NinjectModule
    {
        public override void Load()
        {
            Bind<App>().ToSelf();
            Bind<IHandleFiles>().To<AzureBlobService>();
            Bind<ManifestParser>().ToSelf();
            Bind<ServiceHashCalculator>().ToSelf();
            Bind<SfLocator>().ToSelf();
            Bind<SfProjectHandler>().ToSelf();
            Bind<VersionHandler>().ToSelf();
            Bind<VersionService>().ToSelf();
            Bind<Packager>().ToSelf();
        }
    }
}