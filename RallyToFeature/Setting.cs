using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace RallyToFeature
{

    [Serializable]
    [XmlType(AnonymousType = true)]
    [DebuggerDisplay("{Value}", Name = "{Name}")]
    public class Setting
    {

        public Setting() { }
        public Setting(string name, string value, string description = null)
        {
            Name = name;
            Value = value;
            Description = description;
        }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Value { get; set; }

        [XmlAttribute]
        public string Description { get; set; }

        [XmlIgnore]
        public string CheckedValue
        {
            get { return Value; }
        }

    }

}