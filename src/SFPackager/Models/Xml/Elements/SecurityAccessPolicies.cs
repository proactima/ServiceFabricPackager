using System.Collections.Generic;
using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class SecurityAccessPolicies
    {
        [XmlElement("SecurityAccessPolicy")]
        public List<SecurityAccessPolicy> SecurityAccessPolicy { get; set; }
    }
}