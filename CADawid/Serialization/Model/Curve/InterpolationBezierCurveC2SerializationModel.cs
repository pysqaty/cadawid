using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using CADawid.Model;

namespace CADawid.Serialization.Model
{
    public class InterpolationBezierCurveC2SerializationModel : CurveSerializationModel
    {
        [XmlIgnore]
        public new SerializationVector3 Position { get; set; }
        [XmlIgnore]
        public new SerializationVector3 Rotation { get; set; }
        [XmlIgnore]
        public new SerializationVector3 Scale { get; set; }
        [XmlAttribute(AttributeName = "ShowControlPolygon")]
        public bool DisplayPolygonal { get; set; }

        public InterpolationBezierCurveC2SerializationModel(InterpolationBezierCurveC2 bezierCurveC0) : base(bezierCurveC0)
        {
            DisplayPolygonal = false;
            PointsRef = new List<PointRef>();
            foreach (var p in bezierCurveC0.Nodes)
            {
                PointsRef.Add(new PointRef() { Name = p.Name });
            }
        }

        public InterpolationBezierCurveC2SerializationModel() { }

        public override (IGeometryObject g, GeometryType type) GetSceneObject()
        {
            InterpolationBezierCurveC2 c = ObjectsStaticFactory.CreateObject(GeometryType.InterpolationBezierCurveC2) as InterpolationBezierCurveC2;
            c.Name = Name;
            return (c, GeometryType.InterpolationBezierCurveC2);
        }
    }
}
