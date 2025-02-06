using HCL_ODA_TestPAD.Mvvm.Events;
using HCL_ODA_TestPAD.Settings;
using HCL_ODA_TestPAD.ViewModels;
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
