using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using CADawid.Model;
using CADawid.ViewModel;

namespace CADawid.View
{
    /// <summary>
    /// Interaction logic for BezierCurveC0Panel.xaml
    /// </summary>
    public partial class BezierCurveC0Panel : UserControl
    {
        private IList selectedObjects;
        public BezierCurveC0ViewModel BezierCurveC0ViewModel { get; set; }
        public BezierCurveC0Panel(BezierCurveC0ViewModel bezierCurveC0ViewModel, IList selection)
        {
            BezierCurveC0ViewModel = bezierCurveC0ViewModel;
            selectedObjects = selection;
            InitializeComponent();
            DataContext = BezierCurveC0ViewModel;
        }

        private void AddPointBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            BezierCurveC0ViewModel.BezierCurveC0.AddNodes(selectedObjects);
        }

        private void RemovePointsBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            BezierCurveC0ViewModel.BezierCurveC0.RemoveNodes(selectedObjects);
        }
    }
}
