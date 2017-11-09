using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class Group
    {
        [XmlAttribute("NameRef")]
        public string NameRef { get; set; }
    }
}