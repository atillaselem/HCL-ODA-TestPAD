/////////////////////////////////////////////////////////////////////////////// 
// Copyright (C) 2002-2022, Open Design Alliance (the "Alliance"). 
// All rights reserved. 
// 
// This software and its documentation and related materials are owned by 
// the Alliance. The software may only be incorporated into application 
// programs owned by members of the Alliance, subject to a signed 
// Membership Agreement and Supplemental Software License Agreement with the
// Alliance. The structure and organization of this software are the valuable  
// trade secrets of the Alliance and its suppliers. The software is also 
// protected by copyright law and international treaty provisions. Application  
// programs incorporating this software must include the following statement 
// with their copyright notices:
//   
//   This application incorporates Open Design Alliance software pursuant to a license 
//   agreement with Open Design Alliance.
//   Open Design Alliance Copyright (C) 2002-2022 by Open Design Alliance. 
//   All rights reserved.
//
// By use of this software, its documentation or related materials, you 
// acknowledge and accept the above terms.
///////////////////////////////////////////////////////////////////////////////
using HCL_ODA_TestPAD.Dialogs;
using System.Windows.Controls;
using System.Windows;
using MessageBox = System.Windows.Forms.MessageBox;
using HCL_ODA_TestPAD.ViewModels.Base;
using ODA.Kernel.TD_RootIntegrated;
using ODA.Visualize.TV_Visualize;

namespace HCL_ODA_TestPAD.ODA.ModelBrowser;

class TvModelProperties : BasePaletteProperties
{
    private OdTvModelId _modelId;

    public TvModelProperties(OdTvModelId modelId, int countOfChild, OdTvGsDeviceId devId, IOdaSectioning renderArea)
        : base(devId, renderArea)
    {
        _modelId = modelId;
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvModel model = _modelId.openObject();
        int row = 0;
        AddLabelAndTextBox("Name:", model.getName(), MainGrid, new[] { row, 0, row++, 1 }, true);
        AddLabelAndTextBox("Number of entities:", countOfChild.ToString(), MainGrid, new[] { row, 0, row++, 1 }, true);
        Button btn = AddLabelAndButton("Show statistic", "...", MainGrid, new[] { row, 0, row, 1 });
        btn.Click += ShowStatBtn_Click;
        _mm.StopTransaction(mtr);
    }

    private void ShowStatBtn_Click(object sender, RoutedEventArgs e)
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvModel model = _modelId.openObject();
        OdTvGeometryStatistic stat = model.getStatistic();

