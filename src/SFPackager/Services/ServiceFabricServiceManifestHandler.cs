using System.IO;
using System.Xml;
using SFPackager.Helpers;
using SFPackager.Models;

namespace SFPackager.Services
{
    public class ServiceFabricServiceManifestHandler
    {
        public ServiceFabricServiceProject ReadXml(ServiceFabricServiceProject project, string buildOutputPath)
        {
            var document = new XmlDocument();
            var rawXml = File.ReadAllText(project.ServiceManifestFileFullPath);
            document.LoadXml(rawXml);
            var nsManager = new XmlNamespaceManager(document.NameTable);
            nsManager.AddNamespace("x", "http://schemas.microsoft.com/2011/01/fabric");

            project.ServiceName = XmlHelper.GetSingleValue("//x:ServiceManifest/@Name", document, nsManager);
            project.ServiceVersion = XmlHelper.GetSingleValue("//x:ServiceManifest/@Version", document, nsManager);

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

        private static SubPackage GetPackageInfo(string packageName, PackageType packageType,
            ServiceFabricServiceProject project, XmlDocument document,
            XmlNamespaceManager nsManager)
        {
            var name = XmlHelper.GetSingleValue($"//x:{packageName}/@Name", document, nsManager);
            var configPackage = new SubPackage
            {
                Name = name,
                Version = XmlHelper.GetSingleValue($"//x:{packageName}/@Version", document, nsManager),
                PackageType = packageType,
                Path = $"{project.PackageRoot}{name}"
            };

            return configPackage;
        }

        public void WriteXml(string inputManifest, string outputPath)
        {
            using (var outStream = new FileStream(outputPath, FileMode.Truncate))
            {
                var document = new XmlDocument();
                document.LoadXml(inputManifest);
                var nsManager = new XmlNamespaceManager(document.NameTable);
                nsManager.AddNamespace("x", "http://schemas.microsoft.com/2011/01/fabric");

                XmlHelper.SetSingleValue("//x:ServiceManifest/@Version", "ballestein", document, nsManager);

                document.Save(outStream);
            }
        }
    }
}