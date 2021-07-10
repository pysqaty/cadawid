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
    /// Interaction logic for TorusPanel.xaml
    /// </summary>
    public partial class TorusPanel : UserControl
    {
        public TorusViewModel TorusViewModel { get; set; }
        public TorusPanel(TorusViewModel viewModel)
        {
            TorusViewModel = viewModel;
            InitializeComponent();
            DataContext = TorusViewModel;
        }
        private void Parameter_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TorusViewModel.SelectedObject.ResetGeometry();
        }
    }
}
