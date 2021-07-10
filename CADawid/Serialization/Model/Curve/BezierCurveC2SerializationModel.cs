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
    public class BezierCurveC2SerializationModel : CurveSerializationModel
    {
        [XmlIgnore]
        public new SerializationVector3 Position { get; set; }
        [XmlIgnore]
        public new SerializationVector3 Rotation { get; set; }
        [XmlIgnore]
        public new SerializationVector3 Scale { get; set; }

        [XmlAttribute(AttributeName = "ShowBernsteinPoints")]
        public bool DisplayBernsteinNodes { get; set; }
        [XmlAttribute(AttributeName = "ShowBernsteinPolygon")]
        public bool DisplayBernsteinPolygon { get; set; }
        [XmlAttribute(AttributeName = "ShowDeBoorPolygon")]
        public bool DisplayBsplinePolygon { get; set; }


        public BezierCurveC2SerializationModel(BezierCurveC2 bezierCurveC2) : base(bezierCurveC2)
        {
            DisplayBernsteinNodes = bezierCurveC2.DisplayBernsteinNodes;
            DisplayBernsteinPolygon = bezierCurveC2.DisplayBernsteinPolygon;
            DisplayBsplinePolygon = bezierCurveC2.DisplayBsplinePolygon;
            PointsRef = new List<PointRef>();
            foreach (var p in bezierCurveC2.Nodes)
            {
                PointsRef.Add(new PointRef() { Name = p.Name });
            }
        }

        public BezierCurveC2SerializationModel() { }

        public override (IGeometryObject g, GeometryType type) GetSceneObject()
        {
            BezierCurveC2 c = ObjectsStaticFactory.CreateObject(GeometryType.BezierCurveC2) as BezierCurveC2;
            c.DisplayBernsteinNodes = DisplayBernsteinNodes;
            c.DisplayBernsteinPolygon = DisplayBernsteinPolygon;
            c.DisplayBsplinePolygon = DisplayBsplinePolygon;
            c.Name = Name;
            return (c, GeometryType.BezierCurveC2);
        }
    }
}
