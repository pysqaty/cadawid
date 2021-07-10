using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CADawid.Model;

namespace CADawid.Serialization.Model
{
    public class PointSerializationModel : SerializationModel
    {
        [XmlIgnore]
        public new SerializationVector3 Rotation { get; set; }
        [XmlIgnore]
        public new SerializationVector3 Scale { get; set; }
        public PointSerializationModel(Point point) : base(point)
        {

        }

        public PointSerializationModel() { }

        public override (IGeometryObject g, GeometryType type) GetSceneObject()
        {
            Point p = ObjectsStaticFactory.CreateObject(GeometryType.Point) as Point;
            p.TranslationV = Position;
            p.Name = Name;
            return (p, GeometryType.Point);
        }
    }
}
