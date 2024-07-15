using HCL_ODA_TestPAD.ViewModels;
using System.Windows.Controls;

namespace HCL_ODA_TestPAD.Views
{
    /// <summary>
    /// Interaction logic for LoadingOverlay.xaml
    /// </summary>
    public partial class LoadingOverlay : UserControl
    {
        public LoadingOverlay()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var dataContext = (OverlayViewModel)this.DataContext;
            dataContext.CancelTokenSource.Cancel();
            dataContext.IsLoading = false;
        }
    }
}
