using System;
using System.Windows.Data;
using System.Windows.Media;

namespace HCL_ODA_TestPAD.Resources.Converters
{
    public class CategoryToBrushConverter : IValueConverter
    {
        private const string Category01 = "Fibex_FlexRay";
        private const string Category02 = "Fibex_CAN";
        private const string Category03 = "Fibex_CAN_FD";
        private const string Category04 = "Ldf_LIN";
        private const string Category05 = "ZF-4";
        private const string Category06 = "ZF-5";
        private const string Category07 = "E2E";

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(Brush))
                throw new ArgumentException("targetType");
            var cat = (string)value;

            switch (cat)
            {
                case Category01: return Brushes.LightYellow;
                case Category02: return Brushes.Thistle;
                case Category03: return Brushes.LightCyan;
                case Category04: return Brushes.LightBlue;
                case Category05: return Brushes.Khaki;
                case Category06: return Brushes.Thistle;
                case Category07: return Brushes.DarkKhaki;
                default: return Brushes.WhiteSmoke;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
