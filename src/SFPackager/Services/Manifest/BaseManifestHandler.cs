using System.Xml;

namespace SFPackager.Services.Manifest
{
    public class BaseManifestHandler
    {
        protected const string NamespaceString = "http://schemas.microsoft.com/2011/01/fabric";

        protected XmlNamespaceManager GetNsManager(XmlDocument document)
        {
            var nsManager = new XmlNamespaceManager(document.NameTable);
            nsManager.AddNamespace("x", NamespaceString);

            return nsManager;
        }
    }
}