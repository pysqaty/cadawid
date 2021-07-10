using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CADawid.DxModule;
using CADawid.Serialization.Model;
using CADawid.Utils;
using CADawid.ViewModel;
using SharpDX;
using SharpDX.Direct3D;

namespace CADawid.Model
{
    public class GregoryPatch : Surface<PatchVertex, Index>, IPointBased
    {
        List<Segment> Vectors { get; set; }
        public bool DisplayVectors { get; set; }
        List<Point> DependencyPoints { get; set; }
        List<Point> DependencyNextPoints { get; set; }
        public GregoryPatch(List<Point> dependency, List<Point> dependencyNext, Vector4 color, Vector4 unselectedColor, bool isRemovable = true)
            : base(color, unselectedColor, isRemovable)
        {
            DisplayVectors = false;
            PrecisionU = 8;
            PrecisionV = 8;
            foreach(Point p in dependency)
            {
                p.OnTransformed += UpdateGeometry;
                p.AddToObject(this);
            }
            foreach (Point p in dependencyNext)
            {
                p.OnTransformed += UpdateGeometry;
                p.AddToObject(this);
            }

            DependencyPoints = dependency;
            DependencyNextPoints = dependencyNext;

        }
        public override IGeometryObject Copy()
        {
            throw new NotImplementedException();
        }

        public override SerializationModel GetSerializationModel()
        {
            throw new NotImplementedException();
        }

        public override GeometryViewModel GetViewModel()
        {
            return new GregoryPatchViewModel()
            {
                SelectedObject = this
            };
        }

        protected override Geometry<PatchVertex, Index> GenerateGeometry()
        {
            (Vector3[,] CalculatedNodes, Vector3[,] InternalDoubles) = CalculatePatchPoints();
            List<PatchVertex> vertexList = new List<PatchVertex>();
            Vector4[,] nodes = new Vector4[4, 4];
            List<float> tmp = new List<float>();
            for (int i = 0; i < InternalDoubles.GetLength(0); i++)
            {
                for (int j = 0; j < InternalDoubles.GetLength(1); j++)
                {
                    tmp.Add(InternalDoubles[i, j].X);
                    tmp.Add(InternalDoubles[i, j].Y);
                    tmp.Add(InternalDoubles[i, j].Z);
                    tmp.Add(1.0f);
                }
            }
            int ind = 0;
            for (int i = 0; i < nodes.GetLength(0); i++)
            {
                for (int j = 0; j < nodes.GetLength(1); j++)
                {
                    nodes[i, j] = new Vector4(CalculatedNodes[i, j].X,
                        CalculatedNodes[i, j].Y, CalculatedNodes[i, j].Z, tmp[ind++]);
                }
            }
            vertexList.Add(new PatchVertex(nodes));
            Index[] indexArr = new Index[1];
            return new Geometry<PatchVertex, Index>(vertexList.ToArray(), indexArr);
        }

