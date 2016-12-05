using System.Collections.Generic;
using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class Policies
    {
        [XmlElement("EndpointBindingPolicy")]
        public List<EndpointBindingPolicy> EndpointBindingPolicy { get; set; }

        [XmlElement("SecurityAccessPolicies")]
        public SecurityAccessPolicies SecurityAccessPolicies { get; set; }
    }
}