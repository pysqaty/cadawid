using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace CADawid.Model
{
    public abstract class Curve<V, I> : GeometryObject<V, I>, ICurve, IPointBased
        where V : unmanaged
        where I : unmanaged
    {
        public List<Point> Nodes { get; set; }
        public override Matrix CurrentModel => Matrix.Identity;
        public override Vector3 CurrentPosition
        {
            get
            {
                Vector3 positon = new Vector3(0f);
                if (Nodes.Count > 0)
                {
                    foreach (Point node in Nodes)
                    {
                        positon += node.CurrentPosition;
                    }
                    positon /= Nodes.Count;
                }
                return positon;
            }
        }

        public Curve(Vector4 color, Vector4 unselectedColor, List<Point> nodes, bool isRemovable)
        {
            Nodes = new List<Point>();
            foreach (IGeometryObject n in nodes)
            {
                if(n is Point node)
                {
                    node.OnTransformed += UpdateGeometry;
                    Nodes.Add(node);
                    node.AddToObject(this);
                }
            }
            Color = color;
            UnselectedColor = unselectedColor;
            IsRemovable = isRemovable;
        }

        public virtual bool AddNode(IGeometryObject node)
        {
            if(node is Point n)
            {
                //if (!Nodes.Contains(node))
                //{
                    node.OnTransformed += UpdateGeometry;
                    Nodes.Add(n);
                    n.AddToObject(this);
                    ResetGeometry();
                //}
                return true;
            }
            else
            {
                return false;
            }
        }
        public virtual void AddNodes(IList nodes)
        {
            foreach(IGeometryObject node in nodes)
            {
                if (node is Point n)
                {
                    //if (!Nodes.Contains(node))
                    //{
                        node.OnTransformed += UpdateGeometry;
                        Nodes.Add(n);
                        n.AddToObject(this);
                    //}
                }
            }
            ResetGeometry();
        }
        public delegate void EmptyNodes(IGeometryObject p);
        public event EmptyNodes OnEmptyNodes = delegate { };
        public virtual void RemoveNodes(IGeometryObject[] removedGeometries)
        {
            foreach (IGeometryObject geometry in removedGeometries)
            {
                if (geometry is Point node)
                {
                    Nodes.Remove(node);
                    node.RemoveFromObject(this);
                }
            }
            if(Nodes.Count == 0)
            {
                OnEmptyNodes(this);
            }
            ResetGeometry();
        }
        public virtual void RemoveNodes(IList removedGeometries)
        {
            foreach (IGeometryObject geometry in removedGeometries)
            {
                if (geometry is Point node)
                {
                    Nodes.Remove(node);
                    node.RemoveFromObject(this);
                }
            }
            if (Nodes.Count == 0)
            {
                OnEmptyNodes(this);
            }
            ResetGeometry();
        }
        public void UpdateGeometry(IGeometryObject geometryObject)
        {
            ResetGeometry();
        }

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

        public void SwapPoint(Point p1, Point p2)
        {
            for(int i = 0; i < Nodes.Count; i++)
            {
                if(Nodes[i] == p1)
                {
                    Nodes[i] = p2;
                    p2.OnTransformed += UpdateGeometry;
                    p2.AddToObject(this);
                    p1.RemoveFromObject(this);
                }
            }
            ResetGeometry();
        }

        public override List<IGeometryObject> Remove(Scene scene)
        {
            foreach(var p in Nodes)
            {
                p.RemoveFromObject(this);
            }
            return base.Remove(scene);
        }
    }
}
