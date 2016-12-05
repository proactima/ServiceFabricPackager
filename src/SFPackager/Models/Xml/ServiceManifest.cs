using System.Collections.Generic;
using System.Xml.Serialization;
using SFPackager.Models.Xml.Elements;

namespace SFPackager.Models.Xml
{
    [XmlRoot("ServiceManifest", Namespace = "http://schemas.microsoft.com/2011/01/fabric")]
    public class ServiceManifest
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Version")]
        public string Version { get; set; }

        [XmlElement("ServiceTypes")]
        public ServiceTypes ServiceTypes { get; set; }

        [XmlElement("CodePackage")]
        public CodePackage CodePackage { get; set; }

        [XmlElement("ConfigPackage")]
        public ConfigPackage ConfigPackage { get; set; }

        [XmlElement("DataPackage")]
        public List<DataPackage> DataPackages { get; set; }

        [XmlElement("Resources")]
        public Resources Resources { get; set; }
    }
}