        private (Vector3[,] CalculatedNodes, Vector3[,] InternalDoubles) CalculatePatchPoints()
        {
            Vector3[] p1Row = DependencyPoints.Take(4).Select(p => p.CurrentPosition).ToArray();
            Vector3[] p2Row = DependencyPoints.Skip(3).Take(4).Select(p => p.CurrentPosition).ToArray();
            Vector3[] p3Row = DependencyPoints.Skip(6).Take(4).Select(p => p.CurrentPosition).ToArray();
            (IEnumerable<Vector3> g1a, IEnumerable<Vector3> g1b) = Algorithm.DeCasteljauSubdivision(p1Row, 0.5f);
            (IEnumerable<Vector3> g2a, IEnumerable<Vector3> g2b) = Algorithm.DeCasteljauSubdivision(p2Row, 0.5f);
            (IEnumerable<Vector3> g3a, IEnumerable<Vector3> g3b) = Algorithm.DeCasteljauSubdivision(p3Row, 0.5f);
            Vector3[] p1NextRow = DependencyNextPoints.Take(4).Select(p => p.CurrentPosition).ToArray();
            Vector3[] p2NextRow = DependencyNextPoints.Skip(4).Take(4).Select(p => p.CurrentPosition).ToArray();
            Vector3[] p3NextRow = DependencyNextPoints.Skip(8).Take(4).Select(p => p.CurrentPosition).ToArray();
            (IEnumerable<Vector3> g1aNext, IEnumerable<Vector3> g1bNext) = Algorithm.DeCasteljauSubdivision(p1NextRow, 0.5f);
            (IEnumerable<Vector3> g2aNext, IEnumerable<Vector3> g2bNext) = Algorithm.DeCasteljauSubdivision(p2NextRow, 0.5f);
            (IEnumerable<Vector3> g3aNext, IEnumerable<Vector3> g3bNext) = Algorithm.DeCasteljauSubdivision(p3NextRow, 0.5f);

            List<Vector3[]> gregoryA = new List<Vector3[]>();
            gregoryA.Add(g1a.ToArray());
            gregoryA.Add(g2a.ToArray());
            gregoryA.Add(g3a.ToArray());

            List<Vector3[]> gregoryB = new List<Vector3[]>();
            gregoryB.Add(g1b.ToArray());
            gregoryB.Add(g2b.ToArray());
            gregoryB.Add(g3b.ToArray());

            List<Vector3[]> gregoryANext = new List<Vector3[]>();
            gregoryANext.Add(g1aNext.ToArray());
            gregoryANext.Add(g2aNext.ToArray());
            gregoryANext.Add(g3aNext.ToArray());

            List<Vector3[]> gregoryBNext = new List<Vector3[]>();
            gregoryBNext.Add(g1bNext.ToArray());
            gregoryBNext.Add(g2bNext.ToArray());
            gregoryBNext.Add(g3bNext.ToArray());

            Vector3[] P3 = new Vector3[3];
            for (int i = 0; i < 3; i++)
            {
                P3[i] = gregoryA[i][3];
            }

            Vector3[] P2 = new Vector3[3];
            for (int i = 0; i < 3; i++)
            {
                P2[i] = 2 * P3[i] - gregoryANext[i][3];
            }

            Vector3[] Q = new Vector3[3];
            for (int i = 0; i < 3; i++)
            {
                Q[i] = (3 * P2[i] - P3[i]) / 2.0f;
            }

            Vector3 P0 = (Q[0] + Q[1] + Q[2]) / 3.0f;

            Vector3[] P1 = new Vector3[3];
            for (int i = 0; i < 3; i++)
            {
                P1[i] = (2 * Q[i] + P0) / 3.0f;
            }

            Vector3[] internalP = new Vector3[12];
            int ind = 0;
            for (int i = 0; i < 3; i++)
            {
                internalP[ind++] = gregoryA[i][1] * 2 - gregoryANext[i][1];
                internalP[ind++] = gregoryA[i][2] * 2 - gregoryANext[i][2];
                internalP[ind++] = gregoryB[i][1] * 2 - gregoryBNext[i][1];
                internalP[ind++] = gregoryB[i][2] * 2 - gregoryBNext[i][2];
            }

            Vector3[] ininP = new Vector3[6];

            Vector3 a0 = P0 - P1[2];
            Vector3 b0 = P1[1] - P0;
            Vector3 a3 = P2[0] - internalP[1];
            Vector3 b3 = internalP[2] - P2[0];

            Vector3 g0 = (a0 + b0) / 2;
            Vector3 g2 = (a3 + b3) / 2;
            Vector3 g1 = (g0 + g2) / 2;

            ininP[0] = P1[0] + g1;
            ininP[1] = P1[0] - g1;

            a0 = P0 - P1[2];
            b0 = P1[0] - P0;
            a3 = P2[1] - internalP[6];
            b3 = internalP[5] - P2[1];

            g0 = (a0 + b0) / 2;
            g2 = (a3 + b3) / 2;
            g1 = (g0 + g2) / 2;

            ininP[2] = P1[1] + g1;
            ininP[3] = P1[1] - g1;

            a0 = P0 - P1[1];
            b0 = P1[0] - P0;
            a3 = P2[2] - internalP[9];
            b3 = internalP[10] - P2[2];

            g0 = (a0 + b0) / 2;
            g2 = (a3 + b3) / 2;
            g1 = (g0 + g2) / 2;

            ininP[4] = P1[2] + g1;
            ininP[5] = P1[2] - g1;

            Vector3[,] nodes = new Vector3[4, 4]
            {
                { gregoryB[0][0], gregoryB[0][1], gregoryB[0][2], gregoryB[0][3]},
                { P2[0], internalP[2], internalP[3], gregoryA[1][1]},
                { P1[0], ininP[2], internalP[5], gregoryA[1][2]},
                { P0, P1[1], P2[1], P3[1]},
            };

            Vector3[,] inNodes = new Vector3[2, 2]
            {
                { internalP[2], internalP[4] },
                { ininP[0], internalP[5] }
            };

            Vectors = new List<Segment>();

            Vectors.Add(new Segment(new Vertex(new Vector4(nodes[0, 1], 1)),
                    new Vertex(new Vector4(nodes[1, 1], 1)), Matrix.Identity,
                    new Vector4(1, 1, 1, 1), new Vector4(1, 1, 1, 1)));
            Vectors.Add(new Segment(new Vertex(new Vector4(nodes[0, 2], 1)),
                    new Vertex(new Vector4(nodes[1, 2], 1)), Matrix.Identity,
                    new Vector4(1, 1, 1, 1), new Vector4(1, 1, 1, 1)));
            Vectors.Add(new Segment(new Vertex(new Vector4(nodes[3, 1], 1)),
                    new Vertex(new Vector4(nodes[2, 1], 1)), Matrix.Identity,
                    new Vector4(1, 1, 1, 1), new Vector4(1, 1, 1, 1)));
            Vectors.Add(new Segment(new Vertex(new Vector4(nodes[3, 2], 1)),
                    new Vertex(new Vector4(nodes[2, 2], 1)), Matrix.Identity,
                    new Vector4(1, 1, 1, 1), new Vector4(1, 1, 1, 1)));

            Vectors.Add(new Segment(new Vertex(new Vector4(nodes[1, 0], 1)),
                    new Vertex(new Vector4(inNodes[0, 0], 1)), Matrix.Identity,
                    new Vector4(1, 1, 1, 1), new Vector4(1, 1, 1, 1)));
            Vectors.Add(new Segment(new Vertex(new Vector4(nodes[2, 0], 1)),
                    new Vertex(new Vector4(inNodes[1, 0], 1)), Matrix.Identity,
                    new Vector4(1, 1, 1, 1), new Vector4(1, 1, 1, 1)));
            Vectors.Add(new Segment(new Vertex(new Vector4(nodes[1, 3], 1)),
                    new Vertex(new Vector4(inNodes[0, 1], 1)), Matrix.Identity,
                    new Vector4(1, 1, 1, 1), new Vector4(1, 1, 1, 1)));
            Vectors.Add(new Segment(new Vertex(new Vector4(nodes[2, 3], 1)),
                    new Vertex(new Vector4(inNodes[1, 1], 1)), Matrix.Identity,
                    new Vector4(1, 1, 1, 1), new Vector4(1, 1, 1, 1)));
            //for(int i = 0; i < 4; i++)
            //{
            //    for(int j = 0; j < 4; j++)
            //    {
            //        if (j == 1 || j == 2)
            //        {
            //            if (i == 0)
            //            {
            //                Vectors.Add(new Segment(new Vertex(new Vector4(nodes[i, j], 1)),
            //                    new Vertex(new Vector4(nodes[i + 1, j], 1)), Matrix.Identity,
            //                    new Vector4(1, 1, 1, 1), new Vector4(1, 1, 1, 1)));
            //            }
            //            else if (i == 3)
            //            {
            //                Vectors.Add(new Segment(new Vertex(new Vector4(nodes[i, j], 1)),
            //                    new Vertex(new Vector4(nodes[i - 1, j], 1)), Matrix.Identity,
            //                    new Vector4(1, 1, 1, 1), new Vector4(1, 1, 1, 1)));
            //            }
            //        }
            //        if(i == 1 || i == 2)
            //        {
            //            if (j == 0)
            //            {
            //                Vectors.Add(new Segment(new Vertex(new Vector4(nodes[i, j], 1)),
            //                    new Vertex(new Vector4(nodes[i, j + 1], 1)), Matrix.Identity,
            //                    new Vector4(1, 1, 1, 1), new Vector4(1, 1, 1, 1)));
            //            }
            //            else if (j == 3)
            //            {
            //                Vectors.Add(new Segment(new Vertex(new Vector4(nodes[i, j], 1)),
            //                    new Vertex(new Vector4(nodes[i, j - 1], 1)), Matrix.Identity,
            //                    new Vector4(1, 1, 1, 1), new Vector4(1, 1, 1, 1)));
            //            }
            //        }

            //    }
            //}

            return (nodes, inNodes);
        }

