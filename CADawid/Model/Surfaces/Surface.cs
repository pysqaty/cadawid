using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace CADawid.Model
{
    public abstract class Surface<V, I> : GeometryObject<V, I>
        where V : unmanaged
        where I : unmanaged
    {
        public int PrecisionU { get; set; }
        public int PrecisionV { get; set; }

       
        public override Matrix CurrentModel => Matrix.Identity;

        public void UpdateGeometry(IGeometryObject geometryObject)
        {
            ResetGeometry();
        }

        public Surface(Vector4 color, Vector4 unselectedColor, bool isRemovable = true)
        {
            Color = color;
            UnselectedColor = unselectedColor;
            IsRemovable = isRemovable;

        }



        
    }
}
