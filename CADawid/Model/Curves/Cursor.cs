using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CADawid.DxModule;
using SharpDX;
using SharpDX.Direct3D;

namespace CADawid.Model
{
    public class Cursor : Point
    {
        public Cursor(float size, Vector4 color, Vector4 unselectedColor) : base(size, color, unselectedColor)
        {
        }

        public override void Render(DxRenderer dxRenderer, Matrix worldTransform, bool isSelected, Func<Vector4, Vector4> modifier)
        {
            dxRenderer.device.ImmediateContext.VertexShader.Set(dxRenderer.vertexShaderCursor);
            dxRenderer.device.ImmediateContext.PixelShader.Set(dxRenderer.pixelShaderCursor);
            dxRenderer.device.ImmediateContext.GeometryShader.Set(null);
            dxRenderer.device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            Geometry<Vertex, Index> g = TryUpdateGeometry(dxRenderer);
            if (g.Indices.Length == 0 || g.Vertices.Length == 0)
            {
                return;
            }
            unsafe
            {
                dxRenderer.SetVertexBuffer<Vertex>(vertexBuffer, sizeof(Vertex));
            }
            dxRenderer.SetIndexBuffer(indexBuffer);

            Matrix modelMatrix = Model * worldTransform;
            DxConstantBuffer cb = new DxConstantBuffer();
            cb.MVP = modelMatrix * dxRenderer.Scene.Camera.VP;
            cb.Color = modifier(isSelected ? Color : UnselectedColor);
            dxRenderer.UpdateConstantBuffer(ref dxRenderer.constantBuffer, cb);

            DxPointConstantBuffer pcb = new DxPointConstantBuffer();
            pcb.parameter = 0.06f;
            dxRenderer.UpdateConstantBuffer(ref dxRenderer.pointConstantBuffer, pcb);

            dxRenderer.device.ImmediateContext.DrawIndexed(g.Indices.Length, 0, 0);
        }
    }
}