        struct Ind
        {
            public int I;
            public int J;
        }

        public static List<GregoryPatch> CreateFillInPatch(List<Point> corners)
        {
            HashSet<BicubicBezierPatchC0> alreadyChecked = new HashSet<BicubicBezierPatchC0>();
            List<GregoryPatch> gps = new List<GregoryPatch>();
            List<(BicubicBezierPatchC0.Edge, BicubicBezierPatchC0)> edges = new List<(BicubicBezierPatchC0.Edge, BicubicBezierPatchC0)>();
            foreach (var p in corners)
            {
                foreach (var pb in p.PointBased)
                {
                    if(pb is BicubicBezierPatchC0 pc0)
                    {
                        if(alreadyChecked.Add(pc0))
                        {
                            edges.AddRange(pc0.GetBorderEdges(corners));
                        }
                    }
                }
            }
            (BicubicBezierPatchC0.Edge, BicubicBezierPatchC0) e1Found = (new BicubicBezierPatchC0.Edge(), null);
            (BicubicBezierPatchC0.Edge, BicubicBezierPatchC0) e2Found = (new BicubicBezierPatchC0.Edge(), null);
            (BicubicBezierPatchC0.Edge, BicubicBezierPatchC0) e3Found = (new BicubicBezierPatchC0.Edge(), null);
            bool found = false;
            foreach ((BicubicBezierPatchC0.Edge, BicubicBezierPatchC0) e1 in edges)
            {
                foreach ((BicubicBezierPatchC0.Edge, BicubicBezierPatchC0) e2 in edges)
                {
                    foreach ((BicubicBezierPatchC0.Edge, BicubicBezierPatchC0) e3 in edges)
                    {
                        Dictionary<Point, int> checkList = new Dictionary<Point, int>();
                        checkList.Add(corners[0], 0);
                        checkList.Add(corners[1], 0);
                        checkList.Add(corners[2], 0);
                        if(checkList.ContainsKey(e1.Item2.Nodes[e1.Item1.startI, e1.Item1.startJ % e1.Item2.Nodes.GetLength(1)]))
                        {
                            checkList[e1.Item2.Nodes[e1.Item1.startI, e1.Item1.startJ % e1.Item2.Nodes.GetLength(1)]]++;
                        }
                        if (checkList.ContainsKey(e2.Item2.Nodes[e2.Item1.startI, e2.Item1.startJ % e2.Item2.Nodes.GetLength(1)]))
                        {
                            checkList[e2.Item2.Nodes[e2.Item1.startI, e2.Item1.startJ % e2.Item2.Nodes.GetLength(1)]]++;
                        }
                        if (checkList.ContainsKey(e3.Item2.Nodes[e3.Item1.startI, e3.Item1.startJ % e3.Item2.Nodes.GetLength(1)]))
                        {
                            checkList[e3.Item2.Nodes[e3.Item1.startI, e3.Item1.startJ % e3.Item2.Nodes.GetLength(1)]]++;
                        }
                        if (checkList.ContainsKey(e1.Item2.Nodes[e1.Item1.endI, e1.Item1.endJ % e1.Item2.Nodes.GetLength(1)]))
                        {
                            checkList[e1.Item2.Nodes[e1.Item1.endI, e1.Item1.endJ % e1.Item2.Nodes.GetLength(1)]]++;
                        }
                        if (checkList.ContainsKey(e2.Item2.Nodes[e2.Item1.endI, e2.Item1.endJ % e2.Item2.Nodes.GetLength(1)]))
                        {
                            checkList[e2.Item2.Nodes[e2.Item1.endI, e2.Item1.endJ % e2.Item2.Nodes.GetLength(1)]]++;
                        }
                        if (checkList.ContainsKey(e3.Item2.Nodes[e3.Item1.endI, e3.Item1.endJ % e3.Item2.Nodes.GetLength(1)]))
                        {
                            checkList[e3.Item2.Nodes[e3.Item1.endI, e3.Item1.endJ % e3.Item2.Nodes.GetLength(1)]]++;
                        }
                        if(checkList.Values.All(k => k == 2))
                        {
                            e1Found = e1;
                            e2Found = e2;
                            e3Found = e3;
                            found = true;
                        }
                        if (found) break;
                    }
                    if (found) break;
                }
                if (found) break;
            }
            if(!found)
            {
                return null;
            }
            Ind[] p1Ends = new Ind[2];
            Ind[] p2Ends = new Ind[2];
            Ind[] p3Ends = new Ind[2];

            p1Ends[0] = new Ind()
            {
                I = e1Found.Item1.startI,
                J = e1Found.Item1.startJ
            };

            p1Ends[1] = new Ind()
            {
                I = e1Found.Item1.endI,
                J = e1Found.Item1.endJ
            };

            p2Ends[0] = new Ind()
            {
                I = e2Found.Item1.startI,
                J = e2Found.Item1.startJ
            };

            p2Ends[1] = new Ind()
            {
                I = e2Found.Item1.endI,
                J = e2Found.Item1.endJ
            };

            p3Ends[0] = new Ind()
            {
                I = e3Found.Item1.startI,
                J = e3Found.Item1.startJ
            };

            p3Ends[1] = new Ind()
            {
                I = e3Found.Item1.endI,
                J = e3Found.Item1.endJ
            };

            BicubicBezierPatchC0 p1 = e1Found.Item2;
            BicubicBezierPatchC0 p2 = e2Found.Item2;
            BicubicBezierPatchC0 p3 = e3Found.Item2;

            if (p1.Nodes[p1Ends[0].I, p1Ends[0].J % p1.Nodes.GetLength(1)] == p2.Nodes[p2Ends[0].I, p2Ends[0].J % p2.Nodes.GetLength(1)])
            {
                p1Ends = p1Ends.Reverse().ToArray();
            }
            else if (p1.Nodes[p1Ends[0].I, p1Ends[0].J % p1.Nodes.GetLength(1)] == p2.Nodes[p2Ends[1].I, p2Ends[1].J % p2.Nodes.GetLength(1)])
            {
                p1Ends = p1Ends.Reverse().ToArray();
                p2Ends = p2Ends.Reverse().ToArray();
            }
            else if (p1.Nodes[p1Ends[1].I, p1Ends[1].J % p1.Nodes.GetLength(1)] == p2.Nodes[p2Ends[1].I, p2Ends[1].J % p2.Nodes.GetLength(1)])
            {
                p2Ends = p2Ends.Reverse().ToArray();
            }

            if (p2.Nodes[p2Ends[1].I, p2Ends[1].J % p2.Nodes.GetLength(1)] == p3.Nodes[p3Ends[1].I, p3Ends[1].J % p3.Nodes.GetLength(1)])
            {
                p3Ends = p3Ends.Reverse().ToArray();
            }

            (Point[] p1RowPoints, Point[] p1NextRowPoints) = GetImportantNodes(p1.Nodes, p1Ends);
            (Point[] p2RowPoints, Point[] p2NextRowPoints) = GetImportantNodes(p2.Nodes, p2Ends);
            (Point[] p3RowPoints, Point[] p3NextRowPoints) = GetImportantNodes(p3.Nodes, p3Ends);

            List<Point> dep1 = new List<Point>();
            dep1.AddRange(p1RowPoints.Take(3));
            dep1.AddRange(p2RowPoints.Take(3));
            dep1.AddRange(p3RowPoints);

            List<Point> depNext1 = new List<Point>();
            depNext1.AddRange(p1NextRowPoints);
            depNext1.AddRange(p2NextRowPoints);
            depNext1.AddRange(p3NextRowPoints);
            gps.Add(new GregoryPatch(dep1, depNext1, new Vector4(1, 0, 0, 1), new Vector4(1, 0, 0, 1)));

            List<Point> dep2 = new List<Point>();
            dep2.AddRange(p2RowPoints.Take(3));
            dep2.AddRange(p3RowPoints.Take(3));
            dep2.AddRange(p1RowPoints);
            List<Point> depNext2 = new List<Point>();
            depNext2.AddRange(p2NextRowPoints);
            depNext2.AddRange(p3NextRowPoints);
            depNext2.AddRange(p1NextRowPoints);
            gps.Add(new GregoryPatch(dep2, depNext2, new Vector4(1, 0, 0, 1), new Vector4(1, 0, 0, 1)));

            List<Point> dep3 = new List<Point>();
            dep3.AddRange(p3RowPoints.Take(3));
            dep3.AddRange(p1RowPoints.Take(3));
            dep3.AddRange(p2RowPoints);
            List<Point> depNext3 = new List<Point>();
            depNext3.AddRange(p3NextRowPoints);
            depNext3.AddRange(p1NextRowPoints);
            depNext3.AddRange(p2NextRowPoints);
            gps.Add(new GregoryPatch(dep3, depNext3, new Vector4(1, 0, 0, 1), new Vector4(1, 0, 0, 1)));

            foreach(GregoryPatch g in gps)
            {
                p1.OnRemove += g.Remove;
                p2.OnRemove += g.Remove;
                p3.OnRemove += g.Remove;
            }

            return gps;
        }

