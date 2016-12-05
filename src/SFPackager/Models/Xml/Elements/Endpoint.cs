using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class Endpoint
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Port")]
        public string Port { get; set; }

        [XmlAttribute("Protocol")]
        public string Protocol { get; set; }

        [XmlAttribute("Type")]
        public string Type { get; set; }
    }
}