using System;
using System.Collections;
using System.Collections.Generic;
using CADawid.DxModule;
using CADawid.Serialization.Model;
using CADawid.Utils;
using CADawid.ViewModel;
using SharpDX;
using SharpDX.Direct3D;

namespace CADawid.Model
{
    public class BezierCurveC2 : Curve<CurveVertex, Index>
    {
        public const float step = 1.0f / 3;
        public delegate void VirtualPointsChanged(IGeometryObject sender, List<Model.Point> virtualPoints);
        public event VirtualPointsChanged OnVirtualPointsChanged;

        private List<Point> BernsteinNodes { get; set; }
        private PolygonalChain bernsteinPolygon;
        private PolygonalChain bsplinePolygon;

        public bool DisplayBernsteinNodes { get; set; }
        public bool DisplayBernsteinPolygon { get; set; }
        public bool DisplayBsplinePolygon { get; set; }

        public float Precision { get; set; } = 50;
        public BezierCurveC2(Vector4 color, Vector4 unselectedColor, List<Point> nodes, bool isRemovable = true) : base(color, unselectedColor, nodes, isRemovable)
        {
            DisplayBsplinePolygon = false;
            DisplayBernsteinPolygon = false;
            DisplayBernsteinNodes = false;
            bsplinePolygon = new PolygonalChain(0.5f * color, 0.5f * unselectedColor, nodes);
        }

        public override bool AddNode(IGeometryObject node)
        {
            bsplinePolygon.AddNode(node);
            return base.AddNode(node);
        }
        public override void AddNodes(IList nodes)
        {
            bsplinePolygon.AddNodes(nodes);
            base.AddNodes(nodes);
        }
        public override void RemoveNodes(IGeometryObject[] removedGeometries)
        {
            bsplinePolygon.RemoveNodes(removedGeometries);
            base.RemoveNodes(removedGeometries);
        }
        public override void RemoveNodes(IList removedGeometries)
        {
            bsplinePolygon.RemoveNodes(removedGeometries);
            base.RemoveNodes(removedGeometries);
        }

        private void UpdateBernsteinNodes()
        {
            var bernsteinNodes = Algorithm.BsplineToBernstein(Nodes);
            if(BernsteinNodes != null && BernsteinNodes.Count == bernsteinNodes.Count)
            {
                for (int i = 0; i < bernsteinNodes.Count; i++)
                {
                    BernsteinNodes[i].TranslationV = bernsteinNodes[i].TranslationV;
                }
            }
            else
            {
                BernsteinNodes = bernsteinNodes;
                BernsteinNodes.ForEach(n => n.OnTransformed += UpdateNodes);
                OnVirtualPointsChanged?.Invoke(this, BernsteinNodes);
            }
            bernsteinPolygon = new PolygonalChain(0.5f * Color, 0.5f * UnselectedColor, BernsteinNodes);
        }

        protected override Geometry<CurveVertex, Index> GenerateGeometry()
        {
            UpdateBernsteinNodes();
            (int numberOfSegment, _) = IndexToSegmentElement(BernsteinNodes.Count - 1);
            CurveVertex[] vertexArr = new CurveVertex[numberOfSegment + 1];
            for (int i = 0; i <= numberOfSegment; i++)
            {
                int inx = SegmentElementToIndex(i, 0);
                vertexArr[i] = new CurveVertex(new Vector4(BernsteinNodes[inx].CurrentPosition, 1),
                    new Vector4(BernsteinNodes[inx + 1].CurrentPosition, 1),
                    new Vector4(BernsteinNodes[inx + 2].CurrentPosition, 1),
                    new Vector4(BernsteinNodes[inx + 3].CurrentPosition, 1));
            }
            Index[] indexArr = new Index[1];
            return new Geometry<CurveVertex, Index>(vertexArr, indexArr);
        }

