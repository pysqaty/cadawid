using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CADawid.Model
{
    public struct Geometry<V, I>
        where V : struct
        where I : struct
    { 
        public V[] Vertices { get; set; }
        public I[] Indices { get; set; }

        public Geometry(V[] vertices, I[] indices) : this()
        {
            Vertices = vertices;
            Indices = indices;
        }
    }
}
