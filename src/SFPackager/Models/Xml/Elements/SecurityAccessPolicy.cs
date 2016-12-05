using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class SecurityAccessPolicy
    {
        [XmlAttribute("GrantRights")]
        public string GrantRights { get; set; }
        [XmlAttribute("PrincipalRef")]
        public string PrincipalRef { get; set; }
        [XmlAttribute("ResourceRef")]
        public string ResourceRef { get; set; }
        [XmlAttribute("ResourceType")]
        public string ResourceType { get; set; }
    }
}