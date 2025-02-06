using System;
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
    public partial class TestPadSettingsView : UserControl
    {
        public TestPadSettingsViewModel Vm { get; set; }
        public TestPadSettingsView()
        {
            InitializeComponent();
            Loaded += UserControl_Loaded;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Vm = DataContext as TestPadSettingsViewModel;
            if (Vm != null)
            {
                SubscribeAppEvents();
            }
        }
        private void OnPropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            try
            {
                if (e.OriginalSource is PropertyItem propItem && (e.OldValue.ToString() != e.NewValue.ToString()))
                {
                    Vm.TestPadSettings.OnDispatchChange(propItem.PropertyName, e.NewValue.ToString());
                    if (propItem.PropertyName == "PrismType")
                    {
                        var prismType = e.NewValue.ToString();
                        Vm.EventFactory().GetEvent<PrismTypeChangedEvent>().Publish(prismType);
                    }
                }
            }
            catch (Exception exception)
            {
                throw new InvalidCastException(exception.Message);
            }
        }
        /// <summary>
        /// Subscribes the application events.
        /// </summary>
        public void SubscribeAppEvents()
        {
            Vm.EventFactory().GetEvent<SettingsUpdateEvent>().Subscribe(OnSettingsUpdateEvent);
        }

        private void OnSettingsUpdateEvent(AppSettings updatedAppSettings)
        {
            Vm.AppSettings = updatedAppSettings;
        }

        public void UnSubscribeAppEvents()
        {

        }
    }
}
