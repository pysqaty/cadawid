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
    public class BicubicBsplinePatchC2SerializationModel : PatchSerializationModel
    {
        [XmlIgnore]
        public new SerializationVector3 Position { get; set; }
        [XmlIgnore]
        public new SerializationVector3 Rotation { get; set; }
        [XmlIgnore]
        public new SerializationVector3 Scale { get; set; }



        public BicubicBsplinePatchC2SerializationModel(BicubicBsplinePatchC2 patch) : base(patch)
        {
            WrapDirection = patch.IsCylindrical ? SurfaceCylinderDirection.Column : SurfaceCylinderDirection.None;
            PrecisionU = patch.PrecisionU;
            PrecisionV = patch.PrecisionV;
            DisplayBezierGrid = patch.DisplayBezierGrid;

            PointsRef = new List<GridPointRef>();
            for (int i = 0; i < patch.Nodes.GetLength(0); i++)
            {
                for (int j = 0; j < patch.Nodes.GetLength(1); j++)
                {
                    PointsRef.Add(new GridPointRef()
                    {
                        Row = i,
                        Column = j,
                        PointRef = patch.Nodes[i, j].Name
                    });
                }
            }
        }

        public BicubicBsplinePatchC2SerializationModel() { }

        public override (IGeometryObject g, GeometryType type) GetSceneObject()
        {
            BicubicBsplinePatchC2 c = ObjectsStaticFactory.CreateObject(GeometryType.BicubicBsplinePatchC2) as BicubicBsplinePatchC2;
            c.Name = Name;
            c.IsCylindrical = WrapDirection == SurfaceCylinderDirection.None ? false : true;
            c.DisplayBezierGrid = DisplayBezierGrid;

            int rows = int.MinValue;
            int cols = int.MinValue;
            foreach (GridPointRef rf in PointsRef)
            {
                rows = rf.Row > rows ? rf.Row : rows;
                cols = rf.Column > cols ? rf.Column : cols;
            }

            if (WrapDirection == SurfaceCylinderDirection.Column)
            {
                c.PrecisionU = PrecisionU;
                c.PrecisionV = PrecisionV;
                vNodes = rows + 1;
                uNodes = cols + 1;
                PatchesU = uNodes;
                PatchesV = vNodes - 3;
            }
            else if(WrapDirection == SurfaceCylinderDirection.Row)
            {
                c.PrecisionU = PrecisionV;
                c.PrecisionV = PrecisionU;
                uNodes = rows + 1;
                vNodes = cols + 1;
                PatchesU = uNodes;
                PatchesV = vNodes - 3;
            }
            else
            {
                c.PrecisionU = PrecisionU;
                c.PrecisionV = PrecisionV;
                vNodes = rows + 1;
                uNodes = cols + 1;
                PatchesU = uNodes - 3;
                PatchesV = vNodes - 3;
            }

            c.PatchesV = PatchesV;
            c.PatchesU = PatchesU;

            return (c, GeometryType.BicubicBsplinePatchC2);
        }
    }
}
