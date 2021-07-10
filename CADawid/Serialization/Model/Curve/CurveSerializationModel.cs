using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CADawid.Model;

namespace CADawid.Serialization.Model
{
    public abstract class CurveSerializationModel : SerializationModel
    {
        [XmlArray(ElementName = "Points")]
        [XmlArrayItem(ElementName = "PointRef")]
        public List<PointRef> PointsRef { get; set; }

        public CurveSerializationModel(IGeometryObject model) : base(model) { }

        public CurveSerializationModel() { }
    }
}
