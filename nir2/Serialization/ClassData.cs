using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace nir2.Serialization
{
    [Serializable]
    public class ClassData
    {
        public string Name { get; set; }
        public string Constraint { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        [XmlArray("Attributes")]
        [XmlArrayItem("Attribute")]
        public List<AttributeData> Attributes { get; set; } = new List<AttributeData>();

        [XmlArray("Methods")]
        [XmlArrayItem("Method")]
        public List<MethodData> Methods { get; set; } = new List<MethodData>();
    }
}
