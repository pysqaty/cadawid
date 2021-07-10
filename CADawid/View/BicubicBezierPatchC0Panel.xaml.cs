using System;
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
    /// Interaction logic for BicubicBezierPatchC0Panel.xaml
    /// </summary>
    public partial class BicubicBezierPatchC0Panel : UserControl
    {
        public PatchViewModel BicubicBezierPatchC0ViewModel { get; set; } 
        public BicubicBezierPatchC0Panel(PatchViewModel viewModel)
        {
            BicubicBezierPatchC0ViewModel = viewModel;
            InitializeComponent();
            DataContext = BicubicBezierPatchC0ViewModel;
        }

        private void Parameter_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BicubicBezierPatchC0ViewModel.SelectedObject.ResetGeometry();
        }
    }
}
