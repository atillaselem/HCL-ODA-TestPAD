using HCL_ODA_TestPAD.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace HCL_ODA_TestPAD.Views
{
    /// <summary>
    /// Interaction logic for TabbedCadModelView.xaml
    /// </summary>
    public partial class TabbedCadModelView : UserControl
    {
        public TabbedCadModelView()
        {
            InitializeComponent();
        }
        private void OnTabControlLoaded(object sender, RoutedEventArgs e)
        {
            var viewModel = (TabbedCadModelViewModel)DataContext;
            viewModel.UiDispatcher = Dispatcher;
        }
    }
}
