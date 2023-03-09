using HCL_ODA_TestPAD.Mvvm.Events;
using HCL_ODA_TestPAD.ViewModels;
using System.Windows;
using System.Windows.Controls;
using HCL_ODA_TestPAD.Settings;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace HCL_ODA_TestPAD.Views
{
    /// <summary>
    /// Interaction logic for TestPADSettingsView.xaml
    /// </summary>
    public partial class TestPADSettingsView : UserControl
    {
        //private readonly ISettingsProvider _trSettings;
        public TestPADSettingsViewModel VM { get; set; }
        public TestPADSettingsView()
        {
            InitializeComponent();
            Loaded += UserControl_Loaded;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            VM = DataContext as TestPADSettingsViewModel;
            if (VM != null)
            {
                SubscribeAppEvents();
            }
        }
        private void OnPropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            if (e.OriginalSource is PropertyItem propItem && (e.OldValue.ToString() != e.NewValue.ToString()))
            {
                VM.SettingsProvider.OnDispatchChange(propItem.PropertyName, e.NewValue.ToString());
            }
        }
        /// <summary>
        /// Subscribes the application events.
        /// </summary>
        public void SubscribeAppEvents()
        {
            VM.EventFactory().GetEvent<SettingsUpdateEvent>().Subscribe(OnSettingsUpdateEvent);
        }

        private void OnSettingsUpdateEvent(AppSettings updatedAppSettings)
        {
            VM.AppSettings = updatedAppSettings;
        }

        public void UnSubscribeAppEvents()
        {

        }
    }
}
