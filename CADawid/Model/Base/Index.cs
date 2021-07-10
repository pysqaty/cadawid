using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CADawid.Model
{
    public struct Index
    {
        public UInt16 Ind { get; set; }

        public Index(UInt16 ind)
        {
            Ind = ind;
        }
    }
}
