using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CADawid.Model;

namespace CADawid.ViewModel
{
    public class GregoryPatchViewModel : GeometryViewModel
    {
        public GregoryPatch Patch => SelectedObject as GregoryPatch;

        public int PrecisionU
        {
            get => Patch.PrecisionU;
            set
            {
                Patch.PrecisionU = value;
                NotifyPropertyChanged(nameof(PrecisionU));
            }
        }
        public int PrecisionV
        {
            get => Patch.PrecisionV;
            set
            {
                Patch.PrecisionV = value;
                NotifyPropertyChanged(nameof(PrecisionV));
            }
        }

        public bool DisplayVectors
        {
            get => Patch.DisplayVectors;
            set
            {
                Patch.DisplayVectors = value;
                NotifyPropertyChanged(nameof(DisplayVectors));
            }
        }

        public override void Visit(IGeometryPanel visitor)
        {
            visitor.Accept(this);
        }

        public override void VisitToAdd(IGeometryAddPanel visitor)
        {
            throw new NotImplementedException();
        }
    }
}
