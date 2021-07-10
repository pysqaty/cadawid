using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CADawid.Model
{
    public interface ISurface
    {
        void SetNodes(Point[,] nodes);
    }
}
