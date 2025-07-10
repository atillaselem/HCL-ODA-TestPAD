using HCL_ODA_TestPAD.Settings;
using HCL_ODA_TestPAD.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace HCL_ODA_TestPAD.Views
{
    /// <summary>
    /// Interaction logic for CadLayersView.xaml
    /// </summary>
    public partial class CadLayersView : UserControl
    {
        public CadLayersViewModel Vm { get; set; }
        public CadLayersView()
        {
            InitializeComponent();
            Loaded += UserControl_Loaded;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Vm = DataContext as CadLayersViewModel;
            if (Vm != null)
            {
                SubscribeAppEvents();
            }
        }
        /// <summary>
        /// Subscribes the application events.
        /// </summary>
        public void SubscribeAppEvents()
        {
            //Vm.EventFactory().GetEvent<SettingsUpdateEvent>().Subscribe(OnSettingsUpdateEvent);
        }

        private void OnSettingsUpdateEvent(AppSettings updatedAppSettings)
        {
            //Vm.AppSettings = updatedAppSettings;
        }
    }
}
