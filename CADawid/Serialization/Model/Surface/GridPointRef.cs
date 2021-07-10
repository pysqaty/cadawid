using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CADawid.Serialization.Model
{
    public struct GridPointRef
    {
        [XmlAttribute(AttributeName = "Name")]
        public string PointRef { get; set; }
        [XmlAttribute]
        public int Row { get; set; }
        [XmlAttribute]
        public int Column { get; set; }
    }
}
