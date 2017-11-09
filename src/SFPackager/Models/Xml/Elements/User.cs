using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class User
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("AccountType")]
        public string AccountType { get; set; }

        [XmlElement("MemberOf")]
        public MemberOf MemberOf { get; set; }
    }
}