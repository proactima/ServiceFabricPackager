﻿using SFPackager.Interfaces;
using SFPackager.Models;
using SFPackager.Models.Xml;
using SFPackager.Services.Cluster;
using SFPackager.Services.FileStorage;
using SFPackager.Services.Manifest;
using SimpleInjector;

namespace SFPackager
{
    public static class SimpleInjectorSetup
    {
        public static Container Configure(AppConfig baseConfig, PackageConfig packageConfig)
        {
            var container = new Container();

            if (baseConfig.UseAzureStorage)
                container.Register<IHandleFiles, AzureBlobService>();
            if (baseConfig.UseLocalStorage)
                container.Register<IHandleFiles, LocalFileService>();

            if (baseConfig.UseSecureCluster)
                container.Register<IHandleClusterConnection, SecureClusterConnection>();
            else
                container.Register<IHandleClusterConnection, NonSecureClusterConnection>();

            container.RegisterSingleton(baseConfig);
            container.RegisterSingleton(packageConfig);

            container.Register(() => new ManifestLoader<ApplicationManifest>());
            container.Register(() => new ManifestLoader<ServiceManifest>());
            container.Register(() => new ManifestLoader<CoreProjectFile>(false));

            container.Verify();

            return container;
        }

        public static void RegisterPackageConfig(Container container, PackageConfig config)
        {
            container.RegisterSingleton(config);
        }

        public static Container GetSetupContainer(AppConfig baseConfig)
        {
            var container = new Container();

            if (baseConfig.UseAzureStorage)
                container.Register<IHandleFiles, AzureBlobService>();
            if(baseConfig.UseLocalStorage)
                container.Register<IHandleFiles, LocalFileService>();

            container.RegisterSingleton(baseConfig);

            return container;
        }
    }
}