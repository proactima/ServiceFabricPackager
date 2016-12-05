using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class GeneratedNames
    {
        [XmlElement("DefaultService")]
        public NameElement DefaultService { get; set; }

        [XmlElement("ServiceEndpoint")]
        public NameElement ServiceEndpoint { get; set; }

        [XmlElement("ReplicatorEndpoint")]
        public NameElement ReplicatorEndpoint { get; set; }

        [XmlElement("ReplicatorConfigSection")]
        public NameElement ReplicatorConfigSection { get; set; }

        [XmlElement("ReplicatorSecurityConfigSection")]
        public NameElement ReplicatorSecurityConfigSection { get; set; }

        [XmlElement("StoreConfigSection")]
        public NameElement StoreConfigSection { get; set; }
    }
}