        public override void Render(DxRenderer dxRenderer, Matrix worldTransform, bool isSelected, Func<Vector4, Vector4> modifier)
        {
            if(DisplayBernsteinNodes)
            {
                if (BernsteinNodes != null)
                {
                    Matrix faceIt = Matrix.RotationX(-dxRenderer.Scene.Camera.RotationV.X) * Matrix.RotationY(-dxRenderer.Scene.Camera.RotationV.Y) *
                        Matrix.RotationZ(-dxRenderer.Scene.Camera.RotationV.Z);
                    foreach (var node in BernsteinNodes)
                    {
                        node.Render(dxRenderer, faceIt * Matrix.Translation(node.TranslationV), node.IsSelected, modifier);
                    }
                }
            }
            if(DisplayBernsteinPolygon)
            {
                if (bernsteinPolygon != null)
                {
                    bernsteinPolygon.Render(dxRenderer, worldTransform, isSelected, modifier);
                }
            }
            if(DisplayBsplinePolygon)
            {
                if (bsplinePolygon != null)
                {
                    bsplinePolygon.Render(dxRenderer, worldTransform, isSelected, modifier);
                }
            }

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

        public override GeometryViewModel GetViewModel()
        {
            return new BezierCurveC2ViewModel()
            {
                SelectedObject = this
            };
        }
        public override IGeometryObject Copy()
        {
            IGeometryObject copied = new BezierCurveC2(Color, UnselectedColor, Nodes);
            return copied;
        }

        private void UpdateNodes(IGeometryObject transformedNode)
        {
            Point transformedPoint = transformedNode as Point;
            //update other bernstein nodes
            int index = BernsteinNodes.IndexOf(transformedPoint);
            (int segment, int number) = IndexToSegmentElement(index);
            switch(number)
            {
                case 1:
                    {
                        int knot = segment;
                        List<Point> second = Algorithm.EvaluateOnLine(transformedPoint, Nodes[knot], knot + step, knot, new List<float>() { knot + 2 * step, knot + 1 });
                        BernsteinNodes[index + 1].TranslationV = second[0].TranslationV;
                        Nodes[knot + 1].SetPosition(second[1].TranslationV);
                        if(knot != 0)
                        {
                            List<Point> first = Algorithm.EvaluateOnLine(transformedPoint, BernsteinNodes[index - 1], knot + step, knot, new List<float>() { knot - step });
                            BernsteinNodes[index - 2].TranslationV = first[0].TranslationV;
                            List<Point> third = Algorithm.EvaluateOnLine(Nodes[knot], BernsteinNodes[index - 2], knot, knot - step, new List<float>() { knot - 2 * step, knot - 1 });
                            BernsteinNodes[index - 3].TranslationV = third[0].TranslationV;
                            Nodes[knot - 1].SetPosition(third[1].TranslationV);
                        }
                        break;
                    }
                case 2:
                    {
                        int knot = segment + 1;
                        List<Point> second = Algorithm.EvaluateOnLine(transformedPoint, Nodes[knot], knot - step, knot, new List<float>() { knot - 2 * step, knot - 1 });
                        BernsteinNodes[index - 1].TranslationV = second[0].TranslationV;
                        Nodes[knot - 1].SetPosition(second[1].TranslationV);
                        if(knot != Nodes.Count - 1)
                        {
                            List<Point> first = Algorithm.EvaluateOnLine(transformedPoint, BernsteinNodes[index + 1], knot - step, knot, new List<float>() { knot + step });
                            BernsteinNodes[index + 2].TranslationV = first[0].TranslationV;
                            List<Point> third = Algorithm.EvaluateOnLine(Nodes[knot], BernsteinNodes[index + 2], knot, knot + step, new List<float>() { knot + 2 * step, knot + 1 });
                            BernsteinNodes[index + 3].TranslationV = third[0].TranslationV;
                            Nodes[knot + 1].SetPosition(third[1].TranslationV);
                        }
                        break;
                    }
                case 3:
                    {
                        int knot = segment + 1;
                        List<Point> first = Algorithm.EvaluateOnParallelLine(BernsteinNodes[index - 1], BernsteinNodes[index + 1], knot - step, knot + step,
                            transformedPoint, knot, new List<float>() { knot - step, knot + step });
                        BernsteinNodes[index - 1].TranslationV = first[0].TranslationV;
                        BernsteinNodes[index + 1].TranslationV = first[1].TranslationV;
                        List<Point> second = Algorithm.EvaluateOnLine(BernsteinNodes[index - 1], Nodes[knot], knot - step, knot, new List<float>() { knot - 2 * step, knot - 1 });
                        BernsteinNodes[index - 2].TranslationV = second[0].TranslationV;
                        Nodes[knot - 1].SetPosition(second[1].TranslationV);
                        List<Point> third = Algorithm.EvaluateOnLine(BernsteinNodes[index + 1], Nodes[knot], knot + step, knot, new List<float>() { knot + 2 * step, knot + 1 });
                        BernsteinNodes[index + 2].TranslationV = third[0].TranslationV;
                        Nodes[knot + 1].SetPosition(third[1].TranslationV);
                        break;
                    }
            }
        }

        private int SegmentElementToIndex(int segment, int number)
        {
            int index = segment * 3 + number;
            return index;
        }
        private (int segment, int number) IndexToSegmentElement(int index)
        {
            int segment = (int)Math.Ceiling((index / 3.0)) - 1;
            int number = index - segment * 3;
            return (segment, number);
        }

        public override SerializationModel GetSerializationModel()
        {
            return new BezierCurveC2SerializationModel(this);
        }
    }
}
