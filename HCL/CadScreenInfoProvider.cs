using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace HCL_ODA_TestPAD.HCL;

internal static partial class NativeMethods
{
    [DllImport("User32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    public static extern IntPtr MonitorFromPoint([In] System.Drawing.Point pt, [In] uint dwFlags);

    [DllImport("Shcore.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    public static extern IntPtr GetDpiForMonitor([In] IntPtr hmonitor, [In] DpiType dpiType, [Out] out uint dpiX, [Out] out uint dpiY);
}
public interface ICadScreenInfoProvider
{
    (uint dpiX, uint dpiY) GetEffectiveDpi();

    void GetRawDpi(out uint dpiX, out uint dpiY);
}
internal enum DpiType
{
    EFFECTIVE = 0,
    RAW = 2,
}
public sealed class CadScreenInfoProvider : ICadScreenInfoProvider
{
    //private static readonly ILogger _logger = new HPLLogger(typeof(CadScreenInfoProvider));

    /// <summary>
    /// Value indicating <see cref="NativeMethods.MonitorFromPoint"/> to return a handle to the display monitor that is nearest to the point.
    /// </summary>
    private const int _MONITOR_DEFAULT_TO_NEAREST = 2;

    /// <summary>
    /// Value returned if <see cref="NativeMethods.GetDpiForMonitor"/> successfully returns the X and Y DPI values for the specified monitor.
    /// </summary>
    private const int _S_OK = 0;

    /// <summary>
    /// Value returned if <see cref="NativeMethods.GetDpiForMonitor"/> the DPI type, or pointers passed in are not valid.
    /// </summary>
    private const int _E_INVALID_ARG = -2147024809;

    /// <summary>
    /// Persistent values for effective and raw dpi in x and y directions
    /// </summary>
    private uint _effectiveDpiX, _effectiveDpiY, _rawDpiX, _rawDpiY;

    /// <summary>
    /// Returns the scaling of the given screen.
    /// </summary>
    /// <param name="dpiType">The type of dpi that should be given back..</param>
    /// <param name="dpiX">Gives the horizontal scaling back (in dpi).</param>
    /// <param name="dpiY">Gives the vertical scaling back (in dpi).</param>
    private static void GetDpi(DpiType dpiType, out uint dpiX, out uint dpiY)
    {
        try
        {
            var point = new System.Drawing.Point(1, 1);
            var monitor = NativeMethods.MonitorFromPoint(point, _MONITOR_DEFAULT_TO_NEAREST);

            switch (NativeMethods.GetDpiForMonitor(monitor, dpiType, out dpiX, out dpiY).ToInt32())
            {
                case _S_OK:
                    return;
                case _E_INVALID_ARG:
                    throw new ArgumentException(
                        "Invalid Argument. See https://msdn.microsoft.com/en-us/library/windows/desktop/dn280510.aspx for more information.");
                default:
                    throw new ArgumentException(
                        "Unknown error. See https://msdn.microsoft.com/en-us/library/windows/desktop/dn280510.aspx for more information.");
            }
        }

        catch (ArgumentException argumentException)
        {
            dpiX = 96;
            dpiY = 96;
            var ex = argumentException.InnerException ?? argumentException;
        }
    }


    /// <inheritdoc cref="IScreenInfoProvider"/>
    public void GetRawDpi(out uint dpiX, out uint dpiY)
    {
        if (_rawDpiX == 0 || _rawDpiY == 0)
        {
            GetDpi(DpiType.RAW, out _rawDpiX, out _rawDpiY);
        }

        if (_rawDpiX == 165 && _rawDpiY == 184)
        {
            _rawDpiX = 226;
            _rawDpiY = 226;
        }

        dpiX = _rawDpiX;
        dpiY = _rawDpiY;
    }

    public (uint dpiX, uint dpiY) GetEffectiveDpi()
    {
        if (_effectiveDpiX == 0 || _effectiveDpiY == 0)
        {
            GetDpi(DpiType.EFFECTIVE, out _effectiveDpiX, out _effectiveDpiY);
        }

        return (_effectiveDpiX, _effectiveDpiY);
    }

    private static double _dpiScaleFactor;
    public static double DpiScaleFactor
    {
        get
        {
            if (_dpiScaleFactor != 0)
            {
                return _dpiScaleFactor;
            }

            var infoProvider = new CadScreenInfoProvider().GetEffectiveDpi();
            _dpiScaleFactor = infoProvider.dpiX / 96.0;

            return _dpiScaleFactor;
        }
    }

    public static Point DpiScaledPoint(Point point)
    {
        return new Point(point.X * DpiScaleFactor,
            point.Y * DpiScaleFactor);
    }
}
