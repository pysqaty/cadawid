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

namespace CADawid.Model
{
    public class BicubicBsplinePatchC2 : BicubicPatch<PatchVertex, Index>
    {
        private Grid bezierGrid;

        public Point[,] GenerateNodes(Vector3 center)
        {
            if(IsCylindrical)
            {
                int uNodes = 4 * PatchesU - 3 * (PatchesU - 1) - 3;
                int vNodes = 4 * PatchesV - 3 * (PatchesV - 1);

                float r = R;//(R / (uNodes - 3)) * (uNodes - 1);
                float height = (Height / (vNodes - 3)) * (vNodes - 1);

                Point[,] nodes = new Point[vNodes, uNodes];

                float stepU = 2.0f * (float)Math.PI / (uNodes);
                float stepV = height / (vNodes - 1);

                float v = -height / 2;
                for (int i = 0; i < vNodes; i++)
                {
                    float u = 0.0f;
                    for (int j = 0; j < uNodes; j++)
                    {
                        Model.Point bezierPoint = new Model.Point(0.5f, new Vector4(1, 0, 0, 1),
                               new Vector4(1, 0, 0, 0.3f), false)
                        {
                            TranslationV = new MyVector3(center.X + v, center.Y + r * (float)Math.Sin(u), center.Z + r * (float)Math.Cos(u))
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
                int uNodes = 4 * PatchesU - 3 * (PatchesU - 1);
                int vNodes = 4 * PatchesV - 3 * (PatchesV - 1);

                float lengthU = (LengthU / (uNodes - 3)) * (uNodes - 1);
                float lengthV = (LengthV / (vNodes - 3)) * (vNodes - 1);

                Point[,] nodes = new Point[vNodes, uNodes];

                float stepU = lengthU / (uNodes - 1);
                float stepV = lengthV / (vNodes - 1);

                float v = -lengthV / 2;
                for (int i = 0; i < vNodes; i++)
                {
                    float u = -lengthU / 2;
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
            if(Nodes != null)
            {
                foreach (Point p in Nodes)
                {
                    p.RemoveFromObject(this);
                }
            }
            
            Nodes = nodes;
            foreach(var p in Nodes)
            {
                p.OnTransformed += UpdateGeometry;
                p.AddToObject(this);
            }
            bezierGrid = new Grid(new Vector4(1, 1, 1, 1), new Vector4(1, 1, 1, 1), Nodes, IsRemovable, IsCylindrical);
        }

        public BicubicBsplinePatchC2(int patchesU, int patchesV, float lengthU,
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

        public override IGeometryObject Copy()
        {
            BicubicBsplinePatchC2 copied = new BicubicBsplinePatchC2(PatchesU, PatchesV, LengthU, LengthV, R, Height,
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
                    for (int i = pv; i < pv + 4; i++)
                    {
                        nodes[i - pv, 0] = Nodes[i, (pu) % Nodes.GetLength(1)].CurrentPosition;
                        nodes[i - pv, 1] = Nodes[i, (pu + 1) % Nodes.GetLength(1)].CurrentPosition;
                        nodes[i - pv, 2] = Nodes[i, (pu + 2) % Nodes.GetLength(1)].CurrentPosition;
                        nodes[i - pv, 3] = Nodes[i, (pu + 3) % Nodes.GetLength(1)].CurrentPosition;
                    }
                    vertexList.Add(new PatchVertex(nodes));
                }
            }
            Index[] indexArr = new Index[1];
            return new Geometry<PatchVertex, Index>(vertexList.ToArray(), indexArr);
        }

        public override void Render(DxRenderer dxRenderer, Matrix worldTransform, bool isSelected, Func<Vector4, Vector4> modifier)
        {
            if (DisplayBezierGrid)
            {
                bezierGrid.Render(dxRenderer, worldTransform, isSelected, modifier);
            }

            dxRenderer.device.ImmediateContext.VertexShader.Set(dxRenderer.vertexShaderBsplinePatch);
            dxRenderer.device.ImmediateContext.PixelShader.Set(dxRenderer.pixelShaderBsplinePatch);
            dxRenderer.device.ImmediateContext.GeometryShader.Set(dxRenderer.geometryShaderBsplinePatch);
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

        public override GeometryViewModel GetViewModel()
        {
            return new PatchViewModel()
            {
                SelectedObject = this
            };
        }

        public override SerializationModel GetSerializationModel()
        {
            return new BicubicBsplinePatchC2SerializationModel(this);
        }
    }
}
