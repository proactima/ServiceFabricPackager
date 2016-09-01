using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using SFPackager.Helpers;
using SFPackager.Models;
using SFPackager.Services.Manifest;

namespace SFPackager.Services
{
    public class ManifestWriter
    {
        private const string NamespaceString = "http://schemas.microsoft.com/2011/01/fabric";
        private readonly PackageConfig _packageConfig;
        private readonly CertificateAppender _certAppender;

        public ManifestWriter(PackageConfig packageConfig, CertificateAppender certAppender)
        {
            _packageConfig = packageConfig;
            _certAppender = certAppender;
        }

        public void UpdateManifests(
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
                var packagedAppManifest = Path.Combine(appData.PackagePath, appData.ApplicationManifestFile);

                var appDocument = new XmlDocument();
                var rawAppXml = File.ReadAllText(packagedAppManifest);
                appDocument.LoadXml(rawAppXml);
                var appNsManager = new XmlNamespaceManager(appDocument.NameTable);
                appNsManager.AddNamespace("x", NamespaceString);

                appDocument.SetSingleValue("//x:ApplicationManifest/@ApplicationTypeVersion", app.Value.Version.ToString(), appNsManager);
                //appDocument.RemoveNodes("//x:ApplicationManifest/x:Parameters", "/x:Parameter", appNsManager);
                //appDocument.RemoveNodes("//x:ApplicationManifest/x:DefaultServices", "/x:Service", appNsManager);

                var serviceNodes = appDocument.GetNodes("//x:ApplicationManifest/x:ServiceManifestImport/x:ServiceManifestRef", appNsManager);

                foreach (var service in serviceNodes)
                {
                    var serviceElement = service as XmlElement;
                    if (serviceElement == null)
                        continue;

                    var serviceName = serviceElement.GetAttribute("ServiceManifestName");
                    if (!versions.ContainsKey(serviceName))
                        continue;

                    var serviceVersion = versions[serviceName].Version;
                    serviceElement.SetAttribute("ServiceManifestVersion", serviceVersion.ToString());
                }

                _certAppender.SetCertificates(appDocument, appData.ApplicationTypeName, new List<string>());

                using (var outStream = new FileStream(packagedAppManifest, FileMode.Truncate))
                {
                    appDocument.Save(outStream);
                }

                var includedServices = versions
                    .Where(x => x.Value.ParentRef.Equals(app.Key))
                    .Where(x => x.Value.IncludeInPackage)
                    .ToList();

                foreach (var service in includedServices)
                {
                    var serviceData = appData.Services[service.Key];

                    var packagedServiceManifest = Path.Combine(appData.PackagePath, service.Key,
                        serviceData.ServiceManifestFile);
                    var serviceDocument = new XmlDocument();
                    var rawServiceXml = File.ReadAllText(packagedServiceManifest);
                    serviceDocument.LoadXml(rawServiceXml);
                    var serviceNsManager = new XmlNamespaceManager(serviceDocument.NameTable);
                    serviceNsManager.AddNamespace("x", NamespaceString);

                    serviceDocument.SetSingleValue("//x:ServiceManifest/@Version", service.Value.Version.ToString(), serviceNsManager);

                    var subPackages = versions
                        .Where(x => x.Value.ParentRef.Equals(service.Key))
                        .ToList();

                    foreach (var package in subPackages)
                    {
                        var packageName = package.Key.Split('-')[1];
                        string nodeName;

                        switch (package.Value.PackageType)
                        {
                            case PackageType.Code:
                                nodeName = "CodePackage";
                                break;
                            case PackageType.Config:
                                nodeName = "ConfigPackage";
                                break;
                            case PackageType.Data:
                                nodeName = "DataPackage";
                                break;
                            case PackageType.None:
                                continue;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        var node = serviceDocument.GetNode($"//x:ServiceManifest/x:{nodeName}[@Name='{packageName}']", serviceNsManager);
                        node.Attributes["Version"].Value = package.Value.Version.ToString();
                    }

                    using (var outStream = new FileStream(packagedServiceManifest, FileMode.Truncate))
                    {
                        serviceDocument.Save(outStream);
                    }
                }
            }
        }
    }
}