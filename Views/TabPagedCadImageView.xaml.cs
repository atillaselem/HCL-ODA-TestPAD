using HCL_ODA_TestPAD.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace HCL_ODA_TestPAD.Views
{
    /// <summary>
    /// Interaction logic for TabPagedCadImageView.xaml
    /// </summary>
    public partial class TabPagedCadImageView : UserControl
    {
        private TabPagedCadImageViewModel ViewModel { get; set; }
        public TabPagedCadImageView()
        {
            InitializeComponent();
            CadTabPageControl.SelectionChanged += CadTabPageControl_SelectionChanged;
        }

        private void CadTabPageControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.TabPageSelectionChanged(CadTabPageControl.SelectedIndex);
        }

        private void OnTabControlLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel = (TabPagedCadImageViewModel)DataContext;
        }
    }
}
