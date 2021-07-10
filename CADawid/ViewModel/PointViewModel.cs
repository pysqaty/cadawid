using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CADawid.Model;

namespace CADawid.ViewModel
{
    public class PointViewModel : GeometryViewModel
    {
        private Point torus => SelectedObject as Point;

        public override void Visit(IGeometryPanel visitor)
        {
            visitor.Accept(this);
        }

        public override void VisitToAdd(IGeometryAddPanel visitor)
        {
            visitor.AcceptToAdd(this);
        }
    }
}
