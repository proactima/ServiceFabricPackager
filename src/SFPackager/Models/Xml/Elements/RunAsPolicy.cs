using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class RunAsPolicy
    {
        [XmlAttribute("CodePackageRef")]
        public string CodePackageRef { get; set; }

        [XmlAttribute("UserRef")]
        public string UserRef { get; set; }

        [XmlAttribute("EntryPointType")]
        public string EntryPointType { get; set; }
    }
}