using System;
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
using SharpDX.Direct3D11;

namespace CADawid.Model
{
    public class BicubicBezierPatchC0 : BicubicPatch<PatchVertex, Index>
    {
        private Grid bezierGrid;

        public Point[,] GenerateNodes(Vector3 center)
        {
            if (IsCylindrical)
            {
                int uNodes = 4 * PatchesU - (PatchesU - 1) - 1;
                int vNodes = 4 * PatchesV - (PatchesV - 1);

                Point[,] nodes = new Point[vNodes, uNodes];

                float stepU = 2.0f * (float)Math.PI / (uNodes);
                float stepV = Height / (vNodes - 1);

                float v = -Height / 2;
                for (int i = 0; i < vNodes; i++)
                {
                    float u = 0.0f;
                    for (int j = 0; j < uNodes; j++)
                    {
                        Model.Point bezierPoint = new Model.Point(0.5f, new Vector4(1, 0, 0, 1),
                               new Vector4(1, 0, 0, 0.3f), false)
                        {
                            TranslationV = new MyVector3(center.X + R * (float)Math.Sin(u), center.Y + v, center.Z + R * (float)Math.Cos(u))
                        };
                        nodes[i, j] = bezierPoint;
                        u += stepU;
                    }
                    v += stepV;
                }
                return nodes;
            }
            else
            {
                int uNodes = 4 * PatchesU - (PatchesU - 1);
                int vNodes = 4 * PatchesV - (PatchesV - 1);

                Point[,] nodes = new Point[vNodes, uNodes];

                float stepU = LengthU / (uNodes - 1);
                float stepV = LengthV / (vNodes - 1);

                float v = -LengthV / 2;
                for (int i = 0; i < vNodes; i++)
                {
                    float u = -LengthU / 2;
                    for (int j = 0; j < uNodes; j++)
                    {
                        Model.Point bezierPoint = new Model.Point(0.5f, new Vector4(1, 0, 0, 1),
                               new Vector4(1, 0, 0, 0.3f), false)
                        {
                            TranslationV = new MyVector3(center.X + v, center.Y + u, center.Z)
                        };
                        nodes[i, j] = bezierPoint;
                        u += stepU;
                    }
                    v += stepV;
                }
                return nodes;
            }
        }
        public override void SetNodes(Point[,] nodes)
        {
            if (Nodes != null)
            {
                foreach (Point p in Nodes)
                {
                    p.RemoveFromObject(this);
                }
            }

            Nodes = nodes;
            foreach (var p in Nodes)
            {
                p.OnTransformed += UpdateGeometry;
                p.AddToObject(this);
            }
            bezierGrid = new Grid(new Vector4(1, 1, 1, 1), new Vector4(1, 1, 1, 1), Nodes, IsRemovable, IsCylindrical);
        }

        public BicubicBezierPatchC0(int patchesU, int patchesV, float lengthU, 
            float lengthV, float r, float height,
            int precisionU, int precisionV, bool isCylindrical,
            Vector4 color, Vector4 unselectedColor, bool isRemovable = true) 
            : base(color, unselectedColor, isRemovable)
        {
            DisplayBezierGrid = false;
            IsCylindrical = isCylindrical;
            PatchesU = patchesU;
            PatchesV = patchesV;
            LengthU = lengthU;
            LengthV = lengthV;
            R = r;
            Height = height;
            PrecisionU = precisionU;
            PrecisionV = precisionV;

            //GenerateNodes(new Vector3(0, 0, 0));
        }

        public override GeometryViewModel GetViewModel()
        {
            return new PatchViewModel()
            {
                SelectedObject = this
            };
        }
        public override IGeometryObject Copy()
        {
            BicubicBezierPatchC0 copied = new BicubicBezierPatchC0(PatchesU, PatchesV, LengthU, LengthV, R, Height, 
                PrecisionU, PrecisionV, IsCylindrical, Color, UnselectedColor);
            return copied;
        }


