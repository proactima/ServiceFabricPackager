using System.Collections.Generic;
using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class MemberOf
    {
        [XmlElement("Group")]
        public List<Group> Groups { get; set; }

        [XmlElement("SystemGroup")]
        public List<SystemGroup> SystemGroups { get; set; }
    }
}