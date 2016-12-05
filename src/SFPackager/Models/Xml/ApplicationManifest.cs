using System.Collections.Generic;
using System.Xml.Serialization;
using SFPackager.Models.Xml.Elements;

namespace SFPackager.Models.Xml
{
    [XmlRoot("ApplicationManifest", Namespace = "http://schemas.microsoft.com/2011/01/fabric")]
    public class ApplicationManifest
    {
        [XmlAttribute]
        public string ApplicationTypeName { get; set; }

        [XmlAttribute]
        public string ApplicationTypeVersion { get; set; }

        [XmlElement("Parameters")]
        public Parameters Parameters { get; set; }

        [XmlElement("ServiceManifestImport")]
        public List<ServiceManifestImport> ServiceManifestImports { get; set; }

        [XmlElement("DefaultServices")]
        public DefaultServices DefaultServices { get; set; }

        [XmlElement("Principals")]
        public Principals Principals { get; set; }

        [XmlElement("Policies")]
        public Policies Policies { get; set; }

        [XmlElement("Certificates")]
        public Certificates Certificates { get; set; }
    }
}