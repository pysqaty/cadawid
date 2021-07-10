using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CADawid.Model;
using SharpDX;

namespace CADawid.ViewModel
{
    public class TorusViewModel : GeometryViewModel
    {
        public Torus Torus => SelectedObject as Torus;
        public float R
        {
            get => Torus.R;
            set
            {
                Torus.R = value;
                NotifyPropertyChanged("R");
            }
        }
        public float r
        {
            get => Torus.r;
            set
            {
                Torus.r = value;
                NotifyPropertyChanged("r");
            }
        }
        public int Precision1
        {
            get => Torus.Precision1;
            set
            {
                Torus.Precision1 = value;
                NotifyPropertyChanged("Precision1");
            }
        }
        public int Precision2
        {
            get => Torus.Precision2;
            set
            {
                Torus.Precision2 = value;
                NotifyPropertyChanged("Precision2");
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
