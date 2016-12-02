using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using SFPackager.Helpers;
using SFPackager.Models;
using SFPackager.Models.Xml;
using SFPackager.Models.Xml.Elements;

namespace SFPackager.Services
{
    public class ManifestReader
    {
        private readonly AppConfig _baseConfig;
        private readonly PackageConfig _packageConfig;

        public ManifestReader(AppConfig baseConfig, PackageConfig packageConfig)
        {
            _baseConfig = baseConfig;
            _packageConfig = packageConfig;
        }

        public void Handle(
            Dictionary<string, GlobalVersion> versions,
            Dictionary<string, ServiceFabricApplicationProject> applications)
        {
            var apps = versions
                .Where(x => x.Value.VersionType == VersionType.Application)
                .Where(x => x.Value.IncludeInPackage)
                .ToList();

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("x", "http://schemas.microsoft.com/2011/01/fabric");
            var applicationSerializer = new XmlSerializer(typeof(ApplicationManifest));
            var serviceSerializer = new XmlSerializer(typeof(ServiceManifest));

            foreach (var app in apps)
            {
                var appData = applications[app.Key];
                var appPackagePath = appData.GetPackagePath(_baseConfig.PackageOutputPath);
                var packagedAppManifest = appData.GetAppManifestTargetFile(appPackagePath);

                ApplicationManifest appManifest;
                using (var reader = new FileStream(packagedAppManifest.FullName, FileMode.Open))
                {
                    appManifest = (ApplicationManifest)applicationSerializer.Deserialize(reader);
                }

                appManifest.ApplicationTypeVersion = app.Value.ToString();
                appManifest.Parameters = null;
                appManifest.DefaultServices = null;
                var serviceNames = appData.Services.Select(x => x.Key).ToList();

                var encipherments = _packageConfig
                    .Encipherment
                    .Where(x => EncipherNameEqualsAppName(x, appData))
                    .ToList();

                if (encipherments.Any())
                {
                    appManifest.Principals.Users.User = encipherments
                        .Select(principal => new User
                        {
                            Name = principal.Name,
                            AccountType = "NetworkService"
                        })
                        .ToList();

                    appManifest.Policies.SecurityAccessPolicies.SecurityAccessPolicy = encipherments
                        .Select(x => new SecurityAccessPolicy
                        {
                            GrantRights = "Read",
                            PrincipalRef = x.Name,
                            ResourceRef = x.CertName,
                            ResourceType = "Certificate"
                        })
                        .ToList();
                }

                foreach (var serviceImport in appManifest.ServiceManifestImports)
                {
                    var serviceName = serviceImport.ServiceManifestRef.ServiceManifestName;
                    if (!versions.ContainsKey(serviceName))
                        continue;

                    serviceImport.ServiceManifestRef.ServiceManifestVersion = versions[serviceName].Version.ToString();
                }

                var httpsCerts = _packageConfig
                    .Https
                    .Where(x => x.ApplicationTypeName.Equals(appData.ApplicationTypeName))
                    .OrderBy(x => x.CertThumbprint)
                    .ToList();

                if (httpsCerts.Any())
                {
                    var certList = new List<EndpointCertificate>();
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

                var secretCerts = _packageConfig
                    .Encipherment
                    .Where(x => x.ApplicationTypeName.Equals(appData.ApplicationTypeName))
                    .ToList();

                if (secretCerts.Any())
                {
                    foreach (var encipherment in secretCerts)
                    {
                        if (appManifest.Certificates == null)
                            appManifest.Certificates = new Certificates();

                        if (appManifest.Certificates.SecretsCertificate == null)
                            appManifest.Certificates.SecretsCertificate = new List<SecretsCertificate>();

                        var secretCert = new SecretsCertificate
                        {
                            Name = encipherment.CertName,
                            X509FindValue = encipherment.CertThumbprint,
                            X509FindType = "FindByThumbprint"
                        };
                        appManifest.Certificates.SecretsCertificate.Add(secretCert);
                    }
                }

                using (var writer = new FileStream(appPackagePath.FullName, FileMode.Create, FileAccess.Write))
                {
                    applicationSerializer.Serialize(writer, appManifest);
                }

                var includedServices = versions
                    .Where(x => x.Value.ParentRef.Equals(app.Key))
                    .Where(x => x.Value.IncludeInPackage)
                    .ToList();

                foreach (var service in includedServices)
                {
                    var serviceData = appData.Services[service.Key];

                    var packagedServiceManifest = Path.Combine(appPackagePath.FullName, service.Key,
                        serviceData.ServiceManifestFile);
                    ServiceManifest serviceManifest;
                    using (var reader = new FileStream(packagedServiceManifest, FileMode.Open))
                    {
                        serviceManifest = (ServiceManifest)serviceSerializer.Deserialize(reader);
                    }

                    serviceManifest.Version = service.Value.Version.ToString();

                    var endpoints = _packageConfig
                        .Endpoints
                        .Where(x => x.ApplicationTypeName.Equals(appData.ApplicationTypeName))
                        .Where(x => x.ServiceManifestName.Equals(serviceData.ServiceName))
                        .ToList();

                    if (endpoints.Any())
                    {
                        var endpointList = new List<Endpoint>();

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
            }
        }

        private static bool EncipherNameEqualsAppName(Encipherment enchipherment,
            ServiceFabricApplicationProject appData)
        {
            return enchipherment.ApplicationTypeName.Equals(appData.ApplicationTypeName,
                StringComparison.CurrentCultureIgnoreCase);
        }
    }
}