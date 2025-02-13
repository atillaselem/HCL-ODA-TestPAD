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
using HCL_ODA_TestPAD.ViewModels.Base;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ODA.Kernel.TD_RootIntegrated;
using ODA.Visualize.TV_Visualize;

namespace HCL_ODA_TestPAD.ODA.ModelBrowser;

class TvCylinderProperties : TvBaseGeometryProperties
{
    private enum TypeOfPropety
    {
        Points,
        Radiuses
    }

    private TypeOfPropety _type;

    private OdGePoint3dVector _pointArr;
    private OdDoubleArray _radii;
    private int _countOfObjectsForLoad = 200;
    private bool _isChanged = false;
    private int _countOfLoadedObjects = 0;
    private UIElement _currentPanel;
    private bool _isScrollableControl;

    public TvCylinderProperties(OdTvGeometryDataId geomId, OdTvGsDeviceId devId, IOdaSectioning renderArea)
        : base(geomId, devId, renderArea)
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvCylinderData cyl = GeomId.openAsCylinder();
        int row = 0;
        Button pnts = AddLabelAndButton("Points", "...", MainGrid, new[] { row, 0, row++, 1 });
        pnts.Click += ShowPoints_Click;
        Button rad = AddLabelAndButton("Radiuses", "...", MainGrid, new[] { row, 0, row++, 1 });
        rad.Click += ShowRadiuses_Click;
        List<string> list = new List<string>() { "None", "First", "Second", "Both" };
        ComboBox caps = AddLabelAndComboBox("Caps:", list, (int)cyl.getCaps(), MainGrid, new[] { row, 0, row++, 1 });
        caps.SelectionChanged += Caps_SelectionChanged;

        StretchingTreeViewItem cmn = AddTreeItem("Common properties", MainGrid, new[] { row, 0 });
        GetProperties(cmn);
        _mm.StopTransaction(mtr);

    }

    private void Caps_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ComboBox cb = sender as ComboBox;
        if (cb == null)
            return;
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvCylinderData cyl = GeomId.openAsCylinder();
        cyl.setCaps((OdTvCylinderData_Capping)cb.SelectedIndex);
        Update();
        _mm.StopTransaction(mtr);
    }

    protected override void ScrollDialog_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        ScrollViewer sv = sender as ScrollViewer;
        if (sv == null)
            return;

        double max = _isScrollableControl ? sv.ScrollableHeight : _countOfLoadedObjects * 26;
        double cur = sv.VerticalOffset;
        if (cur / max * 100 >= 90)
        {
            switch (_type)
            {
                case TypeOfPropety.Points:
                    LoadPoints();
                    break;
                case TypeOfPropety.Radiuses:
                    LoadRadiuses();
                    break;
            }
        }
    }


    private void ShowPoints_Click(object sender, RoutedEventArgs e)
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvCylinderData cyl = GeomId.openAsCylinder();
        _pointArr = cyl.getPoints();

        if (!CheckCountOfObject(_pointArr.Count))
        {
            _pointArr.Clear();
            _countOfLoadedObjects = 0;
            _mm.StopTransaction(mtr);
            return;
        }

        _currentPanel = new StretchingTreeView()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch,
            Margin = new Thickness(2, 2, 0, 2)
        };
        _type = TypeOfPropety.Points;
        _isScrollableControl = true;
        LoadPoints();

        if (CreateDialog("Cylinder points", new Size(300, 300), _currentPanel).ShowDialog() == true && _isChanged)
        {
            cyl.setPoints(_pointArr);
            Update();
        }
        _pointArr.Clear();
        _countOfLoadedObjects = 0;
        _mm.StopTransaction(mtr);
    }

    private void LoadPoints()
    {
        for (int i = 0; i < _countOfObjectsForLoad; i++, _countOfLoadedObjects++)
        {
            if (_countOfLoadedObjects >= _pointArr.Count)
                return;
            StretchingTreeViewItem itm = AddTreeItem("Point_" + _countOfLoadedObjects, (StretchingTreeView)_currentPanel);
            itm.Tag = _countOfLoadedObjects;
            itm.Items.Add(null);
            itm.Expanded += Point_Expanded;
        }
    }

    private void Point_Expanded(object sender, RoutedEventArgs e)
    {
        StretchingTreeViewItem itm = sender as StretchingTreeViewItem;
        if (itm == null)
            return;
        itm.Items.Clear();
        TextBox[] pnt = GetXyz(itm, _pointArr[(int)itm.Tag]);
        foreach (var p in pnt)
        {
            p.LostKeyboardFocus += Point_LostKeyboardFocus;
            p.Margin = new Thickness(0, 0, 0, 2);
        }
    }

    private void Point_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
    {
        TextBox tb = sender as TextBox;
        if (tb == null)
            return;
        Grid grid = (Grid)tb.Parent;
        StretchingTreeViewItem parent = (StretchingTreeViewItem)grid.Parent;
        int ind = (int)parent.Tag;
        CoordinateType type = (CoordinateType)tb.Tag;
        OdGePoint3d newPt = SetPointCoordByType(_pointArr[ind], type, tb.Text);
        if (newPt != _pointArr[ind])
        {
            _pointArr[ind] = newPt;
            if (!_isChanged) _isChanged = true;
        }
    }


    private void ShowRadiuses_Click(object sender, RoutedEventArgs e)
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvCylinderData cyl = GeomId.openAsCylinder();
        _radii = cyl.getRadii();

        if (!CheckCountOfObject(_radii.Count))
        {
            _radii.Clear();
            _countOfLoadedObjects = 0;
            _mm.StopTransaction(mtr);
            return;
        }

        _type = TypeOfPropety.Radiuses;

        _currentPanel = CreateGrid(2, 0);
        _isScrollableControl = false;
        LoadRadiuses();

        if (CreateDialog("Shell faces", new Size(300, 300), _currentPanel).ShowDialog() == true && _isChanged)
        {
            cyl.setRadii(_radii);
            Update();
        }
        _radii.Clear();
        _countOfLoadedObjects = 0;
        _mm.StopTransaction(mtr);
    }

    private void LoadRadiuses()
    {
        for (int i = 0; i < _countOfObjectsForLoad; i++, _countOfLoadedObjects++)
        {
            if (_countOfLoadedObjects >= _radii.Count)
                return;

            Grid grid = (Grid)_currentPanel;
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(26) });
            TextBox txtBox = AddLabelAndTextBox("Radius_" + _countOfLoadedObjects, _radii[_countOfLoadedObjects].ToString(),
                grid, new[] { _countOfLoadedObjects, 0, _countOfLoadedObjects, 1 });
            txtBox.Margin = new Thickness(0, 0, 0, 2);
            txtBox.Tag = _countOfLoadedObjects;
            txtBox.LostKeyboardFocus += Radius_LostKeyboardFocus;
        }
    }

    private void Radius_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
    {
        TextBox tb = sender as TextBox;
        if (tb == null)
            return;
        int ind = (int)tb.Tag;
        if (!_radii[ind].Equals(double.Parse(tb.Text)))
        {
            _radii[ind] = double.Parse(tb.Text);
            if (!_isChanged) _isChanged = true;
        }
    }


}
