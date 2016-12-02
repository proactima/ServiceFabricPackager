using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class NameElement
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }
    }
}