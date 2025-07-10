namespace HCL_ODA_TestPAD.Utility
{
    public static class ColorExtensions
    {
        public static System.Drawing.Color ToDrawingColor(this System.Windows.Media.Color mediaColor)
        {
            return System.Drawing.Color.FromArgb(mediaColor.A,
                                                 mediaColor.R,
                                                 mediaColor.G,
                                                 mediaColor.B);
        }

        public static System.Drawing.Color ToDrawingColor(this System.Windows.Media.SolidColorBrush solidColorBrush)
        {
            return solidColorBrush is null ? System.Drawing.Color.Empty : solidColorBrush.Color.ToDrawingColor();
        }
    }
}
