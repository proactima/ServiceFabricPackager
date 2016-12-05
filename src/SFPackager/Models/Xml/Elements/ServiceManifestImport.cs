using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class ServiceManifestImport
    {
        [XmlElement("ServiceManifestRef")]
        public ServiceManifestRef ServiceManifestRef { get; set; }

        [XmlElement("ConfigOverrides")]
        public ConfigOverrides ConfigOverrides { get; set; }

        [XmlElement("Policies")]
        public Policies Policies { get; set; }
    }
}