using SharpDX;

namespace CADawid.Utils
{
    public static class BasisConverter
    {
        public static Matrix PowerToBernsteinMatrix = new Matrix(new float[]
        {
            1,    1,    1,    1,
            0,    1/3f, 2/3f, 1,
            0,    0,    1/3f, 1,
            0,    0,    0,    1
        });

        public static Vector4 PowerToBernstein(Vector4 powerBasis)
        {
            return Vector4.Transform(powerBasis, PowerToBernsteinMatrix);
        }

        public static Vector4 PowerToBernstein(float a, float b, float c, float d) => PowerToBernstein(new Vector4(a, b, c, d));
    }
}
