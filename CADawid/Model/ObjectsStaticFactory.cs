using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace CADawid.Model
{
    public static class ObjectsStaticFactory
    {
        public static IGeometryObject CreateObject(GeometryType type)
        {
            switch (type)
            {
                case GeometryType.Torus:
                    {
                        return new Torus(6f, 2f, 20, 10, SharpDX.Matrix.Identity,
                            new Vector4(1, 1, 0, 1), new Vector4(1, 1, 0, 0.3f));
                    }
                case GeometryType.Point:
                    {
                        return new Model.Point(0.5f, new Vector4(1, 0, 0, 1),
                            new Vector4(1, 0, 0, 0.3f));
                    }
                case GeometryType.BezierCurveC0:
                    {
                        return new Model.BezierCurveC0(new Vector4(1, 1, 1, 1),
                            new Vector4(0.4f, 0.4f, 0.4f, 1), new List<Model.Point>());
                    }
                case GeometryType.BezierCurveC2:
                    {
                        return new Model.BezierCurveC2(new Vector4(1, 1, 1, 1),
                            new Vector4(0.4f, 0.4f, 0.4f, 1), new List<Model.Point>());
                    }
                case GeometryType.InterpolationBezierCurveC2:
                    {
                        return new Model.InterpolationBezierCurveC2(new Vector4(1, 1, 1, 1),
                            new Vector4(0.4f, 0.4f, 0.4f, 1), new List<Model.Point>());
                    }
                case GeometryType.BicubicBezierPatchC0:
                    {
                        return new BicubicBezierPatchC0(1, 1, 20.0f, 20.0f, 10.0f, 20.0f, 4, 4, false, new Vector4(0, 1, 0, 1),
                            new Vector4(0, 0.5f, 0, 1));
                    }
                case GeometryType.BicubicBsplinePatchC2:
                    {
                        return new BicubicBsplinePatchC2(1, 1, 20.0f, 20.0f, 10.0f, 20.0f, 4, 4, false, new Vector4(0, 1, 0, 1),
                            new Vector4(0, 0.5f, 0, 1));
                    }
            }
            return null;
        }
    }
}
