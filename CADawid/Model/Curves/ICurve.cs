using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CADawid.Model
{
    public interface ICurve
    {
        bool AddNode(IGeometryObject node);
        void AddNodes(IList nodes);
        void RemoveNodes(IGeometryObject[] removedGeometries);
        void RemoveNodes(IList removedGeometries);
    }
}
