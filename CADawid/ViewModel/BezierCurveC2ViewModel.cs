using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CADawid.Model;

namespace CADawid.ViewModel
{
    public class BezierCurveC2ViewModel : GeometryViewModel
    {
        public BezierCurveC2 BezierCurveC2 => SelectedObject as BezierCurveC2;
        public bool DisplayBernsteinNodes
        {
            get => BezierCurveC2.DisplayBernsteinNodes;
            set
            {
                BezierCurveC2.DisplayBernsteinNodes = value;
                NotifyPropertyChanged(nameof(DisplayBernsteinNodes));
            }
        }

        public bool DisplayBernsteinPolygon
        {
            get => BezierCurveC2.DisplayBernsteinPolygon;
            set
            {
                BezierCurveC2.DisplayBernsteinPolygon = value;
                NotifyPropertyChanged(nameof(DisplayBernsteinPolygon));
            }
        }

        public bool DisplayBsplinePolygon
        {
            get => BezierCurveC2.DisplayBsplinePolygon;
            set
            {
                BezierCurveC2.DisplayBsplinePolygon = value;
                NotifyPropertyChanged(nameof(DisplayBsplinePolygon));
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