        protected override Geometry<PatchVertex, Index> GenerateGeometry()
        {
            List<PatchVertex> vertexList = new List<PatchVertex>();
            for (int pu = 0; pu < PatchesU; pu++)
            {
                for (int pv = 0; pv < PatchesV; pv++)
                {
                    Vector3[,] nodes = new Vector3[4, 4];
                    for (int i = 3 * pv; i < 3 * pv + 4; i++)
                    {
                        nodes[i - 3 * pv, 0] = Nodes[i, (3 * pu) % Nodes.GetLength(1)].CurrentPosition;
                        nodes[i - 3 * pv, 1] = Nodes[i, (3 * pu + 1) % Nodes.GetLength(1)].CurrentPosition;
                        nodes[i - 3 * pv, 2] = Nodes[i, (3 * pu + 2) % Nodes.GetLength(1)].CurrentPosition;
                        nodes[i - 3 * pv, 3] = Nodes[i, (3 * pu + 3) % Nodes.GetLength(1)].CurrentPosition;
                    }
                    vertexList.Add(new PatchVertex(nodes));
                }
            }
            Index[] indexArr = new Index[1];
            return new Geometry<PatchVertex, Index>(vertexList.ToArray(), indexArr);
        }
        public override void Render(DxRenderer dxRenderer, Matrix worldTransform, bool isSelected, Func<Vector4, Vector4> modifier)
        {
            if(DisplayBezierGrid)
            {
                bezierGrid.Render(dxRenderer, worldTransform, isSelected, modifier);
            }

            dxRenderer.device.ImmediateContext.VertexShader.Set(dxRenderer.vertexShaderPatch);
            dxRenderer.device.ImmediateContext.PixelShader.Set(dxRenderer.pixelShaderPatch);
            dxRenderer.device.ImmediateContext.GeometryShader.Set(dxRenderer.geometryShaderPatch);
            dxRenderer.device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;
            dxRenderer.device.ImmediateContext.InputAssembler.InputLayout = dxRenderer.inputLayoutPatch;

            Geometry<PatchVertex, Index> g = TryUpdateGeometry(dxRenderer);
            if (g.Indices.Length == 0 || g.Vertices.Length == 0)
            {
                return;
            }
            unsafe
            {
                dxRenderer.SetVertexBuffer<PatchVertex>(vertexBuffer, sizeof(PatchVertex));
            }
            dxRenderer.SetIndexBuffer(indexBuffer);

            Matrix modelMatrix = Model * worldTransform;
            DxConstantBuffer cb = new DxConstantBuffer();
            cb.MVP = modelMatrix * dxRenderer.Scene.Camera.VP;
            cb.Color = modifier(isSelected ? Color : UnselectedColor);
            dxRenderer.UpdateConstantBuffer(ref dxRenderer.geometryConstantBuffer, cb);

            DxPatchConstantBuffer pcb = new DxPatchConstantBuffer();
            pcb.precision = new Vector2(PrecisionU, PrecisionV);
            dxRenderer.UpdateConstantBuffer(ref dxRenderer.patchConstantBuffer, pcb);

            dxRenderer.device.ImmediateContext.Draw(g.Vertices.Length, 0);

            dxRenderer.device.ImmediateContext.InputAssembler.InputLayout = dxRenderer.inputLayout;
        }

        public override SerializationModel GetSerializationModel()
        {
            return new BicubicBezierPatchC0SerializationModel(this);
        }

        public struct Edge
        {
            public int startI;
            public int startJ;
            public int endI;
            public int endJ;

            public Edge(int startI = 0, int startJ = 0, int endI = 0, int endJ = 0)
            {
                this.startI = startI;
                this.startJ = startJ;
                this.endI = endI;
                this.endJ = endJ;
            }
        }

        public List<(Edge, BicubicBezierPatchC0)> GetBorderEdges(List<Point> corners)
        {
            List<(Edge, BicubicBezierPatchC0)> edges = new List<(Edge, BicubicBezierPatchC0)>();
            int l = IsCylindrical ? 1 : 0;
            for(int i = 0; i < Nodes.GetLength(0); i+=3)
            {
                for(int j = 0; j < Nodes.GetLength(1) + l; j+=3)
                {
                    if(i == 0 || i == Nodes.GetLength(0) - 1)
                    {
                        if(corners.Contains(Nodes[i,j % Nodes.GetLength(1)]))
                        {
                            if(j - 3 >=0)
                            {
                                if (corners.Contains(Nodes[i, (j - 3) % Nodes.GetLength(1)]))
                                {
                                    edges.Add((new Edge(i, j, i, j - 3), this));
                                }
                            }
                            if(j + 3 <= Nodes.GetLength(1) - 1 + l)
                            {
                                if (corners.Contains(Nodes[i, (j + 3) % Nodes.GetLength(1)]))
                                {
                                    edges.Add((new Edge(i, j, i, j + 3), this));
                                }
                            }
                        }
                    }
                    if(!IsCylindrical)
                    {
                        if (j == 0 || j == Nodes.GetLength(1) - 1)
                        {
                            if (corners.Contains(Nodes[i, j]))
                            {
                                if (i - 3 >= 0)
                                {
                                    if (corners.Contains(Nodes[i - 3, j]))
                                    {
                                        edges.Add((new Edge(i, j, i - 3, j), this));
                                    }
                                }
                                if (i + 3 <= Nodes.GetLength(0) - 1)
                                {
                                    if (corners.Contains(Nodes[i + 3, j]))
                                    {
                                        edges.Add((new Edge(i, j, i + 3, j), this));
                                    }
                                }
                            }
                        }
                    }
                    
                }
            }
            return edges;
        }
    }
}
