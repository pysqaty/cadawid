using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace CADawid.Model
{
    public abstract class BicubicPatch<V, I> : Surface<V, I>, ISurface, IPointBased
        where V : unmanaged
        where I : unmanaged
    {
        public BicubicPatch(Vector4 color, Vector4 unselectedColor, bool isRemovable = true) : base(color, unselectedColor, isRemovable)
        {
        }

        public override Vector3 CurrentPosition
        {
            get
            {
                Vector3 positon = new Vector3(0f);
                if (Nodes != null)
                {
                    foreach (Point node in Nodes)
                    {
                        positon += node.CurrentPosition;
                    }
                    positon /= Nodes.GetLength(0) * Nodes.GetLength(1);
                }
                return positon;
            }
        }
        public Point[,] Nodes { get; set; }
        public bool IsCylindrical { get; set; }
        public int PatchesU { get; set; }
        public int PatchesV { get; set; }

        public float LengthU { get; set; }
        public float LengthV { get; set; }

        public float R { get; set; }
        public float Height { get; set; }

        public bool DisplayBezierGrid { get; set; }

        public override void Translate(Vector3 translation)
        {
            foreach (Point node in Nodes)
            {
                node.Translate(translation);
            }
            NotifyTransformed();
        }
        public override void Rotate(Vector3 rotation, Vector3 pivot)
        {
            foreach (Point node in Nodes)
            {
                node.Rotate(rotation, pivot);
            }
            NotifyTransformed();
        }
        public override void RotateLocally(Vector3 rotation)
        {
            foreach (Point node in Nodes)
            {
                node.RotateLocally(rotation);
            }
            NotifyTransformed();
        }
        public override void Scale(Vector3 scale, Vector3 pivot)
        {
            foreach (Point node in Nodes)
            {
                node.Scale(scale, pivot);
            }
            NotifyTransformed();
        }
        public override void ScaleLocally(Vector3 scale)
        {
            foreach (Point node in Nodes)
            {
                node.ScaleLocally(scale);
            }
            NotifyTransformed();
        }

        public abstract void SetNodes(Point[,] nodes);

        public void SwapPoint(Point p1, Point p2)
        {
            for (int i = 0; i < Nodes.GetLength(0); i++)
            {
                for (int j = 0; j < Nodes.GetLength(1); j++)
                {
                    if (Nodes[i, j] == p1)
                    {
                        Nodes[i, j] = p2;
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
            List<IGeometryObject> removed = new List<IGeometryObject>();
            foreach (Point node in Nodes)
            {
                node.RemoveFromObject(this);
            }
            removed.AddRange(base.Remove(scene));
            var gps = OnRemove?.Invoke(scene);
            if(gps != null)
            {
                removed.AddRange(gps);
            }
            
            return removed;
        }

        public delegate List<IGeometryObject> Removed(Scene scene);
        public event Removed OnRemove;
    }
}
