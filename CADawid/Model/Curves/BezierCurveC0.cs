using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CADawid.DxModule;
using CADawid.Serialization.Model;
using CADawid.Utils;
using CADawid.ViewModel;
using SharpDX;
using SharpDX.Direct3D;

namespace CADawid.Model
{
    public class BezierCurveC0 : Curve<Vertex, Index>
    {
        public float Precision { get; set; } = 50;
        private const float MinPrecision = 20f;
        private const float MaxPrecision = 300f;

        private PolygonalChain bernsteinPolygon;
        public bool DisplayPolygonal { get; set; }
        public BezierCurveC0(Vector4 color, Vector4 unselectedColor, List<Model.Point> nodes, bool isRemovable = true) : base(color, unselectedColor, nodes, isRemovable)
        {
            DisplayPolygonal = false;
            bernsteinPolygon = new PolygonalChain(0.5f * color, 0.5f * unselectedColor, nodes);
        }

        public void SetPrecision(DxCamera camera)
        {
            if (Nodes.Count > 2)
            {
                bool anySeen = false;
                List<Vector3> nodes = new List<Vector3>();

                foreach (Point p in Nodes)
                {
                    Vector3 screenNode = p.GetScreenPosition(camera);
                    if(screenNode.X < camera.Width && screenNode.X > 0f &&
                        screenNode.Y < camera.Height && screenNode.Y > 0f)
                    {
                        anySeen = true;
                    }
                    nodes.Add(screenNode);
                }
                if(!anySeen)
                {
                    Precision = 2f;
                    return;
                }
                Precision = (float)Math.Floor(Algorithm.PolygonArea(nodes) / 100f);
                if (Precision < MinPrecision)
                {
                    Precision = MinPrecision;
                }
                else if(Precision > MaxPrecision)
                {
                    Precision = MaxPrecision;
                }
            }
            else
            {
                Precision = 2f;
            }
            ResetGeometry();
        }

        public override void ResetGeometry()
        {
            bernsteinPolygon.ResetGeometry();
            base.ResetGeometry();
        }
        public override bool AddNode(IGeometryObject node)
        {
            bernsteinPolygon.AddNode(node);
            return base.AddNode(node);
        }
        public override void AddNodes(IList nodes)
        {
            bernsteinPolygon.AddNodes(nodes);
            base.AddNodes(nodes);
        }
        public override void RemoveNodes(IGeometryObject[] removedGeometries)
        {
            bernsteinPolygon.RemoveNodes(removedGeometries);
            base.RemoveNodes(removedGeometries);
        }
        public override void RemoveNodes(IList removedGeometries)
        {
            bernsteinPolygon.RemoveNodes(removedGeometries);
            base.RemoveNodes(removedGeometries);
        }

        public override GeometryViewModel GetViewModel()
        {
            return new BezierCurveC0ViewModel()
            {
                SelectedObject = this
            };
        }
        public override IGeometryObject Copy()
        {
            IGeometryObject copied = new BezierCurveC0(Color, UnselectedColor, Nodes);
            return copied;
        }
        protected override Geometry<Vertex, Index> GenerateGeometry()
        {
            List<Vertex> vertexList = new List<Vertex>();
            float step = 1.0f / (Precision - 1);

            int segmentLength = 4;
            int s = 0;
            while(s < Nodes.Count)
            {
                int length = s + segmentLength <= Nodes.Count ? segmentLength : Nodes.Count - s;

                float t0 = 0;
                float t = t0;
                IEnumerable<Vector3> nodes = Nodes
                    .GetRange(s, length)
                    .Select(n => n.CurrentPosition);
                for (int i = 0; i < Precision; i++)
                {
                    Vector3 tValue = Algorithm.DeCasteljau<Vector3>(nodes, t);
                    vertexList.Add(new Vertex(tValue.X, tValue.Y, tValue.Z));
                    t += step;
                }

                s = s + segmentLength - 1;
            }
            
            Index[] edgeArray = new Index[(vertexList.Count - 1) * 2];
            ushort node = 0;
            for (int i = 0; i < edgeArray.Length; i += 2)
            {
                edgeArray[i] = new Index(node);
                edgeArray[i + 1] = new Index((UInt16)(node + 1));
                node++;
            }
            return new Geometry<Vertex, Index>(vertexList.ToArray(), edgeArray);

        }
        public override void Render(DxRenderer dxRenderer, Matrix worldTransform, bool isSelected, Func<Vector4, Vector4> modifier)
        {
            if (DisplayPolygonal)
            {
                bernsteinPolygon.Render(dxRenderer, worldTransform, isSelected, modifier);
            }

            dxRenderer.device.ImmediateContext.VertexShader.Set(dxRenderer.vertexShader);
            dxRenderer.device.ImmediateContext.PixelShader.Set(dxRenderer.pixelShader);
            dxRenderer.device.ImmediateContext.GeometryShader.Set(null);
            dxRenderer.device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
            base.Render(dxRenderer, worldTransform, isSelected, modifier);
        }

        public override SerializationModel GetSerializationModel()
        {
            return new BezierCurveC0SerializationModel(this);
        }
    }
}
