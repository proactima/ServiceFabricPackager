using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SFPackager.Helpers;
using SFPackager.Models;
using SFPackager.Models.Xml;
using SFPackager.Models.Xml.Elements;

namespace SFPackager.Services.Manifest
{
    public class ManifestHandler
    {
        private readonly ManifestLoader<ApplicationManifest> _appManifestLoader;
        private readonly AppConfig _baseConfig;
        private readonly PackageConfig _packageConfig;
        private readonly ManifestLoader<ServiceManifest> _serviceManifestLoader;
        private readonly HandleEnciphermentCert _handleEnciphermentCert;
        private readonly HandleEndpointCert _handleEndpointCert;

        public ManifestHandler(
            AppConfig baseConfig,
            PackageConfig packageConfig,
            ManifestLoader<ApplicationManifest> appManifestLoader,
            ManifestLoader<ServiceManifest> serviceManifestLoader,
            HandleEnciphermentCert handleEnciphermentCert,
            HandleEndpointCert handleEndpointCert)
        {
            _baseConfig = baseConfig;
            _packageConfig = packageConfig;
            _appManifestLoader = appManifestLoader;
            _serviceManifestLoader = serviceManifestLoader;
            _handleEnciphermentCert = handleEnciphermentCert;
            _handleEndpointCert = handleEndpointCert;
        }

        public void Handle(
            Dictionary<string, GlobalVersion> versions,
            Dictionary<string, ServiceFabricApplicationProject> applications)
        {
            var apps = versions
                .Where(x => x.Value.VersionType == VersionType.Application)
                .Where(x => x.Value.IncludeInPackage)
                .ToList();

            foreach (var app in apps)
            {
                var appData = applications[app.Key];
                var appPackagePath = appData.GetPackagePath(_baseConfig.PackageOutputPath);
                var packagedAppManifest = appData.GetAppManifestTargetFile(appPackagePath);

                var appManifest = _appManifestLoader.Load(packagedAppManifest.FullName);

                CleanAppManifest(appManifest);
                SetGeneralInfo(appManifest, versions, app.Value.ToString());
                _handleEndpointCert.SetEndpointCerts(_packageConfig, appManifest, appData.ApplicationTypeName);
                _handleEnciphermentCert.SetEnciphermentCerts(_packageConfig, appManifest, appData.ApplicationTypeName);

                _appManifestLoader.Save(appManifest, packagedAppManifest.FullName);

                var includedServices = versions
                    .Where(x => x.Value.ParentRef.Equals(app.Key))
                    .Where(x => x.Value.IncludeInPackage)
                    .ToList();

                foreach (var service in includedServices)
                {
                    var serviceData = appData.Services[service.Key];

                    var packagedServiceManifest = Path.Combine(appPackagePath.FullName, service.Key,
                        serviceData.ServiceManifestFile);
                    var serviceManifest = _serviceManifestLoader.Load(packagedServiceManifest);

                    SetServiceManifestGeneral(serviceManifest, service.Value);
                    SetServiceEndpoints(serviceManifest, appData.ApplicationTypeName, serviceData.ServiceName);

                    SetServicePackagesData(serviceManifest, versions, service.Key);

                    _serviceManifestLoader.Save(serviceManifest, packagedServiceManifest);
                }
            }
        }

        public void SetServicePackagesData(
            ServiceManifest serviceManifest,
            Dictionary<string, GlobalVersion> versions,
            string parentRef)
        {
            var subPackages = versions
                .Where(x => x.Value.ParentRef.Equals(parentRef))
                .ToList();

            foreach (var package in subPackages)
            {
                var packageName = package.Key.Split('-')[1];

                switch (package.Value.PackageType)
                {
                    case PackageType.Code:
                        serviceManifest.CodePackage.Version = package.Value.Version.ToString();
                        break;
                    case PackageType.Config:
                        serviceManifest.ConfigPackage.Version = package.Value.Version.ToString();
                        break;
                    case PackageType.Data:
                        var dataPackages = serviceManifest
                            .DataPackages
                            .Where(x => x.Name.Equals(packageName))
                            .ToList();
                        if (!dataPackages.Any())
                            return;

                        dataPackages.First().Version = package.Value.Version.ToString();
                        break;
                    case PackageType.None:
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void SetServiceManifestGeneral(
            ServiceManifest serviceManifest,
            GlobalVersion version)
        {
            serviceManifest.Version = version.Version.ToString();
        }

        public void SetServiceEndpoints(
            ServiceManifest serviceManifest,
            string applicationTypeName,
            string serviceName)
        {
            var endpoints = _packageConfig
                .Endpoints
                .Where(x => x.ApplicationTypeName.Equals(applicationTypeName))
                .Where(x => x.ServiceManifestName.Equals(serviceName))
                .ToList();

            if (!endpoints.Any())
                return;

            if (serviceManifest.Resources == null)
                serviceManifest.Resources = new Resources();
            if (serviceManifest.Resources.Endpoints == null)
                serviceManifest.Resources.Endpoints = new Endpoints();
            if (serviceManifest.Resources.Endpoints.Endpoint == null)
                serviceManifest.Resources.Endpoints.Endpoint = new List<Endpoint>();

            var endpointList = serviceManifest.Resources.Endpoints.Endpoint;

            foreach (var endpointConfig in endpoints)
            {
                var endpoint = new Endpoint
                {
                    Name = endpointConfig.EndpointName
                };

                if (!string.IsNullOrWhiteSpace(endpointConfig.Protocol))
                    endpoint.Protocol = endpointConfig.Protocol;

                if (!string.IsNullOrWhiteSpace(endpointConfig.Type))
                    endpoint.Type = endpointConfig.Type;

                if (endpointConfig.Port != 0)
                    endpoint.Port = endpointConfig.Port.ToString();
            }

            serviceManifest.Resources.Endpoints.Endpoint = endpointList;
        }

        public void SetGeneralInfo(
            ApplicationManifest appManifest,
            IReadOnlyDictionary<string, GlobalVersion> versions,
            string appTypeVersion)
        {
            appManifest.ApplicationTypeVersion = appTypeVersion;

            foreach (var serviceImport in appManifest.ServiceManifestImports)
            {
                var serviceName = serviceImport.ServiceManifestRef.ServiceManifestName;
                if (!versions.ContainsKey(serviceName))
                    continue;

                serviceImport.ServiceManifestRef.ServiceManifestVersion = versions[serviceName].Version.ToString();
            }
        }

        public void CleanAppManifest(ApplicationManifest appManifest)
        {
            appManifest.Parameters = null;
            appManifest.DefaultServices = null;
        }
    }
}