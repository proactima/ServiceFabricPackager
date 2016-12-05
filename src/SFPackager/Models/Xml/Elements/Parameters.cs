using System.Collections.Generic;
using System.Xml.Serialization;

namespace SFPackager.Models.Xml.Elements
{
    public class Parameters
    {
        [XmlElement("Parameter")]
        public List<Parameter> Parameter { get; set; }
    }
}