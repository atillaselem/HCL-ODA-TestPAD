using System;
using System.Collections.Generic;
using System.Windows;
using HCL_ODA_TestPAD.Functional.Extensions;
using HCL_ODA_TestPAD.HCL;
using ODA.Visualize.TV_Visualize;
using static HCL_ODA_TestPAD.HCL.CadSelectionConstants;
using HCL_ODA_TestPAD.ODA.ModelBrowser;

namespace HCL_ODA_TestPAD.ViewModels;

public partial class HclCadImageViewModel
{
    public OdTvSelectionSet SelectionSet;
    public enum SelectState
    {
        Point = 0,
        Window = 1,
        Crossing = 2
    }

    private OdTvDCPoint[] _selectionPoints;

    public bool AutoSelectWhileHover { get; set; }
    public string SelectionLevel { get; set; } = "Entities";
    public string SelectionMode { get; set; } = "Point";

    public bool SelectionEnabled { get; set; }

    // list with selected items(bold nodes in model browser)
    public List<TvTreeItem> SelectedNodes = new List<TvTreeItem>();

    public void DoSelection(params Point[] wpfLocation)
    {
        if (!SelectionEnabled)
        {
            return;
        }
        using var odTvGsView = GetView();
        if (odTvGsView == null) return;
        using var option = new OdTvSelectionOptions();
        option.setLevel(GetSelectionLevel());
        var selectionMode = GetSelectionMode();
        option.setMode(selectionMode);

        _selectionPoints = new OdTvDCPoint[2];
        if (selectionMode == OdTvSelectionOptions_Mode.kPoint)
        {
            option.setPickBoxSize(ServiceFactory.AppSettings.PickBoxSize);
            var location = CadScreenInfoProvider.DpiScaledPoint(wpfLocation[0]);
            _selectionPoints[0] = new OdTvDCPoint((int)location.X, (int)location.Y);
            _selectionPoints[1] = new OdTvDCPoint((int)location.X, (int)location.Y);
        }
        else if (selectionMode is OdTvSelectionOptions_Mode.kWindow or OdTvSelectionOptions_Mode.kCrossing)
        {
            if (wpfLocation.Length != 2)
            {
                return;
            }
            var topLeft = CadScreenInfoProvider.DpiScaledPoint(wpfLocation[0]);
            var bottomRight = CadScreenInfoProvider.DpiScaledPoint(wpfLocation[1]);
            _selectionPoints[0] = new OdTvDCPoint((int)bottomRight.X, (int)bottomRight.Y);
            _selectionPoints[1] = new OdTvDCPoint((int)topLeft.X, (int)topLeft.Y);
        }

        var selectionSet = odTvGsView.select(_selectionPoints, option, TvTopModelId);
        if (selectionSet != null && selectionSet.numItems() == 0)
        {
            DisposeSelectionSet(odTvGsView);
            UpdateCadView();
            return;
        }

        if (selectionSet != null && selectionSet.numItems() > 0)
        {
            // clear previous selection by de-highlighting permanent selectionSet and Dispose
            DisposeSelectionSet(odTvGsView);
            Highlight(odTvGsView, selectionSet, true);
            SelectionSet = selectionSet;
        }

        _selectionPoints.ForEach(p => p.Dispose());
        Array.Clear(_selectionPoints);

        UpdateCadView();
    }

    public void SetSelectionLevel()
    {
        TvTopModel = GetTopModel();
        using var selDef = TvTopModel.getSelectability();

        selDef.setVertices(false);
        selDef.setFaces(false);
        selDef.setEdges(false);

        switch (SelectionLevel)
        {
            case SL_Entities:
                selDef.setDefault();
                break;
            case SL_Geometries:
                selDef.setGeometries(true);
                break;
            case SL_Vertices:
                selDef.setVertices(true);
                break;
            case SL_Edges:
                selDef.setEdges(true);
                break;
            case SL_Faces:
                selDef.setFaces(true);
                break;
        }

        TvTopModel.setSelectability(selDef);
        UpdateCadView(true);
    }
    private OdTvSelectionOptions_Level GetSelectionLevel() => SelectionLevel switch
    {
        SL_Entities => OdTvSelectionOptions_Level.kEntity,
        SL_Geometries => OdTvSelectionOptions_Level.kGeometry,
        SL_Vertices => OdTvSelectionOptions_Level.kSubGeometry,
        SL_Edges => OdTvSelectionOptions_Level.kSubGeometry,
        SL_Faces => OdTvSelectionOptions_Level.kSubGeometry,
        _ => OdTvSelectionOptions_Level.kEntity
    };
    public OdTvSelectionOptions_Mode GetSelectionMode() => SelectionMode switch
    {
        SM_Point => OdTvSelectionOptions_Mode.kPoint,
        SM_Window => OdTvSelectionOptions_Mode.kWindow,
        SM_Crossing => OdTvSelectionOptions_Mode.kCrossing,
        _ => OdTvSelectionOptions_Mode.kPoint
    };
    private void DisposeSelectionSet(OdTvGsView odTvGsView)
    {
        if (SelectionSet == null) return;
        Highlight(odTvGsView, SelectionSet, false);
        SelectionSet.Dispose();
        SelectionSet = null;
    }
    private void Highlight(OdTvGsView odTvGsView, OdTvSelectionSet selectionSet, bool highlight)
    {
        using var iterator = selectionSet.getIterator();
        for (; iterator != null && !iterator.done(); iterator.step())
        {
            // get entity
            using var entityId = iterator.getEntity();
            // get sub item
            using var path = new OdTvSubItemPath();
            iterator.getPath(path);
            // perform highlight/de-highlight
            odTvGsView.highlight(entityId, path, highlight);
        }
    }
}