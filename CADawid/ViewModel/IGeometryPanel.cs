using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CADawid.ViewModel
{
    public interface IGeometryPanel
    {
        void Accept(TorusViewModel torus);
        void Accept(PointViewModel point);
        void Accept(BezierCurveC0ViewModel bezier);
        void Accept(BezierCurveC2ViewModel bezierCurveC2);
        void Accept(InterpolationBezierCurveC2ViewModel interpolationBezierCurveC2);
        void Accept(PatchViewModel bicubicBezierPatchC0);
        void Accept(GregoryPatchViewModel gregoryPatch);
    }
}
