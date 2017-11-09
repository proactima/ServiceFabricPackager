using System;
using System.Xml.Serialization;

namespace SFPackager.Models.Xml
{
    [XmlRoot("Project")]
    public class CoreProjectFile
    {
        [XmlElement("PropertyGroup")]
        public PropertyGroup[] PropertyGroup { get; set; }
    }

    public class PropertyGroup
    {
        [XmlElement("TargetFramework")]
        public string TargetFramework { get; set; }

        [XmlElement("RuntimeIdentifier")]
        public string RuntimeIdentifierRaw { get; set; }

        [XmlIgnore]
        public string[] RuntimeIdentifiers => !string.IsNullOrWhiteSpace(RuntimeIdentifierRaw)
            ? RuntimeIdentifierRaw.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
            : new string[] { };
    }
}
