using System.IO;
using System.Xml;
using SFPackager.Helpers;
using SFPackager.Models;

namespace SFPackager.Services
{
    public class ManifestParser
    {
        public ServiceFabricApplicationProject ReadXml(ServiceFabricApplicationProject appProject)
        {
            var document = new XmlDocument();
            var manifestXml = File.ReadAllText(appProject.ApplicationManifestFileFullPath);
            document.LoadXml(manifestXml);
            var nsManager = new XmlNamespaceManager(document.NameTable);
            nsManager.AddNamespace("x", "http://schemas.microsoft.com/2011/01/fabric");

            appProject.ApplicationTypeName = document.GetSingleValue("//x:ApplicationManifest/@ApplicationTypeName", nsManager);
            appProject.ApplicationTypeVersion =
                document.GetSingleValue("//x:ApplicationManifest/@ApplicationTypeVersion", nsManager);

            return appProject;
        }

        public ServiceFabricServiceProject ReadXml(ServiceFabricServiceProject project, string buildOutputPath)
        {
            var document = new XmlDocument();
            var rawXml = File.ReadAllText(project.SourceServiceManifestPath);
            document.LoadXml(rawXml);
            var nsManager = new XmlNamespaceManager(document.NameTable);
            nsManager.AddNamespace("x", "http://schemas.microsoft.com/2011/01/fabric");

            project.ServiceName = document.GetSingleValue("//x:ServiceManifest/@Name", nsManager);
            project.ServiceVersion = document.GetSingleValue("//x:ServiceManifest/@Version", nsManager);

            var codePackage = GetPackageInfo("CodePackage", PackageType.Code, project, document, nsManager);
            codePackage.Path = buildOutputPath;
            project.SubPackages.Add(codePackage);

            var configPackage = GetPackageInfo("ConfigPackage", PackageType.Config, project, document, nsManager);
            project.SubPackages.Add(configPackage);

            var dataPackage = GetPackageInfo("DataPackage", PackageType.Data, project, document, nsManager);
            if (!string.IsNullOrWhiteSpace(dataPackage.Name))
                project.SubPackages.Add(dataPackage);

            return project;
        }

        private static SubPackage GetPackageInfo(
            string packageName,
            PackageType packageType,
            ServiceFabricServiceProject project,
            XmlNode document,
            XmlNamespaceManager nsManager)
        {
            var name = document.GetSingleValue($"//x:{packageName}/@Name", nsManager);
            var configPackage = new SubPackage
            {
                Name = name,
                Version = document.GetSingleValue($"//x:{packageName}/@Version", nsManager),
                PackageType = packageType,
                Path = $"{project.PackageRoot}{name}"
            };

            return configPackage;
        }
    }
}