using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CADawid.Model;

namespace CADawid.ViewModel
{
    public class PatchViewModel : GeometryViewModel
    {
        public BicubicPatch<PatchVertex, Index> Patch => SelectedObject as BicubicPatch<PatchVertex, Index>;

        public bool DisplayBezierGrid
        {
            get
            {
                return Patch.DisplayBezierGrid;
            }
            set
            {
                Patch.DisplayBezierGrid = value;
                NotifyPropertyChanged(nameof(DisplayBezierGrid));
            }
        }
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

        public int PatchesU
        {
            get => Patch.PatchesU;
            set
            {
                Patch.PatchesU = value;
                NotifyPropertyChanged(nameof(PatchesU));
            }
        }

        public int PatchesV
        {
            get => Patch.PatchesV;
            set
            {
                Patch.PatchesV = value;
                NotifyPropertyChanged(nameof(PatchesV));
            }
        }

        public float Width
        {
            get => Patch.LengthU;
            set
            {
                Patch.LengthU = value;
                NotifyPropertyChanged(nameof(Width));
            }
        }

        public float Height
        {
            get => Patch.LengthV;
            set
            {
                Patch.LengthV = value;
                NotifyPropertyChanged(nameof(Height));
            }
        }

        public float R
        {
            get => Patch.R;
            set
            {
                Patch.R = value;
                NotifyPropertyChanged(nameof(R));
            }
        }

        public float CylinderHeight
        {
            get => Patch.Height;
            set
            {
                Patch.Height = value;
                NotifyPropertyChanged(nameof(CylinderHeight));
            }
        }

        public bool IsCylindrical
        {
            get => Patch.IsCylindrical;
            set
            {
                Patch.IsCylindrical = value;
                NotifyPropertyChanged(nameof(IsCylindrical));
            }
        }

        public override void Visit(IGeometryPanel visitor)
        {
            visitor.Accept(this);
        }

        public override void VisitToAdd(IGeometryAddPanel visitor)
        {
            visitor.AcceptToAdd(this);
        }
    }
}
