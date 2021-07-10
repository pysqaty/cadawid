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
    public class InterpolationBezierCurveC2 : Curve<CurveVertex, Index>
    {
        public InterpolationBezierCurveC2(Vector4 color, Vector4 unselectedColor, List<Point> nodes, bool isRemovable = true) : base(color, unselectedColor, nodes, isRemovable)
        {
        }

        public override GeometryViewModel GetViewModel()
        {
            return new InterpolationBezierCurveC2ViewModel()
            {
                SelectedObject = this
            };
        }
        public override IGeometryObject Copy()
        {
            IGeometryObject copied = new InterpolationBezierCurveC2(Color, UnselectedColor, Nodes);
            return copied;
        }

        protected override Geometry<CurveVertex, Index> GenerateGeometry()
        {
            if (Nodes.Count <= 2)
            {
                CurveVertex[] vertices = new CurveVertex[1];
                Index[] indices = new Index[1];
                List<Point> bernsteinPoints = new List<Point>();
                if (Nodes.Count == 2)
                {
                    bernsteinPoints = Algorithm.EvaluateOnLine(Nodes[0], Nodes[1], 0, 1, new List<float>() { 0.0f, 1.0f / 3, 2.0f / 3, 1.0f });
                }
                else if(Nodes.Count == 1)
                {
                    bernsteinPoints = Algorithm.EvaluateOnLine(Nodes[0], Nodes[0], 0, 1, new List<float>() { 0.0f, 1.0f / 3, 2.0f / 3, 1.0f });
                }


                vertices[0] = new CurveVertex(new Vector4(bernsteinPoints[0].CurrentPosition, 1),
                            new Vector4(bernsteinPoints[1].CurrentPosition, 1),
                            new Vector4(bernsteinPoints[2].CurrentPosition, 1),
                            new Vector4(bernsteinPoints[3].CurrentPosition, 1));

                return new Geometry<CurveVertex, Index>(vertices, indices);
            }
            List<float> knots = new List<float>();
            List<float> diffs = new List<float>();

            List<Vector3> coefficientsA = new List<Vector3>();
            List<Vector3> coefficientsB = new List<Vector3>();
            List<Vector3> coefficientsC = new List<Vector3>();
            List<Vector3> coefficientsD = new List<Vector3>();
            
            knots.Add(0);
            for(int i = 0; i < Nodes.Count - 1; i++)
            {
                Vector3 deltaPi = Nodes[i + 1].CurrentPosition - Nodes[i].CurrentPosition;
                float di = (float)Math.Sqrt(deltaPi.X * deltaPi.X + 
                    deltaPi.Y * deltaPi.Y + 
                    deltaPi.Z * deltaPi.Z);

                diffs.Add(di);
                knots.Add(knots.Last() + di);
            }

            float[] a = new float[Nodes.Count - 2];
            a[0] = 0;
            float[] b = new float[Nodes.Count - 2];
            float[] c = new float[Nodes.Count - 2];
            c[c.Length - 1] = 0;
            Vector3[] d = new Vector3[Nodes.Count - 2];

            for(int i = 1; i < a.Length; i++)
            {
                a[i] = diffs[i - 1] / (diffs[i - 1] + diffs[i]);
            }

            for(int i = 0; i < b.Length; i++)
            {
                b[i] = 2;
            }

            for(int i = 0; i < c.Length - 1; i++)
            {
                c[i] = diffs[i + 1] / (diffs[i] + diffs[i + 1]);
            }

            for(int i = 0; i < d.Length; i++)
            {
                Vector3 Pim1 = Nodes[i].CurrentPosition;
                Vector3 Pi = Nodes[i + 1].CurrentPosition;
                Vector3 Pip1 = Nodes[i + 2].CurrentPosition;
                float dim1 = diffs[i] < float.Epsilon ? 1 : diffs[i];
                float di = diffs[i + 1] < float.Epsilon ? 1 : diffs[i + 1];
                float sumDis = diffs[i] < float.Epsilon && diffs[i + 1] < float.Epsilon ? 1 : dim1 + di;
                d[i] = 3 * ((Pip1 - Pi) / di - (Pi - Pim1) / dim1) / (sumDis);
            }

            Vector3[] cC = Algorithm.ThomasAlgorithm(a, b, c, d);
            coefficientsC.Add(new Vector3(0));
            coefficientsC.AddRange(cC);
            coefficientsC.Add(new Vector3(0));

            for(int i = 0; i < coefficientsC.Count - 1; i++)
            {
                Vector3 cd = 2 * (coefficientsC[i + 1] - coefficientsC[i]) / (6 * diffs[i]);
                coefficientsD.Add(cd);

                coefficientsA.Add(Nodes[i].CurrentPosition);
            }

            for(int i = 0; i < diffs.Count; i++)
            {
                Vector3 y = Nodes[i + 1].CurrentPosition - coefficientsA[i] -
                    coefficientsC[i] * diffs[i] * diffs[i] -
                    coefficientsD[i] * diffs[i] * diffs[i] * diffs[i];
                coefficientsB.Add(y / diffs[i]);
            }


            CurveVertex[] vertexArr = new CurveVertex[diffs.Count];
            for (int i = 0; i < diffs.Count; i++)
            {
                coefficientsA[i] *= 1;
                coefficientsB[i] *= diffs[i];
                coefficientsC[i] *= (diffs[i] * diffs[i]);
                coefficientsD[i] *= (diffs[i] * diffs[i] * diffs[i]);
                Vector4 bernsteinBasisX = BasisConverter.PowerToBernstein(coefficientsA[i].X, coefficientsB[i].X,
                    coefficientsC[i].X, coefficientsD[i].X);
                Vector4 bernsteinBasisY = BasisConverter.PowerToBernstein(coefficientsA[i].Y, coefficientsB[i].Y,
                    coefficientsC[i].Y, coefficientsD[i].Y);
                Vector4 bernsteinBasisZ = BasisConverter.PowerToBernstein(coefficientsA[i].Z, coefficientsB[i].Z,
                    coefficientsC[i].Z, coefficientsD[i].Z);
                                                                                    
                Vector4 b0 = new Vector4(bernsteinBasisX.X, bernsteinBasisY.X, bernsteinBasisZ.X, 1);
                Vector4 b1 = new Vector4(bernsteinBasisX.Y, bernsteinBasisY.Y, bernsteinBasisZ.Y, 1);
                Vector4 b2 = new Vector4(bernsteinBasisX.Z, bernsteinBasisY.Z, bernsteinBasisZ.Z, 1);
                Vector4 b3 = new Vector4(bernsteinBasisX.W, bernsteinBasisY.W, bernsteinBasisZ.W, 1);

                vertexArr[i] = new CurveVertex(b0, b1, b2, b3);
            }
            Index[] indexArr = new Index[1];
            return new Geometry<CurveVertex, Index>(vertexArr, indexArr);
        }

        public override void Render(DxRenderer dxRenderer, Matrix worldTransform, bool isSelected, Func<Vector4, Vector4> modifier)
        {
            dxRenderer.device.ImmediateContext.VertexShader.Set(dxRenderer.vertexShaderCurve);
            dxRenderer.device.ImmediateContext.PixelShader.Set(dxRenderer.pixelShaderCurve);
            dxRenderer.device.ImmediateContext.GeometryShader.Set(dxRenderer.geometryShaderCurve);

            dxRenderer.device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;
            dxRenderer.device.ImmediateContext.InputAssembler.InputLayout = dxRenderer.inputLayoutCurve;

            Geometry<CurveVertex, Index> g = TryUpdateGeometry(dxRenderer);
            if (g.Indices.Length == 0 || g.Vertices.Length == 0)
            {
                return;
            }
            unsafe
            {
                dxRenderer.SetVertexBuffer<CurveVertex>(vertexBuffer, sizeof(CurveVertex));
            }
            dxRenderer.SetIndexBuffer(indexBuffer);

            Matrix modelMatrix = Model * worldTransform;
            DxConstantBuffer cb = new DxConstantBuffer();
            cb.MVP = modelMatrix * dxRenderer.Scene.Camera.VP;
            cb.Color = modifier(isSelected ? Color : UnselectedColor);

            dxRenderer.UpdateConstantBuffer(ref dxRenderer.geometryConstantBuffer, cb);

            dxRenderer.device.ImmediateContext.Draw(g.Vertices.Length, 0);

            dxRenderer.device.ImmediateContext.InputAssembler.InputLayout = dxRenderer.inputLayout;
        }

        public override SerializationModel GetSerializationModel()
        {
            return new InterpolationBezierCurveC2SerializationModel(this);
        }
    }
}
