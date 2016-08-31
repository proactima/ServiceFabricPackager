using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml;
using SFPackager.Helpers;
using SFPackager.Models;

namespace SFPackager.Services
{
    public class ManifestWriter
    {
        private readonly PackageConfig _packageConfig;

        public ManifestWriter(PackageConfig packageConfig)
        {
            _packageConfig = packageConfig;
        }

        public void UpdateManifests(
            Dictionary<string, GlobalVersion> versions,
            Dictionary<string, ServiceFabricApplicationProject> applications)
        {
            // For each application that has changed
            // Set main application version
            // Add certificates
            // Remove Parameters
            // Remove Default Services
            // For each service
            // Set version on each version in app manifest
            // Add certificate policies to services
            // Set service version in servicemanifest
            // For each package in service
            // Set package version

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
                var namespaceString = "http://schemas.microsoft.com/2011/01/fabric";
                appNsManager.AddNamespace("x", namespaceString);

                XmlHelper.SetSingleValue("//x:ApplicationManifest/@ApplicationTypeVersion", app.Value.Version.ToString(), appDocument, appNsManager);
                XmlHelper.RemoveNodes("//x:ApplicationManifest/x:Parameters", "/x:Parameter", appDocument, appNsManager);
                XmlHelper.RemoveNodes("//x:ApplicationManifest/x:DefaultServices", "/x:Service", appDocument, appNsManager);
                // Add certificates stuff here

                var httpsCerts = _packageConfig.Https
                    .Where(x => x.ApplicationTypeName.Equals(appData.ApplicationTypeName))
                    .ToList();

                if (httpsCerts.Any())
                    AddCertificatesToAppManifest(httpsCerts, appDocument, namespaceString, appNsManager);

                var serviceNodes = XmlHelper.GetNodes(
                    "//x:ApplicationManifest/x:ServiceManifestImport/x:ServiceManifestRef", appDocument, appNsManager);

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

                    var packagedServiceManifest = Path.Combine(appData.PackagePath, service.Key, serviceData.ServiceManifestFile);
                    var serviceDocument = new XmlDocument();
                    var rawServiceXml = File.ReadAllText(packagedServiceManifest);
                    serviceDocument.LoadXml(rawServiceXml);
                    var serviceNsManager = new XmlNamespaceManager(serviceDocument.NameTable);
                    serviceNsManager.AddNamespace("x", namespaceString);

                    XmlHelper.SetSingleValue("//x:ServiceManifest/@Version", service.Value.Version.ToString(), serviceDocument, serviceNsManager);

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

                        var node = XmlHelper.GetNode($"//x:ServiceManifest/x:{nodeName}[@Name='{packageName}']", serviceDocument,
                                    serviceNsManager);
                        node.Attributes["Version"].Value = package.Value.Version.ToString();
                    }

                    using (var outStream = new FileStream(packagedServiceManifest, FileMode.Truncate))
                    {
                        serviceDocument.Save(outStream);
                    }
                }
            }
        }

        private static void AddCertificatesToAppManifest(
            IEnumerable<HttpsConfig> httpsCerts,
            XmlDocument appDocument,
            string namespaceString,
            XmlNamespaceManager appNsManager)
        {
            var i = 0;
            var certElement = appDocument.CreateElement("Certificates", namespaceString);
            var distinctThumbprints = httpsCerts.GroupBy(x => x.CertThumbprint);

            foreach (var certGroup in distinctThumbprints)
            {
                if(!certGroup.Any())
                    continue;

                var certificate = certGroup.First();
                var certName = $"Certificate{i}";

                var endpointElement = appDocument.CreateElement("EndpointCertificate", namespaceString);
                endpointElement.SetAttribute("X509FindValue", certificate.CertThumbprint);
                endpointElement.SetAttribute("Name", certName);
                certElement.AppendChild(endpointElement);

                foreach (var cert in certGroup)
                {
                    var serviceManifestNode = XmlHelper.GetNode($"//x:ApplicationManifest/x:ServiceManifestImport/x:ServiceManifestRef[@ServiceManifestName='{cert.ServiceManifestName}']", appDocument, appNsManager);
                    if(serviceManifestNode == null)
                        continue;

                    var importNode = serviceManifestNode.ParentNode;
                    var policyNode = appDocument.CreateElement("Policies", namespaceString);
                    var bindingNode = appDocument.CreateElement("EndpointBindingPolicy", namespaceString);
                    bindingNode.SetAttribute("EndpointRef", cert.EndpointName);
                    bindingNode.SetAttribute("CertificateRef", certName);
                    policyNode.AppendChild(bindingNode);
                    importNode.AppendChild(policyNode);
                }

                i++;
            }

            var root = XmlHelper.GetNode("//x:ApplicationManifest", appDocument, appNsManager);
            root.AppendChild(certElement);
        }
    }
}
