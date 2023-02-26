using HCL_ODA_TestPAD.EventBroker;
using HCL_ODA_TestPAD.Mvvm.Events;
using HCL_ODA_TestPAD.ViewModels;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace HCL_ODA_TestPAD.Views
{
    /// <summary>
    /// Interaction logic for TestPADSettingsView.xaml
    /// </summary>
    public partial class TestPADSettingsView : UserControl
    {
        //private readonly ISettingsProvider _trSettings;

        public TestPADSettingsView()
        {
            InitializeComponent();
            SubscribeAppEvents();
            
        }   
        private void OnPropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            if (e.OriginalSource is PropertyItem propItem)
            {
                //MessageBox.Show($"Name : {propItem.PropertyName} - Value : {e.NewValue}");
                var trSettings = ((TestPADSettingsViewModel)DataContext).TrSettings;
                trSettings.OnDispatchChange(propItem.PropertyName, e.NewValue);
            }
        }
        /// <summary>
        /// Subscribes the application events.
        /// </summary>
        public void SubscribeAppEvents()
        {
            FxBroker<AET>.Instance.Subscribe(AET.EVENT_UPDATE_SETTINGS_UI, OnUpdateSettingUi);
        }

        private void OnUpdateSettingUi(AppEvent<AET, object> obj)
        {
            _propertyGrid.Update();
        }
        public void UnSubscribeAppEvents()
        {
            FxBroker<AET>.Instance.UnSubscribe(AET.EVENT_UPDATE_SETTINGS_UI, OnUpdateSettingUi);
        }
    }
}
