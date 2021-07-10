using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CADawid.Model;

namespace CADawid.Serialization.Model
{
    public abstract class SerializationModel
    {
        [XmlAttribute]
        public string Name { get; set; }
        public SerializationVector3 Position { get; set; }
        public SerializationVector3 Rotation { get; set; }
        public SerializationVector3 Scale { get; set; }

        public SerializationModel(IGeometryObject model)
        {
            Name = model.Name;
            Position = model.TranslationV;
            Rotation = model.RotationV;
            Scale = model.ScaleV;
        }

        public SerializationModel() { }

        public abstract (IGeometryObject g, GeometryType type) GetSceneObject();

    }
}
