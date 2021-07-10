using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CADawid.Model;

namespace CADawid.ViewModel
{
    public class InterpolationBezierCurveC2ViewModel : GeometryViewModel
    {
        public InterpolationBezierCurveC2 InterpolationBezierCurveC2 => SelectedObject as InterpolationBezierCurveC2;
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
