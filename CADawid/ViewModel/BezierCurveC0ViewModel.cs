using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CADawid.Model;

namespace CADawid.ViewModel
{
    public class BezierCurveC0ViewModel : GeometryViewModel
    {
        public BezierCurveC0 BezierCurveC0 => SelectedObject as BezierCurveC0;
        public bool DisplayPolygonal
        {
            get => BezierCurveC0.DisplayPolygonal;
            set
            {
                BezierCurveC0.DisplayPolygonal = value;
                NotifyPropertyChanged(nameof(DisplayPolygonal));
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