        StackPanel panel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };

        bool isEmpty = true;

        if (stat.getCount(OdTvGeometryStatistic_Types.kEntity) > 0 || stat.getCount(OdTvGeometryStatistic_Types.kLight) > 0
            || stat.getCount(OdTvGeometryStatistic_Types.kInsert) > 0)
        {
            isEmpty = false;
            GroupBox gb = new GroupBox { Header = "Entity objects" };
            Grid grid = CreateGrid(2, 3);
            panel.Children.Add(gb);
            int row = 0;
            if (stat.getCount(OdTvGeometryStatistic_Types.kEntity) > 0)
                AddLabelAndTextBox("Number of entities:", stat.getCount(OdTvGeometryStatistic_Types.kEntity).ToString(), grid, new[] { row, 0, row++, 1 }, true);
            if (stat.getCount(OdTvGeometryStatistic_Types.kLight) > 0)
                AddLabelAndTextBox("Number of lights:", stat.getCount(OdTvGeometryStatistic_Types.kLight).ToString(), grid, new[] { row, 0, row++, 1 }, true);
            if (stat.getCount(OdTvGeometryStatistic_Types.kInsert) > 0)
                AddLabelAndTextBox("Number of inserts:", stat.getCount(OdTvGeometryStatistic_Types.kInsert).ToString(), grid, new[] { row, 0, row++, 1 }, true);
            gb.Content = grid;
        }

        ulong sum = 0;
        for (uint i = (uint)OdTvGeometryStatistic_Types.kPolyline; i < (uint)OdTvGeometryStatistic_Types.kPoints; i++)
            sum += stat.getCount((OdTvGeometryStatistic_Types)i);

        if (sum > 0)
        {
            isEmpty = false;
            GroupBox gb = new GroupBox { Header = "Geometry objects" };
            Grid grid = CreateGrid(2, 20);
            panel.Children.Add(gb);
            gb.Content = grid;
            int row = 0;
            if (stat.getCount(OdTvGeometryStatistic_Types.kPolyline) > 0)
                AddLabelAndTextBox("Number of polylines:", stat.getCount(OdTvGeometryStatistic_Types.kPolyline).ToString(), grid, new[] { row, 0, row++, 1 }, true);
            if (stat.getCount(OdTvGeometryStatistic_Types.kCircle) > 0)
                AddLabelAndTextBox("Number of circles:", stat.getCount(OdTvGeometryStatistic_Types.kCircle).ToString(), grid, new[] { row, 0, row++, 1 }, true);
            if (stat.getCount(OdTvGeometryStatistic_Types.kCircleWedge) > 0)
                AddLabelAndTextBox("Number of circle wedges:", stat.getCount(OdTvGeometryStatistic_Types.kCircleWedge).ToString(), grid, new[] { row, 0, row++, 1 }, true);
            if (stat.getCount(OdTvGeometryStatistic_Types.kCircularArc) > 0)
                AddLabelAndTextBox("Number of circle arcs:", stat.getCount(OdTvGeometryStatistic_Types.kCircularArc).ToString(), grid, new[] { row, 0, row++, 1 }, true);
            if (stat.getCount(OdTvGeometryStatistic_Types.kEllipse) > 0)
                AddLabelAndTextBox("Number of ellipses:", stat.getCount(OdTvGeometryStatistic_Types.kEllipse).ToString(), grid, new[] { row, 0, row++, 1 }, true);
            if (stat.getCount(OdTvGeometryStatistic_Types.kEllipticArc) > 0)
                AddLabelAndTextBox("Number of elliptic arcs:", stat.getCount(OdTvGeometryStatistic_Types.kEllipticArc).ToString(), grid, new[] { row, 0, row++, 1 }, true);
            if (stat.getCount(OdTvGeometryStatistic_Types.kPolygon) > 0)
                AddLabelAndTextBox("Number of polygons:", stat.getCount(OdTvGeometryStatistic_Types.kPolygon).ToString(), grid, new[] { row, 0, row++, 1 }, true);
            if (stat.getCount(OdTvGeometryStatistic_Types.kText) > 0)
                AddLabelAndTextBox("Number of text:", stat.getCount(OdTvGeometryStatistic_Types.kText).ToString(), grid, new[] { row, 0, row++, 1 }, true);
            if (stat.getCount(OdTvGeometryStatistic_Types.kShell) > 0)
                AddLabelAndTextBox("Number of shells:", stat.getCount(OdTvGeometryStatistic_Types.kShell).ToString(), grid, new[] { row, 0, row++, 1 }, true);
            if (stat.getCount(OdTvGeometryStatistic_Types.kSphere) > 0)
                AddLabelAndTextBox("Number of spheres:", stat.getCount(OdTvGeometryStatistic_Types.kSphere).ToString(), grid, new[] { row, 0, row++, 1 }, true);
            if (stat.getCount(OdTvGeometryStatistic_Types.kCylinder) > 0)
                AddLabelAndTextBox("Number of cylinders:", stat.getCount(OdTvGeometryStatistic_Types.kCylinder).ToString(), grid, new[] { row, 0, row++, 1 }, true);
            if (stat.getCount(OdTvGeometryStatistic_Types.kGeomInsert) > 0)
                AddLabelAndTextBox("Number of inserts:", stat.getCount(OdTvGeometryStatistic_Types.kGeomInsert).ToString(), grid, new[] { row, 0, row++, 1 }, true);
            if (stat.getCount(OdTvGeometryStatistic_Types.kSubEntity) > 0)
                AddLabelAndTextBox("Number of sub entities:", stat.getCount(OdTvGeometryStatistic_Types.kSubEntity).ToString(), grid, new[] { row, 0, row++, 1 }, true);
            if (stat.getCount(OdTvGeometryStatistic_Types.kNurbs) > 0)
                AddLabelAndTextBox("Number of nurbs:", stat.getCount(OdTvGeometryStatistic_Types.kNurbs).ToString(), grid, new[] { row, 0, row++, 1 }, true);
            if (stat.getCount(OdTvGeometryStatistic_Types.kRasterImage) > 0)
                AddLabelAndTextBox("Number of raster images:", stat.getCount(OdTvGeometryStatistic_Types.kRasterImage).ToString(), grid, new[] { row, 0, row++, 1 }, true);
            if (stat.getCount(OdTvGeometryStatistic_Types.kInfiniteLine) > 0)
                AddLabelAndTextBox("Number of infinite lines:", stat.getCount(OdTvGeometryStatistic_Types.kInfiniteLine).ToString(), grid, new[] { row, 0, row++, 1 }, true);
            if (stat.getCount(OdTvGeometryStatistic_Types.kMesh) > 0)
                AddLabelAndTextBox("Number of meshes:", stat.getCount(OdTvGeometryStatistic_Types.kMesh).ToString(), grid, new[] { row, 0, row++, 1 }, true);
            if (stat.getCount(OdTvGeometryStatistic_Types.kPointCloud) > 0)
                AddLabelAndTextBox("Number of point clouds:", stat.getCount(OdTvGeometryStatistic_Types.kPointCloud).ToString(), grid, new[] { row, 0, row++, 1 }, true);
            if (stat.getCount(OdTvGeometryStatistic_Types.kGrid) > 0)
                AddLabelAndTextBox("Number of grids:", stat.getCount(OdTvGeometryStatistic_Types.kGrid).ToString(), grid, new[] { row, 0, row++, 1 }, true);
            if (stat.getCount(OdTvGeometryStatistic_Types.kColoredShape) > 0)
                AddLabelAndTextBox("Number of colored shapes:", stat.getCount(OdTvGeometryStatistic_Types.kColoredShape).ToString(), grid, new[] { row, 0, row++, 1 }, true);
        }

        if (stat.getCount(OdTvGeometryStatistic_Types.kFace) > 0 || stat.getCount(OdTvGeometryStatistic_Types.kPoints) > 0)
        {
            isEmpty = false;
            GroupBox gb = new GroupBox { Header = "Geometry info" };
            Grid grid = CreateGrid(2, 2);
            gb.Content = grid;
            panel.Children.Add(gb);
            int row = 0;
            if (stat.getCount(OdTvGeometryStatistic_Types.kPoints) > 0)
                AddLabelAndTextBox("Number of points:", stat.getCount(OdTvGeometryStatistic_Types.kPoints).ToString(), grid, new[] { row, 0, row++, 1 }, true);
            if (stat.getCount(OdTvGeometryStatistic_Types.kFace) > 0)
                AddLabelAndTextBox("Number of faces:", stat.getCount(OdTvGeometryStatistic_Types.kFace).ToString(), grid, new[] { row, 0, row++, 1 }, true);
        }

        if (isEmpty)
        {
            MessageBox.Show("Model statistic", "Model is empty.", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            _mm.StopTransaction(mtr);
            return;
        }

        CreateDialog("Model statistic", new Size(300, 300), panel).ShowDialog();

        _mm.StopTransaction(mtr);
    }
}
