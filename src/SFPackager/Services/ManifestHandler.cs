using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SFPackager.Helpers;
using SFPackager.Models;
using SFPackager.Models.Xml;
using SFPackager.Models.Xml.Elements;

namespace SFPackager.Services
{
    public class ManifestHandler
    {
        private readonly ManifestLoader<ApplicationManifest> _appManifestLoader;
        private readonly AppConfig _baseConfig;
        private readonly PackageConfig _packageConfig;
        private readonly ManifestLoader<ServiceManifest> _serviceManifestLoader;

        public ManifestHandler(
            AppConfig baseConfig,
            PackageConfig packageConfig,
            ManifestLoader<ApplicationManifest> appManifestLoader,
            ManifestLoader<ServiceManifest> serviceManifestLoader)
        {
            _baseConfig = baseConfig;
            _packageConfig = packageConfig;
            _appManifestLoader = appManifestLoader;
            _serviceManifestLoader = serviceManifestLoader;
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

                SetGeneralInfo(versions, appManifest, app);
                SetEndpointCerts(appData, appManifest);
                SetEnciphermentCerts(appData, appManifest);

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

                    SetServiceManifestGeneral(serviceManifest, service);
                    SetServiceEndpoints(appData, serviceData, serviceManifest);

                    SetServicePackagesData(versions, service, serviceManifest);

                    _serviceManifestLoader.Save(serviceManifest, packagedServiceManifest);
                }
            }
        }

        private static void SetServicePackagesData(
            Dictionary<string, GlobalVersion> versions,
            KeyValuePair<string, GlobalVersion> service,
            ServiceManifest serviceManifest)
        {
            var subPackages = versions
                .Where(x => x.Value.ParentRef.Equals(service.Key))
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

        private static void SetServiceManifestGeneral(
            ServiceManifest serviceManifest,
            KeyValuePair<string, GlobalVersion> service)
        {
            serviceManifest.Version = service.Value.Version.ToString();
        }

        private void SetServiceEndpoints(
            ServiceFabricApplicationProject appData,
            ServiceFabricServiceProject serviceData,
            ServiceManifest serviceManifest)
        {
            var endpoints = _packageConfig
                .Endpoints
                .Where(x => x.ApplicationTypeName.Equals(appData.ApplicationTypeName))
                .Where(x => x.ServiceManifestName.Equals(serviceData.ServiceName))
                .ToList();

            if (!endpoints.Any())
                return;

            if(serviceManifest.Resources == null)
                serviceManifest.Resources = new Resources();
            if(serviceManifest.Resources.Endpoints == null)
                serviceManifest.Resources.Endpoints = new Endpoints();
            if(serviceManifest.Resources.Endpoints.Endpoint == null)
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

        private void SetEndpointCerts(
            ServiceFabricApplicationProject appData,
            ApplicationManifest appManifest)
        {
            var httpsCerts = _packageConfig
                .Https
                .Where(x => x.ApplicationTypeName.Equals(appData.ApplicationTypeName))
                .OrderBy(x => x.CertThumbprint)
                .ToList();

            if (!httpsCerts.Any())
                return;

            if (appManifest.Certificates == null)
                appManifest.Certificates = new Certificates();
            if (appManifest.Certificates.EndpointCertificates == null)
                appManifest.Certificates.EndpointCertificates = new List<EndpointCertificate>();

            var certList = appManifest.Certificates.EndpointCertificates;

            var distinctThumbprints = httpsCerts
                .GroupBy(x => x.CertThumbprint);

            distinctThumbprints.ForEach((certGroup, i) =>
            {
                var certificate = certGroup.First();
                var certName = $"Certificate{i}";

                certList.Add(new EndpointCertificate
                {
                    Name = certName,
                    X509FindValue = certificate.CertThumbprint
                });

                certGroup.ForEach(cert =>
                {
                    var importNodeList = appManifest
                        .ServiceManifestImports
                        .Where(x => x.ServiceManifestRef.ServiceManifestName.Equals(cert.ServiceManifestName))
                        .ToList();

                    if (!importNodeList.Any())
                        return;

                    var importNode = importNodeList.First();

                    if (importNode.Policies == null)
                        importNode.Policies = new Policies();
                    if (importNode.Policies.EndpointBindingPolicy == null)
                        importNode.Policies.EndpointBindingPolicy = new List<EndpointBindingPolicy>();

                    var binding = new EndpointBindingPolicy
                    {
                        CertificateRef = certName,
                        EndpointRef = cert.EndpointName
                    };

                    importNode.Policies.EndpointBindingPolicy.Add(binding);
                });
            });

            appManifest.Certificates.EndpointCertificates = certList;
        }

        private static void SetGeneralInfo(
            IReadOnlyDictionary<string, GlobalVersion> versions,
            ApplicationManifest appManifest,
            KeyValuePair<string, GlobalVersion> app)
        {
            appManifest.ApplicationTypeVersion = app.Value.ToString();
            appManifest.Parameters = null;
            appManifest.DefaultServices = null;

            foreach (var serviceImport in appManifest.ServiceManifestImports)
            {
                var serviceName = serviceImport.ServiceManifestRef.ServiceManifestName;
                if (!versions.ContainsKey(serviceName))
                    continue;

                serviceImport.ServiceManifestRef.ServiceManifestVersion = versions[serviceName].Version.ToString();
            }
        }

        private void SetEnciphermentCerts(
            ServiceFabricApplicationProject appData,
            ApplicationManifest appManifest)
        {
            var encipherments = _packageConfig
                .Encipherment
                .Where(x => EncipherNameEqualsAppName(x, appData))
                .ToList();

            if (!encipherments.Any())
                return;

            if (appManifest.Principals == null)
                appManifest.Principals = new Principals();
            if (appManifest.Principals.Users == null)
                appManifest.Principals.Users = new Users();
            if (appManifest.Principals.Users.User == null)
                appManifest.Principals.Users.User = new List<User>();
            if (appManifest.Policies == null)
                appManifest.Policies = new Policies();
            if (appManifest.Policies.SecurityAccessPolicies == null)
                appManifest.Policies.SecurityAccessPolicies = new SecurityAccessPolicies();
            if (appManifest.Policies.SecurityAccessPolicies.SecurityAccessPolicy == null)
                appManifest.Policies.SecurityAccessPolicies.SecurityAccessPolicy = new List<SecurityAccessPolicy>();
            if (appManifest.Certificates == null)
                appManifest.Certificates = new Certificates();
            if (appManifest.Certificates.SecretsCertificate == null)
                appManifest.Certificates.SecretsCertificate = new List<SecretsCertificate>();

            foreach (var encipherment in encipherments)
            {
                var user = new User
                {
                    Name = encipherment.Name,
                    AccountType = "NetworkService"
                };

                var policy = new SecurityAccessPolicy
                {
                    GrantRights = "Read",
                    PrincipalRef = encipherment.Name,
                    ResourceRef = encipherment.CertName,
                    ResourceType = "Certificate"
                };

                var secretCert = new SecretsCertificate
                {
                    Name = encipherment.CertName,
                    X509FindValue = encipherment.CertThumbprint,
                    X509FindType = "FindByThumbprint"
                };

                appManifest.Principals.Users.User.Add(user);
                appManifest.Policies.SecurityAccessPolicies.SecurityAccessPolicy.Add(policy);
                appManifest.Certificates.SecretsCertificate.Add(secretCert);
            }
        }

        private static bool EncipherNameEqualsAppName(
            Encipherment enchipherment,
            ServiceFabricApplicationProject appData)
        {
            return enchipherment.ApplicationTypeName.Equals(appData.ApplicationTypeName,
                StringComparison.CurrentCultureIgnoreCase);
        }
    }
}