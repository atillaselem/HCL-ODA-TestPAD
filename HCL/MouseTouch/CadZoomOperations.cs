using System;
using HCL_ODA_TestPAD.HCL.Exceptions;
using ODA.Kernel.TD_RootIntegrated;
using ODA.Visualize.TV_Visualize;

namespace HCL_ODA_TestPAD.HCL.MouseTouch;

public sealed class CadZoomOperations : ICadZoomOperations
{
    private readonly OdTvGsViewId _odTvGsViewId;

    public CadZoomOperations(OdTvGsViewId odTvGsViewId)
    {
        _odTvGsViewId = odTvGsViewId;
    }
    public void Zoom(double scale)
    {
        using var odTvGsView = _odTvGsViewId.openObject(OdTv_OpenMode.kForWrite);
        odTvGsView.zoom(scale);
    }
    public void ZoomToArea(OdGePoint3d point1, OdGePoint3d point2)
    {
        if (point1 == null)
        {
            throw new ArgumentNullException(nameof(point1));
        }
        if (point2 == null)
        {
            throw new ArgumentNullException(nameof(point2));
        }
        try
        {
            using var odTvGsView = _odTvGsViewId.openObject(OdTv_OpenMode.kForWrite);
            using var worldToEye = odTvGsView.eyeToWorldMatrix().invert(); 
            using var temp1 = point1.transformBy(worldToEye);
            using var temp2 = point2.transformBy(worldToEye);
            using var vector = point2 - point1;

            if (vector.x != 0 && vector.y != 0)
            {
                using var newPosition = point1 + vector / 2.0;

                vector.x = vector.x < 0 ? -vector.x : vector.x;
                vector.y = vector.y < 0 ? -vector.y : vector.y;

                odTvGsView.dolly(newPosition.asVector());
                var fieldWidth = odTvGsView.fieldWidth() / vector.x;
                var fieldHeight = odTvGsView.fieldHeight() / vector.y;

                odTvGsView.zoom(fieldWidth < fieldHeight ? fieldWidth : fieldHeight);
            }
        }
        catch (ZoomNotPossibleException ex)
        {
            //_logger.LogError("CadView : zoom not possible");
            throw ex;
        }
        catch (Exception)
        {
            //_logger.LogError(exception, "CadView : zoom to area failed");
        }
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private bool _isDisposed;

    /// <summary>
    ///  Disposes managed code if any
    /// </summary>
    /// <param name="disposing">Disposing flag</param>
    private void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        if (disposing)
        {
        }

        _isDisposed = true;
    }
}