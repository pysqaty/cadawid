using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CADawid.DxModule;
using CADawid.Serialization.Model;
using CADawid.ViewModel;
using SharpDX;
using SharpDX.Direct3D;

namespace CADawid.Model
{
    public class Point : GeometryObject<Vertex, Index>
    {
        public float Size { get; set; }

        public List<IPointBased> PointBased { get; set; }

        public Point(float size, Vector4 color, Vector4 unselectedColor, bool isRemovable = true)
        {
            PointBased = new List<IPointBased>();
            Size = size;
            Model = Matrix.Identity;
            Color = color;
            UnselectedColor = unselectedColor;
            IsRemovable = isRemovable;
        }

        public override GeometryViewModel GetViewModel()
        {
            return new PointViewModel()
            {
                SelectedObject = this
            };
        }
        public override IGeometryObject Copy()
        {
            IGeometryObject copied = new Point(Size, Color, UnselectedColor);
            return copied;
        }
        protected override Geometry<Vertex, Index> GenerateGeometry()
        {
            Vertex[] vertexArray = new Vertex[4];
            vertexArray[0] = new Vertex(-Size / 2f, -Size / 2f, 0f, 1f);
            vertexArray[1] = new Vertex(-Size / 2f, Size / 2f, 0f, 1f);
            vertexArray[2] = new Vertex(Size / 2f, Size / 2f, 0f, 1f);
            vertexArray[3] = new Vertex(Size / 2f, -Size / 2f, 0f, 1f);

            Index[] edgeArray = new Index[6];
            edgeArray[0] = new Index(0);
            edgeArray[1] = new Index(1);
            edgeArray[2] = new Index(2);
            edgeArray[3] = new Index(0);
            edgeArray[4] = new Index(2);
            edgeArray[5] = new Index(3);

            return new Geometry<Vertex, Index>(vertexArray, edgeArray);
        }
        public override void Render(DxRenderer dxRenderer, Matrix worldTransform, bool isSelected, Func<Vector4, Vector4> modifier)
        {
            dxRenderer.device.ImmediateContext.VertexShader.Set(dxRenderer.vertexShaderPoint);
            dxRenderer.device.ImmediateContext.PixelShader.Set(dxRenderer.pixelShaderPoint);
            dxRenderer.device.ImmediateContext.GeometryShader.Set(null);
            dxRenderer.device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            DxPointConstantBuffer pcb = new DxPointConstantBuffer();
            pcb.parameter = Size / 2.0f;
            dxRenderer.UpdateConstantBuffer(ref dxRenderer.pointConstantBuffer, pcb);
            base.Render(dxRenderer, worldTransform, isSelected, modifier);
        }

        public void SetPosition(Utils.MyVector3 position)
        {
            TranslationV = position;
            NotifyTransformed();
        }

        public override SerializationModel GetSerializationModel()
        {
            return new PointSerializationModel(this);
        }

        public void AddToObject(IPointBased pointBased)
        {
            if(!PointBased.Contains(pointBased))
            {
                PointBased.Add(pointBased);
            }
            
        }

        public void RemoveFromObject(IPointBased pointBased)
        {
            PointBased.Remove(pointBased);
            if(!PointBased.Any(p => p is BicubicPatch<PatchVertex, Index>))
            {
                IsRemovable = true;
            }
        }
    }
}
