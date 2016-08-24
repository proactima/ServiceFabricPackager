using System.IO;
using System.Xml;
using SFPackager.Helpers;
using SFPackager.Models;

namespace SFPackager.Services
{
    public class ServiceFabricServiceManifestHandler
    {
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