using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class ServiceManifestRef
    {
        [XmlAttribute]
        public string ServiceManifestName { get; set; }

        [XmlAttribute]
        public string ServiceManifestVersion { get; set; }
    }
}