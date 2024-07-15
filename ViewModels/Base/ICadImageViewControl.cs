using System;
using System.Windows;
using System.Windows.Media.Imaging;
using HCL_ODA_TestPAD.HCL.CAD.Math.API;
using HCL_ODA_TestPAD.Settings;
using ODA.Visualize.TV_Visualize;

namespace HCL_ODA_TestPAD.ViewModels.Base;

public interface ICadImageViewControl
{
    void InvalidateControl();
    void SetImageSource(WriteableBitmap writableBitmap);
    void SetFileLoaded(bool isFileLoaded, string filePath, Action<string> emitEvent, bool isCancelled = false);
    void SetRenderMode(OdTvGsView_RenderMode renderMode) { }
    void SetRenderModeButton(OdTvGsView_RenderMode model) { }
    void UpdateView() {}
}

public interface IHclTooling
{
    OdTvGsViewId GetViewId();
    bool IsCadViewRotated();
    CadPoint3D UpdateStatusBarCoordinates(Point mousePosition);
    CadPoint3D GetViewCenter();
    OdTvDatabaseId TvDatabaseId { get; set; }
    double GetScaleFactor();
    (CadPoint3D min, CadPoint3D max) GetViewExtent();
    IServiceFactory ServiceFactory { get; }
    double LastRadius { get; set; }
}
