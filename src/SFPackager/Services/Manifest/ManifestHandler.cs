using System.Collections.Generic;
using System.IO;
using System.Linq;
using SFPackager.Models;
using SFPackager.Models.Xml;
using SFPackager.Models.Xml.Elements;

namespace SFPackager.Services.Manifest
{
    public class ManifestHandler
    {
        private readonly ApplicationManifestHandler _appManifestHandler;
        private readonly ManifestLoader<ApplicationManifest> _appManifestLoader;
        private readonly AppConfig _baseConfig;
        private readonly HandleEnciphermentCert _handleEnciphermentCert;
        private readonly HandleEndpointCert _handleEndpointCert;
        private readonly PackageConfig _packageConfig;
        private readonly ServiceManifestHandler _serviceManifestHandler;
        private readonly ManifestLoader<ServiceManifest> _serviceManifestLoader;

        public ManifestHandler(
            AppConfig baseConfig,
            PackageConfig packageConfig,
            ManifestLoader<ApplicationManifest> appManifestLoader,
            ManifestLoader<ServiceManifest> serviceManifestLoader,
            HandleEnciphermentCert handleEnciphermentCert,
            HandleEndpointCert handleEndpointCert,
            ApplicationManifestHandler appManifestHandler,
            ServiceManifestHandler serviceManifestHandler)
        {
            _baseConfig = baseConfig;
            _packageConfig = packageConfig;
            _appManifestLoader = appManifestLoader;
            _serviceManifestLoader = serviceManifestLoader;
            _handleEnciphermentCert = handleEnciphermentCert;
            _handleEndpointCert = handleEndpointCert;
            _appManifestHandler = appManifestHandler;
            _serviceManifestHandler = serviceManifestHandler;
        }

        public void Handle(
            VersionMap versions,
            Dictionary<string, ServiceFabricApplicationProject> applications)
        {
            var apps = versions
                .PackageVersions
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
                _appManifestHandler.SetGeneralInfo(appManifest, versions.PackageVersions, app.Value);
                _handleEndpointCert.SetEndpointCerts(_packageConfig, appManifest, appData.ApplicationTypeName);
                _handleEnciphermentCert.SetEnciphermentCerts(_packageConfig, appManifest, appData.ApplicationTypeName);

                _appManifestLoader.Save(appManifest, packagedAppManifest.FullName);

                var includedServices = versions
                    .PackageVersions
                    .Where(x => x.Value.ParentRef.Equals(app.Key))
                    .Where(x => x.Value.IncludeInPackage)
                    .ToList();

                foreach (var service in includedServices)
                {
                    var serviceKey = service.Key.Split('-').Last();
                    var serviceData = appData.Services[serviceKey];

                    var packagedServiceManifest = Path.Combine(appPackagePath.FullName, serviceKey,
                        serviceData.ServiceManifestFile);
                    var serviceManifest = _serviceManifestLoader.Load(packagedServiceManifest);

                    _serviceManifestHandler.SetServiceManifestGeneral(serviceManifest, service.Value);
                    SetServiceEndpoints(serviceManifest, appData.ApplicationTypeName, serviceData.ServiceName);

                    _serviceManifestHandler.SetServicePackagesData(serviceManifest, versions.PackageVersions, service.Key);

                    _serviceManifestLoader.Save(serviceManifest, packagedServiceManifest);
                }
            }
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

                endpointList
                    .Where(x => x.Name.Equals(endpointConfig.EndpointName))
                    .ToList()
                    .ForEach(x => endpointList.Remove(x));

                endpointList.Add(endpoint);
            }

            serviceManifest.Resources.Endpoints.Endpoint = endpointList;
        }

        public void CleanAppManifest(ApplicationManifest appManifest)
        {
            appManifest.Parameters = null;
            appManifest.DefaultServices = null;
        }
    }
}