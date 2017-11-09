using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class SystemGroup
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }
    }
}