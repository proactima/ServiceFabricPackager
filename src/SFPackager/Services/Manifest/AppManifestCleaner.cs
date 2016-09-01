using System.Xml;
using SFPackager.Helpers;

namespace SFPackager.Services.Manifest
{
    public class AppManifestCleaner : BaseManifestHandler
    {
        public void Execute(XmlDocument document)
        {
            var nsManager = GetNsManager(document);

            document.RemoveNodes("//x:ApplicationManifest/x:Parameters", "/x:Parameter", nsManager);
            document.RemoveNodes("//x:ApplicationManifest/x:DefaultServices", "/x:Service", nsManager);
        }
    }
}