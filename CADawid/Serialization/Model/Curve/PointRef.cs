using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CADawid.Serialization.Model
{
    public struct PointRef
    {
        [XmlAttribute]
        public string Name { get; set; }
    }
}
