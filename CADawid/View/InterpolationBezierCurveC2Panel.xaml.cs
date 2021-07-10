using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CADawid.ViewModel;

namespace CADawid.View
{
    /// <summary>
    /// Interaction logic for InterpolationBezierCurveC2Panel.xaml
    /// </summary>
    public partial class InterpolationBezierCurveC2Panel : UserControl
    {
        private IList selectedObjects;
        public InterpolationBezierCurveC2ViewModel InterpolationBezierCurveC2ViewModel { get; set; }
        public InterpolationBezierCurveC2Panel(InterpolationBezierCurveC2ViewModel interpolationBezierCurveC2ViewModel, IList selection)
        {
            InterpolationBezierCurveC2ViewModel = interpolationBezierCurveC2ViewModel;
            selectedObjects = selection;
            InitializeComponent();
            DataContext = InterpolationBezierCurveC2ViewModel;
        }
        private void AddPointBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            InterpolationBezierCurveC2ViewModel.InterpolationBezierCurveC2.AddNodes(selectedObjects);
        }

        private void RemovePointsBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            InterpolationBezierCurveC2ViewModel.InterpolationBezierCurveC2.RemoveNodes(selectedObjects);
        }
    }
}
