using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace CADawid.Utils
{
    public static class MathExt
    {
        public static float ToRadians(float val)
        {
            return (float)(Math.PI / 180) * val;
        }

        public static Vector3 RotateVector(Vector4 vector, Matrix rotation)
        {
            Matrix result = Matrix.Zero;
            result.Column4 = vector;
            result = rotation * result;
            return new Vector3(result.M14, result.M24, result.M34);
        }

        public static Vector3 ScaleVector(Vector4 vector, Matrix scale)
        {
            Matrix result = Matrix.Zero;
            result.Column4 = vector;
            result = scale * result;
            return new Vector3(result.M14, result.M24, result.M34);
        }
    }
}
