using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace CADawid.Utils
{
    public static class Algorithm
    {
        public static T DeCasteljau<T>(IEnumerable<T> nodes, float t)
        {
            List<T> current = new List<T>(nodes);
            for(int j = 0; j < nodes.Count() - 1; j++)
            {

                List<T> next = new List<T>();
                for (int i = 0; i < current.Count - 1; i++)
                {
                    next.Add((1 - t) * (dynamic)current[i] + t * (dynamic)current[i + 1]);
                }
                current = next;
            }
            return current[0];
        }

        public static (IEnumerable<T> left, IEnumerable<T> right) DeCasteljauSubdivision<T>(IEnumerable<T> nodes, float t)
        {
            List<T> current = new List<T>(nodes);
            List<T> left = new List<T>();
            List<T> right = new List<T>();
            left.Add(current.First());
            right.Add(current.Last());
            for (int j = 0; j < nodes.Count() - 1; j++)
            {
                List<T> next = new List<T>();
                for (int i = 0; i < current.Count - 1; i++)
                {
                    next.Add((1 - t) * (dynamic)current[i] + t * (dynamic)current[i + 1]);
                }
                current = next;
                left.Add(current.First());
                right.Add(current.Last());
            }
            right.Reverse();
            return (left, right);
        }

        public static float PolygonArea(List<Vector3> polygon)
        {
            polygon.Add(polygon[0]);

            float area = 0;
            for (int i = 0; i < polygon.Count - 1; i++)
            {
                area +=
                    (polygon[i + 1].X - polygon[i].X) *
                    (polygon[i + 1].Y + polygon[i].Y) / 2;
            }
            return Math.Abs(area);
        }

        public static float VectorLegth(Vector3 from, Vector3 to)
        {
            return (to - from).Length();
        }

        public static List<Model.Point> BsplineToBernstein(List<Model.Point> bsplineNodes)
        {
            if(bsplineNodes.Count == 1)
            {
                return EvaluateOnLine(bsplineNodes[0], bsplineNodes[0], 0, 1, new List<float>() { 0.0f, 1.0f / 3, 2.0f / 3, 1.0f });
            }
            if (bsplineNodes.Count == 2)
            {
                return EvaluateOnLine(bsplineNodes[0], bsplineNodes[1], 0, 1, new List<float>() { 0.0f, 1.0f / 3, 2.0f / 3, 1.0f });
            }
            List<Model.Point> fullResult = new List<Model.Point>();
            List<Model.Point> internalNodes = new List<Model.Point>();
            for(int i = 0; i < bsplineNodes.Count - 1; i++)
            {
                internalNodes.AddRange(EvaluateOnLine(bsplineNodes[i], bsplineNodes[i + 1], 
                    i, i+1, new List<float>() { i + 1.0f / 3, i + 2.0f / 3 }));
            }

            int knot = 1;
            List<Model.Point> knotsNodes = new List<Model.Point>();
            for(int i = 1; i < internalNodes.Count - 1; i+=2)
            {
                knotsNodes.AddRange(EvaluateOnLine(internalNodes[i], internalNodes[i + 1],
                    knot - 1.0f / 3, knot + 1.0f / 3, new List<float>() { knot }));
                knot++;
            }

            Model.Point firstNode = new Model.Point(0.3f,
                    new Vector4(0, 1, 0, 1), new Vector4(0, 0.5f, 0, 1));
            firstNode.TranslationV = bsplineNodes[0].TranslationV;

            Model.Point lastNode = new Model.Point(0.3f,
                    new Vector4(0, 1, 0, 1), new Vector4(0, 0.5f, 0, 1));
            lastNode.TranslationV = bsplineNodes.Last().TranslationV;

            knotsNodes.Add(lastNode);

            fullResult.Add(firstNode);

            for(int i = 0; i < knotsNodes.Count; i++)
            {
                fullResult.Add(internalNodes[2 * i]);
                fullResult.Add(internalNodes[2 * i + 1]);
                fullResult.Add(knotsNodes[i]);
            }
            return fullResult;
        }

        public static List<Model.Point> EvaluateOnLine(Model.Point from, Model.Point to, float iFrom, float iTo, List<float> t)
        {
            (float ax, float bx) = GetLineFrom2Points(from.TranslationV.X, iFrom, to.TranslationV.X, iTo);
            (float ay, float by) = GetLineFrom2Points(from.TranslationV.Y, iFrom, to.TranslationV.Y, iTo);
            (float az, float bz) = GetLineFrom2Points(from.TranslationV.Z, iFrom, to.TranslationV.Z, iTo);

            List<Model.Point> results = new List<Model.Point>();
            for (int j = 0; j < t.Count; j++)
            {
                Model.Point controlPoint = new Model.Point(0.3f,
                    new Vector4(0, 1, 0, 1), new Vector4(0, 0.5f, 0, 1));
                controlPoint.TranslationV = new MyVector3(
                    ax * t[j] + bx,
                    ay * t[j] + by,
                    az * t[j] + bz);
                results.Add(controlPoint);
            }
            return results;
        }

        private static (float a, float b) GetLineFrom2Points(float valueFrom, float argFrom, float valueTo, float argTo)
        {
            float a = (valueTo - valueFrom) / (argTo - argFrom);
            float b = valueFrom - argFrom * a;
            return (a, b);
        }

        private static (float a, float b) GetParallelLineAtPoint((float a, float b) line, float val, float arg)
        {
            float parallelA = line.a;
            float parallelB = val - arg * parallelA;
            return (parallelA, parallelB);
        }

        public static List<Model.Point> EvaluateOnParallelLine(Model.Point parallelPoint1, Model.Point parallelPoint2, float iFrom, float iTo, 
            Model.Point atPoint, float atI, List<float> t)
        {
            (float ax, float bx) linex = GetLineFrom2Points(parallelPoint1.TranslationV.X, iFrom, parallelPoint2.TranslationV.X, iTo);
            (float ay, float by) liney = GetLineFrom2Points(parallelPoint1.TranslationV.Y, iFrom, parallelPoint2.TranslationV.Y, iTo);
            (float az, float bz) linez = GetLineFrom2Points(parallelPoint1.TranslationV.Z, iFrom, parallelPoint2.TranslationV.Z, iTo);

            (float pax, float pbx) = GetParallelLineAtPoint(linex, atPoint.TranslationV.X, atI);
            (float pay, float pby) = GetParallelLineAtPoint(liney, atPoint.TranslationV.Y, atI);
            (float paz, float pbz) = GetParallelLineAtPoint(linez, atPoint.TranslationV.Z, atI);

            List<Model.Point> results = new List<Model.Point>();
            for (int j = 0; j < t.Count; j++)
            {
                Model.Point controlPoint = new Model.Point(0.3f,
                    new Vector4(0, 1, 0, 1), new Vector4(0, 0.5f, 0, 1));
                controlPoint.TranslationV = new MyVector3(
                    pax * t[j] + pbx,
                    pay * t[j] + pby,
                    paz * t[j] + pbz);
                results.Add(controlPoint);
            }
            return results;
        }

        public static Vector3[] ThomasAlgorithm(float[] a, float[] b, float[] c, Vector3[] d)
        {
            if(d.Length == 1)
            {
                d[0] /= b[0];
                return d;
            }
            int n = d.Length - 1;
            c[0] /= b[0];
            d[0] /= b[0];
            for(int i = 1; i < n; i++)
            {
                c[i] /= (b[i] - a[i] * c[i - 1]);
                d[i] = (d[i] - a[i] * d[i - 1]) / (b[i] - a[i] * c[i - 1]);
            }

            d[n] = (d[n] - a[n] * d[n - 1]) / (b[n] - a[n] * c[n - 1]);

            for(int i = n - 1; i > 0; i--)
            {
                d[i] -= c[i] * d[i + 1];
            }
            return d;
        }
    }
}
