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
    public class BezierCurveC0SerializationModel : CurveSerializationModel
    {
        [XmlIgnore]
        public new SerializationVector3 Position { get; set; }
        [XmlIgnore]
        public new SerializationVector3 Rotation { get; set; }
        [XmlIgnore]
        public new SerializationVector3 Scale { get; set; }
        [XmlAttribute(AttributeName = "ShowControlPolygon")]
        public bool DisplayPolygonal { get; set; }


        public BezierCurveC0SerializationModel(BezierCurveC0 bezierCurveC0) : base(bezierCurveC0)
        {
            DisplayPolygonal = bezierCurveC0.DisplayPolygonal;
            PointsRef = new List<PointRef>();
            foreach(var p in bezierCurveC0.Nodes)
            {
                PointsRef.Add(new PointRef() { Name = p.Name });
            }
        }

        public BezierCurveC0SerializationModel() { }

        public override (IGeometryObject g, GeometryType type) GetSceneObject()
        {
            BezierCurveC0 c = ObjectsStaticFactory.CreateObject(GeometryType.BezierCurveC0) as BezierCurveC0;
            c.Name = Name;
            c.DisplayPolygonal = DisplayPolygonal;
            return (c, GeometryType.BezierCurveC0);
        }
    }
}
