using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using HCL_ODA_TestPAD.Views;
using HCL_ODA_TestPAD.ViewModels;

namespace HCL_ODA_TestPAD.Resources.Converters
{
    public class ViewModelToViewConverter : IValueConverter
    {
        private Dictionary<Type, Type> _viewModelViewMapper = new Dictionary<Type, Type>()
        {
            { typeof(HclCadImageViewModel), typeof(HclCadImageView) }
        };

        public static Dictionary<string, HclCadImageView> DictModelView = new();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            //use naming convention or custom settings here to get view type
            var viewModelType = value.GetType();
            if (_viewModelViewMapper.ContainsKey(viewModelType))
            {
                var viewType = _viewModelViewMapper[viewModelType];
                HclCadImageView view = null;
                var modelTitle = ((HclCadImageViewModel)value).TabItemTitle;
                if (!DictModelView.ContainsKey(modelTitle))
                {
                    view = (HclCadImageView)Activator.CreateInstance(viewType);
                    DictModelView.Add(modelTitle, view);
                    view.DataContext = value;
                }
                return DictModelView[modelTitle];

            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
