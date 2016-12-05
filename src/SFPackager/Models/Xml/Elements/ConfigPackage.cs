using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class ConfigPackage
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Version")]
        public string Version { get; set; }
    }
}