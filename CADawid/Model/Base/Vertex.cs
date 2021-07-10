using SharpDX;

namespace CADawid.Model
{
    public struct Vertex
    {
        public Vector4 Position { get; set; }

        public Vertex(Vector4 position)
        {
            Position = position;
        }

        public Vertex(float x, float y, float z, float w = 1)
        {
            Position = new Vector4(x, y, z, w);
        }
    }
}
