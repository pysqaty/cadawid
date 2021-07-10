using System;
using CADawid.DxModule;
using CADawid.Serialization.Model;
using CADawid.ViewModel;
using SharpDX;
using SharpDX.Direct3D;

namespace CADawid.Model
{
    public class Segment : GeometryObject<Vertex, Index>
    {
        public Vertex from { get; set; }
        public Vertex to { get; set; }
        public Segment(Vertex from, Vertex to, Matrix model, Vector4 color, Vector4 unselectedColor)
        {
            this.from = from;
            this.to = to;
            Model = model;
            Color = color;
            UnselectedColor = unselectedColor;
        }

        protected override Geometry<Vertex, Index> GenerateGeometry()
        {
            Vertex[] vertexArray = new Vertex[] { from, to };
            Index[] edgeArray = new Index[] { new Index(0), new Index(1) };
            return new Geometry<Vertex, Index>(vertexArray, edgeArray);
        }

        public override GeometryViewModel GetViewModel()
        {
            throw new NotImplementedException();
        }
        public override IGeometryObject Copy()
        {
            throw new NotImplementedException();
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
