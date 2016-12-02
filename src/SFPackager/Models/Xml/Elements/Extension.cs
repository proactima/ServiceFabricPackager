using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class Extension
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("GeneratedId")]
        public string GeneratedId { get; set; }

        [XmlElement("GeneratedNames", Namespace = "http://schemas.microsoft.com/2015/03/fabact-no-schema")]
        public GeneratedNames GeneratedNames { get; set; }
    }
}