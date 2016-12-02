using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class DataPackage
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Version")]
        public string Version { get; set; }
    }
}