using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CADawid.Model;
using CADawid.Serialization.Model;

namespace CADawid.Serialization
{
    public class SceneSerializer
    {
        public SceneSerializer()
        {
        }

        public void Serialize(Scene scene, string filename)
        {
            SerializationScene s = new SerializationScene(scene.AllGeometries.Where(p => !(p is GregoryPatch)).ToList());
            XmlSerializer serializer = new XmlSerializer(typeof(SerializationScene));
            using (TextWriter writer = new StreamWriter(filename))
            {
                serializer.Serialize(writer, s);
            }
        }

        public void Deserialize(Scene scene, MainWindow app, string filename)
        {
            try
            {
                Dictionary<string, IGeometryObject> points = new Dictionary<string, IGeometryObject>();
                FileStream fs = new FileStream(filename, FileMode.Open);
                XmlSerializer x = new XmlSerializer(typeof(SerializationScene));
                SerializationScene s = (SerializationScene)x.Deserialize(fs);
                //pass for points
                foreach (SerializationModel model in s.SerializationModels)
                {
                    (IGeometryObject g, GeometryType type) = model.GetSceneObject();
                    if (g is Point p)
                    {
                        points.Add(p.Name, p);
                        app.AddGeometry(type, g);
                    }
                }
                foreach (SerializationModel model in s.SerializationModels)
                {
                    (IGeometryObject g, GeometryType type) = model.GetSceneObject();
                    if (g is ICurve curve)
                    {
                        CurveSerializationModel csm = model as CurveSerializationModel;
                        foreach (var rf in csm.PointsRef)
                        {
                            curve.AddNode(points[rf.Name]);
                        }
                        app.AddGeometry(type, curve as IGeometryObject);
                    }
                    else if (g is ISurface patch)
                    {
                        PatchSerializationModel psm = model as PatchSerializationModel;

                        Point[,] nodes = new Point[psm.vNodes, psm.uNodes];
                        foreach (GridPointRef rf in psm.PointsRef)
                        {
                            Point p = points[rf.PointRef] as Point;
                            p.IsRemovable = false;
                            if (psm.WrapDirection == SurfaceCylinderDirection.Row)
                            {
                                nodes[rf.Column, rf.Row] = p;
                            }
                            else
                            {
                                nodes[rf.Row, rf.Column] = p;
                            }

                        }
                        patch.SetNodes(nodes);
                        app.AddGeometry(type, patch as IGeometryObject);

                    }
                    else if (g is Point)
                    {
                        continue;
                    }
                    else
                    {
                        app.AddGeometry(type, g);
                    }

                }
            }
            catch
            {
            }
            
        }
    }
}
