using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace CADawid.Model
{
    public struct CurveVertex
    {
        public Vector4 Position0 { get; set; }
        public Vector4 Position1 { get; set; }
        public Vector4 Position2 { get; set; }
        public Vector4 Position3 { get; set; }

        public CurveVertex(Vector4 position0, Vector4 position1, Vector4 position2, Vector4 position3)
        {
            Position0 = position0;
            Position1 = position1;
            Position2 = position2;
            Position3 = position3;
        }
    }
}
