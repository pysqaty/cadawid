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
    /// Interaction logic for BezierCurveC2Panel.xaml
    /// </summary>
    public partial class BezierCurveC2Panel : UserControl
    {
        private IList selectedObjects;
        public BezierCurveC2ViewModel BezierCurveC2ViewModel { get; set; }

        public BezierCurveC2Panel(BezierCurveC2ViewModel bezierCurveC2ViewModel, IList selection)
        {
            BezierCurveC2ViewModel = bezierCurveC2ViewModel;
            selectedObjects = selection;
            InitializeComponent();
            DataContext = BezierCurveC2ViewModel;
        }

        private void AddPointBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            BezierCurveC2ViewModel.BezierCurveC2.AddNodes(selectedObjects);
        }

        private void RemovePointsBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            BezierCurveC2ViewModel.BezierCurveC2.RemoveNodes(selectedObjects);
        }
    }
}
