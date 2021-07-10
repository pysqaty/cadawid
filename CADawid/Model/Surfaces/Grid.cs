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
    public class Grid : BicubicPatch<Vertex, Index>
    {
        public bool ConnectEndEdges { get; set; }
        public Grid(Vector4 color, Vector4 unselectedColor, Point[,] nodes, bool isRemovable = true, bool connectEndEdges = false)
            : base(color, unselectedColor, isRemovable)
        {
            Color = color;
            UnselectedColor = unselectedColor;
            IsRemovable = isRemovable;
            ConnectEndEdges = connectEndEdges;
            Nodes = new Point[nodes.GetLength(0), nodes.GetLength(1)];
            for(int i = 0; i < nodes.GetLength(0); i++)
            {
                for(int j = 0; j < nodes.GetLength(1); j++)
                {
                    nodes[i, j].OnTransformed += UpdateGeometry;
                    Nodes[i, j] = nodes[i, j];
                }
            }
            Nodes = nodes;
        }

        public override IGeometryObject Copy()
        {
            throw new NotImplementedException();
        }

        public override GeometryViewModel GetViewModel()
        {
            throw new NotImplementedException();
        }

        protected override Geometry<Vertex, Index> GenerateGeometry()
        {
            Vertex[] vertices = new Vertex[Nodes.GetLength(0) * Nodes.GetLength(1)];
            int k = 0;
            for (int i = 0; i < Nodes.GetLength(0); i++)
            {
                for (int j = 0; j < Nodes.GetLength(1); j++)
                {
                    vertices[k++] = new Vertex(new Vector4(Nodes[i, j].CurrentPosition, 1.0f));
                }
            }

            List<Index> indexList = new List<Index>();

            for (int i = 0; i < Nodes.GetLength(0) - 1; i++)
            {
                for (int j = 0; j < Nodes.GetLength(1) - 1; j++)
                {
                    indexList.Add(new Index((ushort)(i * Nodes.GetLength(1) + j)));
                    indexList.Add(new Index((ushort)(i * Nodes.GetLength(1) + j + 1)));

                    indexList.Add(new Index((ushort)(i * Nodes.GetLength(1) + j)));
                    indexList.Add(new Index((ushort)((i + 1) * Nodes.GetLength(1) + j)));
                }
            }

            for (int i = 0; i < Nodes.GetLength(0) - 1; i++)
            {
                indexList.Add(new Index((ushort)(i * Nodes.GetLength(1) + Nodes.GetLength(1) - 1)));
                indexList.Add(new Index((ushort)((i + 1) * Nodes.GetLength(1) + Nodes.GetLength(1) - 1)));
            }

            for (int j = 0; j < Nodes.GetLength(1) - 1; j++)
            {
                indexList.Add(new Index((ushort)((Nodes.GetLength(0) - 1) * Nodes.GetLength(1) + j)));
                indexList.Add(new Index((ushort)((Nodes.GetLength(0) - 1) * Nodes.GetLength(1) + j + 1)));
            }

            if(ConnectEndEdges)
            {
                for (int i = 0; i < Nodes.GetLength(0); i++)
                {
                    indexList.Add(new Index((ushort)(i * Nodes.GetLength(1) + Nodes.GetLength(1) - 1)));
                    indexList.Add(new Index((ushort)(i * Nodes.GetLength(1))));
                }
            }

            return new Geometry<Vertex, Index>(vertices, indexList.ToArray());
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

        public override void SetNodes(Point[,] nodes)
        {
            throw new NotImplementedException();
        }
    }
}
