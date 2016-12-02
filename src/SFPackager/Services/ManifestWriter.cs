﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using SFPackager.Helpers;
using SFPackager.Models;
using SFPackager.Services.Manifest;

namespace SFPackager.Services
{
    public class ManifestWriter
    {
        private const string NamespaceString = "http://schemas.microsoft.com/2011/01/fabric";
        private readonly CertificateAppender _certAppender;
        private readonly ServiceImportMutator _serviceImport;
        private readonly EndpointAppender _endpointAppender;
        private readonly AppManifestCleaner _appManifestCleaner;
        private readonly AppConfig _baseConfig;
        private readonly PolicyAppender _policyAppender;
        private readonly PrincipalAppender _principalAppender;

        public ManifestWriter(
            CertificateAppender certAppender,
            ServiceImportMutator serviceImport,
            EndpointAppender endpointAppender,
            AppManifestCleaner appManifestCleaner,
            AppConfig baseConfig,
            PolicyAppender policyAppender,
            PrincipalAppender principalAppender)
        {
            _certAppender = certAppender;
            _serviceImport = serviceImport;
            _endpointAppender = endpointAppender;
            _appManifestCleaner = appManifestCleaner;
            _baseConfig = baseConfig;
            _policyAppender = policyAppender;
            _principalAppender = principalAppender;
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
                var appPackagePath = appData.GetPackagePath(_baseConfig.PackageOutputPath);
                var packagedAppManifest = appData.GetAppManifestTargetFile(appPackagePath);

                var appDocument = new XmlDocument();
                var rawAppXml = File.ReadAllText(packagedAppManifest.FullName);
                appDocument.LoadXml(rawAppXml);
                var appNsManager = new XmlNamespaceManager(appDocument.NameTable);
                appNsManager.AddNamespace("x", NamespaceString);
                
                appDocument.SetSingleValue("//x:ApplicationManifest/@ApplicationTypeVersion",
                    app.Value.Version.ToString(), appNsManager);
                var serviceNames = appData.Services.Select(x => x.Key).ToList();

                _principalAppender.SetPrincipals(appDocument, appData.ApplicationTypeName);
                _policyAppender.SetPolicies(appDocument, appData.ApplicationTypeName);

                _serviceImport.Execute(appDocument, versions);
                _certAppender.SetCertificates(appDocument, appData.ApplicationTypeName, serviceNames);
                _appManifestCleaner.Execute(appDocument);

                using (var outStream = new FileStream(packagedAppManifest.FullName, FileMode.Truncate))
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

                    var packagedServiceManifest = Path.Combine(appPackagePath.FullName, service.Key,
                        serviceData.ServiceManifestFile);
                    var serviceDocument = new XmlDocument();
                    var rawServiceXml = File.ReadAllText(packagedServiceManifest);
                    serviceDocument.LoadXml(rawServiceXml);
                    var serviceNsManager = new XmlNamespaceManager(serviceDocument.NameTable);
                    serviceNsManager.AddNamespace("x", NamespaceString);

                    serviceDocument.SetSingleValue("//x:ServiceManifest/@Version", service.Value.Version.ToString(),
                        serviceNsManager);

                    _endpointAppender.SetEndpoints(serviceDocument, appData.ApplicationTypeName, serviceData.ServiceName);

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

                        var node = serviceDocument.GetNode($"//x:ServiceManifest/x:{nodeName}[@Name='{packageName}']",
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
    }
}