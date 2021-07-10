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
    public class Torus : GeometryObject<Vertex, Index>
    {
        public float R { get; set; }
        public float r { get; set; }

        public int Precision1 { get; set; }
        public int Precision2 { get; set; }

        public Torus(float R, float r, int precision1, int precision2, Matrix model, Vector4 color, Vector4 unselectedColor, bool isRemovable = true)
        {
            this.R = R;
            this.r = r;
            Precision1 = precision1;
            Precision2 = precision2;
            Model = model;
            Color = color;
            UnselectedColor = unselectedColor;
            IsRemovable = isRemovable;
        }

        protected override Geometry<Vertex, Index> GenerateGeometry()
        {
            int k = 0;
            int l = 0;
            Vertex[] vertexArray = new Vertex[Precision1 * Precision2];
            Index[] edgeArray = new Index[vertexArray.Length * 2 * 2];
            int[,] grid = new int[Precision2, Precision1];
            float step1 = 360f / Precision1;
            float step2 = 360f / Precision2;
            float iR = 0f;
            float ir = 0f;
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    Vertex v = Formula(MathExt.ToRadians(iR), MathExt.ToRadians(ir));
                    vertexArray[k] = v;
                    grid[i, j] = k;
                    ir += step1;
                    k++;
                }
                iR += step2;
                ir = 0f;
            }

            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    edgeArray[l] = new Index((UInt16)grid[i, j]);
                    edgeArray[l + 1] = new Index((UInt16)grid[((i + 1) % grid.GetLength(0)), j]);
                    edgeArray[l + 2] = new Index((UInt16)grid[i, j]);
                    edgeArray[l + 3] = new Index((UInt16)grid[i, ((j + 1) % grid.GetLength(1))]);
                    l += 4;
                }
            }
            return new Geometry<Vertex, Index>(vertexArray, edgeArray);
        }
        private Vertex Formula(float alfa, float beta)
        {
            float x = (float)((R + r * Math.Cos(alfa)) * Math.Cos(beta));
            float y = (float)(-r * Math.Sin(alfa));
            float z = (float)((R + r * Math.Cos(alfa)) * Math.Sin(beta));
            return new Vertex(new Vector4(x, y, z, 1f));
        }

        public override GeometryViewModel GetViewModel()
        {
            return new TorusViewModel()
            {
                SelectedObject = this
            };
        }
        public override IGeometryObject Copy()
        {
            IGeometryObject copied = new Torus(R, r, Precision1, Precision2, Model, Color, UnselectedColor);
            return copied;
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
            return new TorusSerializationModel(this);
        }
    }
}