        private static (Point[], Point[]) GetImportantNodes(Model.Point[,] nodes, Ind[] ends)
        {
            Point[] p = new Point[4];
            Point[] pNext = new Point[4];
            if (ends[0].I == ends[1].I)
            {
                int offset = 0;
                if (ends[0].I == 0)
                {
                    offset = 1;
                }
                else //3
                {
                    offset = -1;
                }

                if(ends[0].J > ends[1].J)
                {
                    for (int i = ends[0].J; i >= ends[1].J; i -= 1)
                    {
                        p[3 - (i - ends[1].J)] = nodes[ends[0].I, i % nodes.GetLength(1)];
                        pNext[3 - (i - ends[1].J)] = nodes[ends[0].I + offset, i % nodes.GetLength(1)];
                    }
                }
                else
                {
                    for (int i = ends[0].J; i <= ends[1].J; i += 1)
                    {
                        p[i - ends[0].J] = nodes[ends[0].I, i % nodes.GetLength(1)];
                        pNext[i - ends[0].J] = nodes[ends[0].I + offset, i % nodes.GetLength(1)];
                    }
                }
                
            }
            else //if (p1Ends[0].J == p1Ends[1].J)
            {
                int offset = 0;
                if (ends[0].J == 0)
                {
                    offset = 1;
                }
                else //3
                {
                    offset = -1;
                }
                if (ends[0].I > ends[1].I)
                {
                    for (int i = ends[0].I; i >= ends[1].I; i -= 1)
                    {
                        p[3 - (i - ends[1].I)] = nodes[i, ends[0].J % nodes.GetLength(1)];
                        pNext[3 - (i - ends[1].I)] = nodes[i, (ends[0].J + offset) % nodes.GetLength(1)];
                    }
                }
                else
                {
                    for (int i = ends[0].I; i <= ends[1].I; i += 1)
                    {
                        p[i - ends[0].I] = nodes[i, ends[0].J % nodes.GetLength(1)];
                        pNext[i - ends[0].I] = nodes[i, (ends[0].J + offset) % nodes.GetLength(1)];
                    }
                }
            }
            return (p, pNext);
        }

