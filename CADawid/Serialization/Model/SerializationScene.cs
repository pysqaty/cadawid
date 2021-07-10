using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CADawid.Model;

namespace CADawid.Serialization.Model
{
    [XmlRoot("Scene", Namespace = "http://mini.pw.edu.pl/mg1")]
    public class SerializationScene
    {
        [XmlElement(typeof(TorusSerializationModel),
            ElementName = "Torus")]
        [XmlElement(typeof(PointSerializationModel),
            ElementName = "Point")]
        [XmlElement(typeof(BezierCurveC0SerializationModel),
            ElementName = "BezierC0")]
        [XmlElement(typeof(BezierCurveC2SerializationModel),
            ElementName = "BezierC2")]
        [XmlElement(typeof(InterpolationBezierCurveC2SerializationModel),
            ElementName = "BezierInter")]
        [XmlElement(typeof(BicubicBezierPatchC0SerializationModel),
            ElementName = "PatchC0")]
        [XmlElement(typeof(BicubicBsplinePatchC2SerializationModel),
            ElementName = "PatchC2")]
        public List<SerializationModel> SerializationModels { get; set; }

        public SerializationScene(ICollection<IGeometryObject> scene)
        {
            Dictionary<string, int> names = new Dictionary<string, int>();
            SerializationModels = new List<SerializationModel>();
            foreach (IGeometryObject geometryObject in scene)
            {
                string name = geometryObject.Name.Split('#')[0];
                if(names.ContainsKey(name))
                {
                    geometryObject.Name = name + "#" + names[name]++;
                }
                else
                {
                    names.Add(name, 1);
                }
                
                SerializationModel sm = geometryObject.GetSerializationModel();
                SerializationModels.Add(sm);
            }
        }

        public SerializationScene() { }
    }
}
