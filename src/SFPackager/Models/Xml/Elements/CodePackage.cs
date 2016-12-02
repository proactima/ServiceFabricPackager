using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class CodePackage
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Version")]
        public string Version { get; set; }

        [XmlElement("EntryPoint")]
        public EntryPoint EntryPoint { get; set; }
    }
}