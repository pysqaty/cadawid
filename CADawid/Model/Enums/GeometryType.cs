using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CADawid.Model
{
    public enum GeometryType
    {
        Torus,
        Point,
        BezierCurveC0,
        BezierCurveC2,
        InterpolationBezierCurveC2,
        BicubicBezierPatchC0,
        BicubicBsplinePatchC2
    }
}
