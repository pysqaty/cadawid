using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CADawid.Model;
using SharpDX;

namespace CADawid.Serialization.Model
{
    public class TorusSerializationModel : SerializationModel
    {
        [XmlAttribute(AttributeName = "MajorRadius")]
        public float R { get; set; }
        [XmlAttribute(AttributeName = "MinorRadius")]
        public float r { get; set; }
        [XmlAttribute(AttributeName = "VerticalSlices")]
        public int Precision1 { get; set; }
        [XmlAttribute(AttributeName = "HorizontalSlices")] 
        public int Precision2 { get; set; }

        public TorusSerializationModel(Torus torus) : base(torus)
        {
            R = torus.R;
            r = torus.r;
            Precision1 = torus.Precision1;
            Precision2 = torus.Precision2;
        }

        public TorusSerializationModel() { }

        public override (IGeometryObject g, GeometryType type) GetSceneObject()
        {
            Torus t = ObjectsStaticFactory.CreateObject(GeometryType.Torus) as Torus;
            t.R = R;
            t.r = r;
            t.Precision1 = Precision1;
            t.Precision2 = Precision2;
            t.TranslationV = Position;
            t.RotationV = Rotation;
            t.ScaleV = Scale;
            t.Name = Name;
            return (t, GeometryType.Torus);
        }
    }
}
