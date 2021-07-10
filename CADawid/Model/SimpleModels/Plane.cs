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
    public class Plane : GeometryObject<Vertex, Index>
    {
        public float Width { get; set; }
        public float Height { get; set; }
        public int Precision1 { get; set; }
        public int Precision2 { get; set; }
        public Plane(float width, float height, int precision1, int precision2, Matrix model, Vector4 color, Vector4 unselectedColor)
        {
            Width = width;
            Height = height;
            Precision1 = precision1;
            Precision2 = precision2;
            Model = model;
            Color = color;
            UnselectedColor = unselectedColor;
        }
        protected override Geometry<Vertex, Index> GenerateGeometry()
        {
            int k = 0;
            int l = 0;
            Vertex[] vertexArray = new Vertex[(Precision1 + 1) * (Precision2 + 1)];
            Index[] edgeArray = new Index[vertexArray.Length * 2 * 2];
            int[,] grid = new int[(Precision2 + 1), (Precision1 + 1)];
            float step1 = Height / Precision1;
            float step2 = Width / Precision2;
            float iH = 0f;
            float iW = 0f;
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    Vertex v = new Vertex(new SharpDX.Vector4(iW - Width / 2, 0, iH - Height / 2, 1));
                    vertexArray[k] = v;
                    grid[i, j] = k;
                    iW += step1;
                    k++;
                }
                iH += step2;
                iW = 0f;
            }

            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    if(i + 1 < grid.GetLength(0))
                    {
                        edgeArray[l] = new Index((UInt16)grid[i, j]);
                        edgeArray[l + 1] = new Index((UInt16)grid[((i + 1)), j]);
                        l+=2;
                    }
                    if(j + 1 < grid.GetLength(1))
                    {
                        edgeArray[l] = new Index((UInt16)grid[i, j]);
                        edgeArray[l + 1] = new Index((UInt16)grid[i, ((j + 1))]);
                        l+=2;
                    }
                }
            }
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
