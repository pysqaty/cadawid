using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace CADawid.Model
{
    public struct PatchVertex
    {
        Vector4 p00;
        Vector4 p10;
        Vector4 p20;
        Vector4 p30;
        Vector4 p01;
        Vector4 p11;
        Vector4 p21;
        Vector4 p31;
        Vector4 p02;
        Vector4 p12;
        Vector4 p22;
        Vector4 p32;
        Vector4 p03;
        Vector4 p13;
        Vector4 p23;
        Vector4 p33;

        public PatchVertex(Vector3[,] nodes)
        {
            p00 = new Vector4(nodes[0, 0], 1);
            p10 = new Vector4(nodes[1, 0], 1);
            p20 = new Vector4(nodes[2, 0], 1);
            p30 = new Vector4(nodes[3, 0], 1);
            p01 = new Vector4(nodes[0, 1], 1);
            p11 = new Vector4(nodes[1, 1], 1);
            p21 = new Vector4(nodes[2, 1], 1);
            p31 = new Vector4(nodes[3, 1], 1);
            p02 = new Vector4(nodes[0, 2], 1);
            p12 = new Vector4(nodes[1, 2], 1);
            p22 = new Vector4(nodes[2, 2], 1);
            p32 = new Vector4(nodes[3, 2], 1);
            p03 = new Vector4(nodes[0, 3], 1);
            p13 = new Vector4(nodes[1, 3], 1);
            p23 = new Vector4(nodes[2, 3], 1);
            p33 = new Vector4(nodes[3, 3], 1);
        }

        public PatchVertex(Vector4[,] nodes)
        {
            p00 = nodes[0, 0];
            p10 = nodes[1, 0];
            p20 = nodes[2, 0];
            p30 = nodes[3, 0];
            p01 = nodes[0, 1];
            p11 = nodes[1, 1];
            p21 = nodes[2, 1];
            p31 = nodes[3, 1];
            p02 = nodes[0, 2];
            p12 = nodes[1, 2];
            p22 = nodes[2, 2];
            p32 = nodes[3, 2];
            p03 = nodes[0, 3];
            p13 = nodes[1, 3];
            p23 = nodes[2, 3];
            p33 = nodes[3, 3];
        }
    }
}