        public override void Render(DxRenderer dxRenderer, Matrix worldTransform, bool isSelected, Func<Vector4, Vector4> modifier)
        {
            Geometry<PatchVertex, Index> g = TryUpdateGeometry(dxRenderer);
            if (g.Indices.Length == 0 || g.Vertices.Length == 0)
            {
                return;
            }

            if (Vectors != null && DisplayVectors)
            {
                foreach (Segment vector in Vectors)
                {
                    vector.Render(dxRenderer, worldTransform, isSelected, modifier);
                }
            }

            unsafe
            {
                dxRenderer.SetVertexBuffer<PatchVertex>(vertexBuffer, sizeof(PatchVertex));
            }
            dxRenderer.SetIndexBuffer(indexBuffer);

            dxRenderer.device.ImmediateContext.VertexShader.Set(dxRenderer.vertexShaderGregoryPatch);
            dxRenderer.device.ImmediateContext.PixelShader.Set(dxRenderer.pixelShaderGregoryPatch);
            dxRenderer.device.ImmediateContext.GeometryShader.Set(dxRenderer.geometryShaderGregoryPatch);
            dxRenderer.device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;
            dxRenderer.device.ImmediateContext.InputAssembler.InputLayout = dxRenderer.inputLayoutPatch;

            Matrix modelMatrix = Model * worldTransform;
            DxConstantBuffer cb = new DxConstantBuffer();
            cb.MVP = modelMatrix * dxRenderer.Scene.Camera.VP;
            cb.Color = modifier(isSelected ? Color : UnselectedColor);
            dxRenderer.UpdateConstantBuffer(ref dxRenderer.geometryConstantBuffer, cb);

            DxPatchConstantBuffer pcb = new DxPatchConstantBuffer();
            pcb.precision = new Vector2(PrecisionU, PrecisionV);
            dxRenderer.UpdateConstantBuffer(ref dxRenderer.patchConstantBuffer, pcb);

            dxRenderer.device.ImmediateContext.Draw(g.Vertices.Length, 0);

            dxRenderer.device.ImmediateContext.InputAssembler.InputLayout = dxRenderer.inputLayout;
        }


        public void SwapPoint(Point p1, Point p2)
        {
            bool found = false;
            for (int i = 0; i < DependencyPoints.Count; i++)
            {
                if (DependencyPoints[i] == p1)
                {
                    found = true;
                    DependencyPoints[i] = p2;
                    p2.OnTransformed += UpdateGeometry;
                    p2.AddToObject(this);
                    p1.RemoveFromObject(this);
                }
            }
            if(!found)
            {
                for (int i = 0; i < DependencyNextPoints.Count; i++)
                {
                    if (DependencyNextPoints[i] == p1)
                    {
                        DependencyNextPoints[i] = p2;
                        p2.OnTransformed += UpdateGeometry;
                        p2.AddToObject(this);
                        p1.RemoveFromObject(this);
                    }
                }
            }
            ResetGeometry();
        }

        public override List<IGeometryObject> Remove(Scene scene)
        {
            foreach(var p in DependencyPoints)
            {
                p.RemoveFromObject(this);
            }
            foreach(var p in DependencyNextPoints)
            {
                p.RemoveFromObject(this);
            }
            return base.Remove(scene);
        }
    }
}
