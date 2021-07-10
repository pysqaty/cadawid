using System;
using System.Collections.Generic;
using CADawid.DxModule;
using CADawid.Serialization.Model;
using CADawid.ViewModel;
using SharpDX;
using SharpDX.Direct3D;

namespace CADawid.Model
{
    public class PolygonalChain : Curve<Vertex, Index>
    {
        public PolygonalChain(Vector4 color, Vector4 unselectedColor, List<Point> nodes, bool isRemovable = true) : base(color, unselectedColor, nodes, isRemovable)
        {
        }

        public override GeometryViewModel GetViewModel()
        {
            throw new NotImplementedException();
        }
        public override IGeometryObject Copy()
        {
            throw new NotImplementedException();
        }

        protected override Geometry<Vertex, Index> GenerateGeometry()
        {
            Vertex[] vertexArray = new Vertex[Nodes.Count];
            Index[] edgeArray = new Index[(Nodes.Count - 1) * 2];
            for (int i = 0; i < Nodes.Count; i++)
            {
                vertexArray[i] = new Vertex(Nodes[i].TranslationV.X,
                    Nodes[i].TranslationV.Y, Nodes[i].TranslationV.Z);
            }
            ushort node = 0;
            for(int i = 0; i < edgeArray.Length; i+=2)
            {
                edgeArray[i] = new Index(node);
                edgeArray[i + 1] = new Index((UInt16)(node + 1));
                node++;
            }
            return new Geometry<Vertex, Index>(vertexArray, edgeArray);
        }

        public override void Render(DxRenderer dxRenderer, Matrix worldTransform, bool isSelected, Func<Vector4, Vector4> modifier)
        {
            dxRenderer.device.ImmediateContext.VertexShader.Set(dxRenderer.vertexShader);
            dxRenderer.device.ImmediateContext.PixelShader.Set(dxRenderer.pixelShader);
            dxRenderer.device.ImmediateContext.GeometryShader.Set(null);
            dxRenderer.device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
            base.Render(dxRenderer, worldTransform, isSelected, modifier);
        }

        public override SerializationModel GetSerializationModel()
        {
            throw new NotImplementedException();
        }
    }
}
