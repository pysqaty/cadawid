using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CADawid.ViewModel
{
    public interface IGeometryAddPanel
    {
        void AcceptToAdd(TorusViewModel torus);
        void AcceptToAdd(PointViewModel point);
        void AcceptToAdd(BezierCurveC0ViewModel bezier);
        void AcceptToAdd(BezierCurveC2ViewModel bezierCurveC2);
        void AcceptToAdd(InterpolationBezierCurveC2ViewModel interpolationBezierCurveC2);
        void AcceptToAdd(PatchViewModel bicubicBezierPatchC0);
    }
}
