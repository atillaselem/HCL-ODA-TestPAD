using HCL_ODA_TestPAD.ViewModels;
using System.Windows.Controls;

namespace HCL_ODA_TestPAD.Views
{
    /// <summary>
    /// Interaction logic for AppMonitor.xaml
    /// </summary>
    public partial class AppMonitorView : UserControl
    {
        public AppMonitorView()
        {
            InitializeComponent();
        }

        public AppMonitorView(AppMonitorViewModel viewModel)
        {
            //DataContext = viewModel;
        }

        private void ScrollToContent(object sender, TextChangedEventArgs e)
        {
            if (e.OriginalSource is TextBox textBox)
            {
                textBox.ScrollToEnd();
            }
        }
    }
}
