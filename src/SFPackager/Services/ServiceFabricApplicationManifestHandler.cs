using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using SFPackager.Helpers;
using SFPackager.Models;

namespace SFPackager.Services
{
    public class ServiceFabricApplicationManifestHandler
    {
        public ServiceFabricApplicationProject ReadXml(ServiceFabricApplicationProject appProject)
        {
            var document = new XmlDocument();
            var manifestXml = File.ReadAllText(appProject.ApplicationManifestFileFullPath);
            document.LoadXml(manifestXml);
            var nsManager = new XmlNamespaceManager(document.NameTable);
            nsManager.AddNamespace("x", "http://schemas.microsoft.com/2011/01/fabric");

            appProject.ApplicationTypeName = XmlHelper.GetSingleValue("//x:ApplicationManifest/@ApplicationTypeName", document,
                nsManager);
            appProject.ApplicationTypeVersion = XmlHelper.GetSingleValue("//x:ApplicationManifest/@ApplicationTypeVersion",
                document, nsManager);

            var serviceImports = XmlHelper.GetNodes("//x:ServiceManifestImport", document, nsManager);
            foreach (var element in serviceImports.OfType<XmlElement>())
            {
                //manifest.Services.Add(new ServiceFabricServiceManifestImport
                //{
                //    ServiceManifestName = element["ServiceManifestRef"].Attributes["ServiceManifestName"].Value,
                //    ServiceManifestVersion = element["ServiceManifestRef"].Attributes["ServiceManifestVersion"].Value
                //});
            }
            
            return appProject;
        }
    }
}