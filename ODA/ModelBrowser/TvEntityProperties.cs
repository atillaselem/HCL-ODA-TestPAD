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
using HCL_ODA_TestPAD.ViewModels.Base;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ODA.Kernel.TD_RootIntegrated;
using ODA.Visualize.TV_Visualize;

namespace HCL_ODA_TestPAD.ODA.ModelBrowser;

class TvEntityProperties : BasePaletteProperties
{
    private OdTvEntityId _enId;
    private OdTvGeometryDataId _gEnId;

    private enum ComboboxType
    {
        TextStyle,
        Material,
        MapperAutoTransform,
        MapperProjection,
        TargetDisplayMode
    }

    public TvEntityProperties(OdTvGeometryDataId enId, int countOfChild, OdTvGsDeviceId devId, IOdaSectioning renderArea)
        : base(devId, renderArea)
    {
        _gEnId = enId;
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvEntity en = _gEnId.openAsSubEntity();
        FillEntityInfo(en);
        _mm.StopTransaction(mtr);
    }

    public TvEntityProperties(OdTvEntityId enId, int countOfChild, OdTvGsDeviceId devId, IOdaSectioning renderArea)
    : base(devId, renderArea)
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        _enId = enId;
        OdTvEntity en = enId.openObject();
        FillEntityInfo(en);
        _mm.StopTransaction(mtr);
    }

    private void FillEntityInfo(OdTvEntity en)
    {
        int row = 0;
        FillEntitiesColors(MainGrid, new[] { row, 0, row, 1, row++, 2 });
        FillEntitiesLineweight(MainGrid, new[] { row, 0, row, 1, row++, 2 });
        FillEntitiesLinetype(MainGrid, new[] { row, 0, row, 1, row++, 2 });
        FillEntitiesTransparency(MainGrid, new[] { row, 0, row, 1, row++, 2 });
        FillEntitiesLayer(MainGrid, new[] { row, 0, row, 1, row++, 2 });
        ComboBox txtStyle = AddTextStyleDef("TextStyle:", en.getTextStyle(), MainGrid, new[] { row, 0, row++, 1 });
        txtStyle.SetValue(Grid.ColumnSpanProperty, 2);
        txtStyle.SelectionChanged += Combobox_SelectionChanged;
        txtStyle.Tag = ComboboxType.TextStyle;
        ComboBox mat = AddMatrialDef("Material:", en.getMaterial(), MainGrid, new[] { row, 0, row++, 1 });
        mat.SetValue(Grid.ColumnSpanProperty, 2);
        mat.SelectionChanged += Combobox_SelectionChanged;
        mat.Tag = ComboboxType.Material;
        AddMapper("Mapper", en.getMaterialMapper(), MainGrid, new[] { row, 0, row++, 1 });
        FillEntitiesVisibility(MainGrid, new[] { row, 0, row, 1, row++, 2 });
        List<TextBox> matrix = AddMatrix("Modeling matrix", en.getModelingMatrix(), MainGrid, new[] { row++, 0 });
        matrix.ForEach(o => o.LostKeyboardFocus += ModelingMatrix_LostKeyboardFocus);
        List<string> dispList = new List<string>() { "EveryWhere", "Wirefame", "Render" };
        ComboBox dispMode = AddLabelAndComboBox("Target display mode:", dispList, (int)en.getTargetDisplayMode(), MainGrid, new[] { row, 0, row++, 1 });
        dispMode.Tag = ComboboxType.TargetDisplayMode;
        dispMode.SelectionChanged += Combobox_SelectionChanged;
        FillEntitiesLinetypeScale(MainGrid, new[] { row, 0, row, 1, row++, 2 });
        CheckBox autoregen = AddLabelAndCheckBox("Auto regen:", en.getAutoRegen(), MainGrid, new[] { row, 0, row++, 1 });
        autoregen.Click += Autoregen_Click;

        StretchingTreeViewItem misc = AddTreeItem("Misc", MainGrid, new[] { row, 0 });
        int geom = 0, subEnt = 0;
        for (OdTvGeometryDataIterator it = en.getGeometryDataIterator(); !it.done(); it.step())
        {
            if (it.getGeometryData().getType() == OdTv_OdTvGeometryDataType.kSubEntity)
                subEnt++;
            else
                geom++;
        }
        AddLabelAndTextBox("Count of geomtries:", geom.ToString(), misc, true);
        AddLabelAndTextBox("Count of subentities:", subEnt.ToString(), misc, true);
    }

    private void Autoregen_Click(object sender, RoutedEventArgs e)
    {
        CheckBox cb = sender as CheckBox;
        if (cb == null)
            return;
        MemoryTransaction mtr = _mm.StartTransaction();
        if (_enId != null && !_enId.isNull())
            _enId.openObject(OdTv_OpenMode.kForWrite).setAutoRegen(cb.IsChecked == true);
        if (_gEnId != null && !_gEnId.isNull())
            _gEnId.openAsSubEntity().setAutoRegen(cb.IsChecked == true);
        Update();
        _mm.StopTransaction(mtr);
    }

    private void AddMapper(string label, OdTvMapperDef mapper, Grid grid, int[] arr)
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        StretchingTreeViewItem itm = AddTreeItem(label, grid, arr);
        if (mapper.getType() == OdTvMapperDef_MapperType.kDefault)
            itm.Foreground = new SolidColorBrush(Colors.Gray);
        List<string> autoTrList = new List<string>() { "None", "Object", "Model" };
        ComboBox autoTr = AddLabelAndComboBox("Auto transform:", autoTrList, mapper.autoTransform().ToString().Remove(0, 1), itm);
        autoTr.Tag = ComboboxType.MapperAutoTransform;
        autoTr.SelectionChanged += Combobox_SelectionChanged;
        List<string> projList = new List<string>() { "Planar", "Box", "Cylinder", "Sphere" };
        ComboBox proj = AddLabelAndComboBox("Projection:", projList, (int)mapper.projection(), itm);
        proj.Tag = ComboboxType.MapperProjection;
        proj.SelectionChanged += Combobox_SelectionChanged;
        List<TextBox> matrix = AddMatrix("Transform", mapper.transform(), itm);
        matrix.ForEach(o => o.LostKeyboardFocus += MapperTransform_LostKeyboardFocus);
        _mm.StopTransaction(mtr);
    }

    private void ModelingMatrix_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
    {
        DoubleTextBox dtb = sender as DoubleTextBox;
        if (dtb == null)
            return;
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvEntity en = _enId.openObject(OdTv_OpenMode.kForWrite) ?? _gEnId.openAsSubEntity();
        OdGeMatrix3d matr = en.getModelingMatrix();
        SetMatrix(matr, (MatrixData)dtb.Tag, dtb.Text);
        en.setModelingMatrix(matr);
        Update();
        _mm.StopTransaction(mtr);
    }

    private void MapperTransform_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
    {
        DoubleTextBox dtb = sender as DoubleTextBox;
        if (dtb == null)
            return;
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvEntity en = _enId.openObject(OdTv_OpenMode.kForWrite) ?? _gEnId.openAsSubEntity();
        OdGeMatrix3d matr = en.getMaterialMapper().transform();
        SetMatrix(matr, (MatrixData)dtb.Tag, dtb.Text);
        OdTvMapperDef mapper = en.getMaterialMapper();
        mapper.setTransform(matr);
        en.setMaterialMapper(mapper);
        Update();
        _mm.StopTransaction(mtr);
    }

    private void Combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ComboBox cb = sender as ComboBox;
        if (cb == null)
            return;
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvEntity en = _enId.openObject(OdTv_OpenMode.kForWrite) ?? _gEnId.openAsSubEntity();
        switch ((ComboboxType)cb.Tag)
        {
            case ComboboxType.TextStyle:
                if (GetTextStyleName(en.getTextStyle()) != cb.SelectedItem.ToString())
                    en.setTextStyle(GetTextStyleDef(cb.SelectedItem.ToString()));
                break;
            case ComboboxType.Material:
                if (GetMaterialName(en.getMaterial()) != cb.SelectedItem.ToString())
                    en.setMaterial(GetMaterialDef(cb.SelectedItem.ToString()));
                break;
            case ComboboxType.MapperAutoTransform:
                {
                    OdTvMapperDef mapper = en.getMaterialMapper();
                    int newInd = cb.SelectedIndex == 0 ? 1 : cb.SelectedIndex == 1 ? 2 : 4;
                    if (mapper.autoTransform() != (OdTvMapperDef_AutoTransform)newInd)
                    {
                        mapper.setAutoTransform((OdTvMapperDef_AutoTransform)newInd);
                        en.setMaterialMapper(mapper);
                    }
                    break;
                }
            case ComboboxType.MapperProjection:
                {
                    OdTvMapperDef mapper = en.getMaterialMapper();
                    if (mapper.projection() != (OdTvMapperDef_Projection)cb.SelectedIndex)
                    {
                        mapper.setProjection((OdTvMapperDef_Projection)cb.SelectedIndex);
                        en.setMaterialMapper(mapper);
                    }
                    break;
                }
            case ComboboxType.TargetDisplayMode:
                if (en.getTargetDisplayMode() != (OdTvGeometryData_TargetDisplayMode)cb.SelectedIndex)
                    en.setTargetDisplayMode((OdTvGeometryData_TargetDisplayMode)cb.SelectedIndex);
                break;
        }

        Update();
        _mm.StopTransaction(mtr);
    }

    private Button CreateButton()
    {
        return new Button()
        {
            Content = "...",
            Margin = new Thickness(5, 0, 2, 0),
            HorizontalAlignment = HorizontalAlignment.Right,
            Width = 30,
            Height = 25,
            VerticalAlignment = VerticalAlignment.Center
        };
    }

    private void SetGridWithElements(Grid grid, int[] arr, params UIElement[] uiElements)
    {
        int ind = 0;
        foreach (var p in uiElements)
        {
            Grid.SetRow(p, arr[ind++]);
            Grid.SetColumn(p, arr[ind++]);
            grid.Children.Add(p);
        }
    }

    #region Entity color

    private void FillEntitiesColors(Grid grid, int[] arr)
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvEntity en = _enId.openObject() ?? _gEnId.openAsSubEntity();
        OdTvColorDef colorAll = en.getColor();
        Label lbl = new Label() { Content = "Color:" };

        Colorpicker clrp = new Colorpicker(colorAll, TvDeviceId) { HorizontalAlignment = HorizontalAlignment.Stretch };
        clrp.ColorChanged += Color_ColorChanged;
        if (colorAll.getType() == OdTvColorDef_ColorType.kDefault)
            lbl.Foreground = new SolidColorBrush() { Color = Colors.Gray };

        Button btn = CreateButton();
        btn.Click += ShowEntitiesColors_Click;

        SetGridWithElements(grid, arr, lbl, clrp, btn);

        _mm.StopTransaction(mtr);
    }

    private void Color_ColorChanged(object sender, OdTvColorDef newColor)
    {
        Colorpicker cp = sender as Colorpicker;
        if (cp == null)
            return;
        MemoryTransaction mtr = _mm.StartTransaction();
        if (_enId != null && !_enId.isNull())
            _enId.openObject(OdTv_OpenMode.kForWrite).setColor(newColor);
        if (_gEnId != null && !_gEnId.isNull())
            _gEnId.openAsSubEntity().setColor(newColor);
        Update();
        _mm.StopTransaction(mtr);
    }

    private Grid CreateColorLayout(string label, OdTvColorDef color, out Colorpicker clrp)
    {
        Grid grid = new Grid() { Margin = new Thickness(2) };
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());

        Label lbl = new Label() { Content = label };
        Grid.SetColumn(lbl, 0);
        clrp = new Colorpicker(color, TvDeviceId);
        if (color.getType() == OdTvColorDef_ColorType.kDefault)
            lbl.Foreground = new SolidColorBrush() { Color = Colors.Gray };
        Grid.SetColumn(clrp, 1);

        grid.Children.Add(lbl);
        grid.Children.Add(clrp);

        return grid;
    }

    private void ShowEntitiesColors_Click(object sender, RoutedEventArgs e)
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvEntity en = _enId.openObject() ?? _gEnId.openAsSubEntity();
        StackPanel panel = new StackPanel() { Margin = new Thickness(10), VerticalAlignment = VerticalAlignment.Stretch };
        Colorpicker all, polylines, edges, faces, vertex, text;
        panel.Children.Add(CreateColorLayout("All:", en.getColor(OdTvGeometryData_GeometryTypes.kAll), out all));
        panel.Children.Add(CreateColorLayout("Polylines:", en.getColor(OdTvGeometryData_GeometryTypes.kPolylines), out polylines));
        panel.Children.Add(CreateColorLayout("Edges:", en.getColor(OdTvGeometryData_GeometryTypes.kEdges), out edges));
        panel.Children.Add(CreateColorLayout("Faces:", en.getColor(OdTvGeometryData_GeometryTypes.kFaces), out faces));
        panel.Children.Add(CreateColorLayout("Vertices:", en.getColor(OdTvGeometryData_GeometryTypes.kVertices), out vertex));
        panel.Children.Add(CreateColorLayout("Text:", en.getColor(OdTvGeometryData_GeometryTypes.kText), out text));
        if (CreateDialog("Entity colors", new Size(270, 280), panel).ShowDialog() == true)
        {
            if (all.SelectedColor != en.getColor(OdTvGeometryData_GeometryTypes.kAll))
                en.setColor(all.SelectedColor, (ushort)OdTvGeometryData_GeometryTypes.kAll);
            if (polylines.SelectedColor != en.getColor(OdTvGeometryData_GeometryTypes.kPolylines))
                en.setColor(polylines.SelectedColor, (ushort)OdTvGeometryData_GeometryTypes.kPolylines);
            if (edges.SelectedColor != en.getColor(OdTvGeometryData_GeometryTypes.kEdges))
                en.setColor(edges.SelectedColor, (ushort)OdTvGeometryData_GeometryTypes.kEdges);
            if (faces.SelectedColor != en.getColor(OdTvGeometryData_GeometryTypes.kFaces))
                en.setColor(faces.SelectedColor, (ushort)OdTvGeometryData_GeometryTypes.kFaces);
            if (vertex.SelectedColor != en.getColor(OdTvGeometryData_GeometryTypes.kVertices))
                en.setColor(vertex.SelectedColor, (ushort)OdTvGeometryData_GeometryTypes.kVertices);
            if (text.SelectedColor != en.getColor(OdTvGeometryData_GeometryTypes.kText))
                en.setColor(text.SelectedColor, (ushort)OdTvGeometryData_GeometryTypes.kText);
            Update();
        }
        _mm.StopTransaction(mtr);
    }

    #endregion

    #region Entity lineweight

    private void FillEntitiesLineweight(Grid grid, int[] arr)
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvEntity en = _enId.openObject() ?? _gEnId.openAsSubEntity();
        OdTvLineWeightDef lwDef = en.getLineWeight();
        Label lbl = new Label() { Content = "Lineweight:" };

        IntTextBox tb = new IntTextBox { VerticalContentAlignment = VerticalAlignment.Center, Text = lwDef.getValue().ToString() };
        tb.LostKeyboardFocus += Lineweight_LostKeyboardFocus;
        if (lwDef.getType() == OdTvLineWeightDef_LineWeightType.kDefault)
            lbl.Foreground = new SolidColorBrush() { Color = Colors.Gray };

        Button btn = CreateButton();
        btn.Click += ShowEntitiesLineweights_Click;

        SetGridWithElements(grid, arr, lbl, tb, btn);
        _mm.StopTransaction(mtr);
    }

    private void Lineweight_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
    {
        IntTextBox iTb = sender as IntTextBox;
        if (iTb == null)
            return;
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvEntity en = _enId.openObject(OdTv_OpenMode.kForWrite) ?? _gEnId.openAsSubEntity(); ;
        if (en.getLineWeight().getValue() != byte.Parse(iTb.Text))
        {
            en.setLineWeight(new OdTvLineWeightDef(byte.Parse(iTb.Text)));
            Update();
        }
        _mm.StopTransaction(mtr);
    }

    private Grid CreateTextLayout(string label, OdTvLineWeightDef lwDef, out IntTextBox tb)
    {
        Grid grid = new Grid() { Margin = new Thickness(2) };
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());

        Label lbl = new Label() { Content = label };
        Grid.SetColumn(lbl, 0);
        tb = new IntTextBox { VerticalContentAlignment = VerticalAlignment.Center, Text = lwDef.getValue().ToString() };
        if (lwDef.getType() == OdTvLineWeightDef_LineWeightType.kDefault)
            lbl.Foreground = new SolidColorBrush() { Color = Colors.Gray };
        Grid.SetColumn(tb, 1);

        grid.Children.Add(lbl);
        grid.Children.Add(tb);

        return grid;
    }

    private void ShowEntitiesLineweights_Click(object sender, RoutedEventArgs e)
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvEntity en = _enId.openObject() ?? _gEnId.openAsSubEntity();
        StackPanel panel = new StackPanel() { Margin = new Thickness(10), VerticalAlignment = VerticalAlignment.Stretch };
        IntTextBox all, polylines, edges;
        panel.Children.Add(CreateTextLayout("All:", en.getLineWeight(OdTvGeometryData_GeometryTypes.kAll), out all));
        panel.Children.Add(CreateTextLayout("Polylines:", en.getLineWeight(OdTvGeometryData_GeometryTypes.kPolylines), out polylines));
        panel.Children.Add(CreateTextLayout("Edges:", en.getLineWeight(OdTvGeometryData_GeometryTypes.kEdges), out edges));
        if (CreateDialog("Entity lineweigts", new Size(270, 190), panel).ShowDialog() == true)
        {
            all.Text = all.Text.Replace(".", ",");
            byte val = byte.Parse(all.Text);
            if (!en.getLineWeight(OdTvGeometryData_GeometryTypes.kAll).getValue().Equals(val))
                en.setLineWeight(new OdTvLineWeightDef(val), (ushort)OdTvGeometryData_GeometryTypes.kAll);
            val = byte.Parse(polylines.Text);
            if (!en.getLineWeight(OdTvGeometryData_GeometryTypes.kPolylines).getValue().Equals(val))
                en.setLineWeight(new OdTvLineWeightDef(val), (ushort)OdTvGeometryData_GeometryTypes.kPolylines);
            val = byte.Parse(edges.Text);
            if (!en.getLineWeight(OdTvGeometryData_GeometryTypes.kEdges).getValue().Equals(val))
                en.setLineWeight(new OdTvLineWeightDef(val), (ushort)OdTvGeometryData_GeometryTypes.kEdges);
            Update();
        }
        _mm.StopTransaction(mtr);
    }


    #endregion

    #region Entity linetype

    private void FillEntitiesLinetype(Grid grid, int[] arr)
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvEntity en = _enId.openObject() ?? _gEnId.openAsSubEntity();
        OdTvLinetypeDef ltDef = en.getLinetype();
        Label lbl = new Label() { Content = "Linetype:" };

        ComboBox cb = AddLabelAndComboBox("", GetLinetypesList(), GetLinetypeName(ltDef), null);
        cb.SelectionChanged += Linetype_SelectionChanged;
        if (ltDef.getType() == OdTvLinetypeDef_LinetypeType.kDefault)
            lbl.Foreground = new SolidColorBrush() { Color = Colors.Gray };

        Button btn = CreateButton();
        btn.Click += ShowEntitiesLinetypes_Click;

        SetGridWithElements(grid, arr, lbl, cb, btn);
        _mm.StopTransaction(mtr);
    }

    private void Linetype_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ComboBox cb = sender as ComboBox;
        if (cb == null)
            return;
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvEntity en = _enId.openObject(OdTv_OpenMode.kForWrite) ?? _gEnId.openAsSubEntity(); ;
        if (GetLinetypeName(en.getLinetype()) != cb.SelectedItem.ToString())
        {
            en.setLinetype(GetLinetypeDef(cb.SelectedItem.ToString()));
            Update();
        }
        _mm.StopTransaction(mtr);
    }

    private Grid CreateComboboxLayout(string label, List<string> list, string text, bool isDef, out ComboBox cb)
    {
        Grid grid = new Grid() { Margin = new Thickness(2) };
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());

        Label lbl = new Label() { Content = label };
        Grid.SetColumn(lbl, 0);
        cb = AddLabelAndComboBox("", list, text, null, isDef);
        if (isDef)
            lbl.Foreground = new SolidColorBrush() { Color = Colors.Gray };
        Grid.SetColumn(cb, 1);

        grid.Children.Add(lbl);
        grid.Children.Add(cb);

        return grid;
    }

    private void ShowEntitiesLinetypes_Click(object sender, RoutedEventArgs e)
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvEntity en = _enId.openObject() ?? _gEnId.openAsSubEntity();

        StackPanel panel = new StackPanel() { Margin = new Thickness(10), VerticalAlignment = VerticalAlignment.Stretch };
        OdTvLinetypeDef ltDef = en.getLinetype(OdTvGeometryData_GeometryTypes.kAll);
        ComboBox all, polylines, edges;
        panel.Children.Add(CreateComboboxLayout("All:", GetLinetypesList(), GetLinetypeName(ltDef),
            ltDef.getType() == OdTvLinetypeDef_LinetypeType.kDefault, out all));
        ltDef = en.getLinetype(OdTvGeometryData_GeometryTypes.kPolylines);
        panel.Children.Add(CreateComboboxLayout("Polylines:", GetLinetypesList(), GetLinetypeName(ltDef),
            ltDef.getType() == OdTvLinetypeDef_LinetypeType.kDefault, out polylines));
        ltDef = en.getLinetype(OdTvGeometryData_GeometryTypes.kEdges);
        panel.Children.Add(CreateComboboxLayout("Edges:", GetLinetypesList(), GetLinetypeName(ltDef),
            ltDef.getType() == OdTvLinetypeDef_LinetypeType.kDefault, out edges));

        if (CreateDialog("Entity linetypes", new Size(270, 190), panel).ShowDialog() == true)
        {
            if (all.SelectedItem.ToString() != GetLinetypeName(en.getLinetype(OdTvGeometryData_GeometryTypes.kAll)))
                en.setLinetype(GetLinetypeDef(all.SelectedItem.ToString()), OdTvGeometryData_GeometryTypes.kAll);
            if (polylines.SelectedItem.ToString() != GetLinetypeName(en.getLinetype(OdTvGeometryData_GeometryTypes.kPolylines)))
                en.setLinetype(GetLinetypeDef(polylines.SelectedItem.ToString()), OdTvGeometryData_GeometryTypes.kPolylines);
            if (edges.SelectedItem.ToString() != GetLinetypeName(en.getLinetype(OdTvGeometryData_GeometryTypes.kEdges)))
                en.setLinetype(GetLinetypeDef(edges.SelectedItem.ToString()), OdTvGeometryData_GeometryTypes.kEdges);

            Update();
        }
        _mm.StopTransaction(mtr);
    }

    #endregion

    #region Entity transparency

    private void FillEntitiesTransparency(Grid grid, int[] arr)
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvEntity en = _enId.openObject() ?? _gEnId.openAsSubEntity();
        OdTvTransparencyDef trDef = en.getTransparency();
        Label lbl = new Label() { Content = "Transparency:" };

        DoubleTextBox tb = new DoubleTextBox { VerticalContentAlignment = VerticalAlignment.Center, Text = trDef.getValue().ToString() };
        tb.LostKeyboardFocus += Transparency_LostKeyboardFocus;
        if (trDef.getType() == OdTvTransparencyDef_TransparencyType.kDefault)
            lbl.Foreground = new SolidColorBrush() { Color = Colors.Gray };

        Button btn = CreateButton();
        btn.Click += ShowEntitiesTransparency_Click;
        SetGridWithElements(grid, arr, lbl, tb, btn);
        _mm.StopTransaction(mtr);
    }

    private void Transparency_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
    {
        DoubleTextBox dTb = sender as DoubleTextBox;
        if (dTb == null)
            return;
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvEntity en = _enId.openObject(OdTv_OpenMode.kForWrite) ?? _gEnId.openAsSubEntity(); ;
        if (!en.getTransparency().getValue().Equals(double.Parse(dTb.Text)))
        {
            en.setTransparency(new OdTvTransparencyDef(double.Parse(dTb.Text)));
            Update();
        }
        _mm.StopTransaction(mtr);
    }

    private Grid CreateTextLayout(string label, OdTvTransparencyDef trDef, out DoubleTextBox tb)
    {
        Grid grid = new Grid() { Margin = new Thickness(2) };
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());

        Label lbl = new Label() { Content = label };
        Grid.SetColumn(lbl, 0);
        tb = new DoubleTextBox { VerticalContentAlignment = VerticalAlignment.Center, Text = trDef.getValue().ToString() };
        if (trDef.getType() == OdTvTransparencyDef_TransparencyType.kDefault)
            lbl.Foreground = new SolidColorBrush() { Color = Colors.Gray };
        Grid.SetColumn(tb, 1);

        grid.Children.Add(lbl);
        grid.Children.Add(tb);

        return grid;
    }

    private void ShowEntitiesTransparency_Click(object sender, RoutedEventArgs e)
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvEntity en = _enId.openObject(OdTv_OpenMode.kForWrite) ?? _gEnId.openAsSubEntity(); ;
        StackPanel panel = new StackPanel() { Margin = new Thickness(10), VerticalAlignment = VerticalAlignment.Stretch };
        DoubleTextBox all, polylines, faces, text;
        panel.Children.Add(CreateTextLayout("All:", en.getTransparency(OdTvGeometryData_GeometryTypes.kAll), out all));
        panel.Children.Add(CreateTextLayout("Polylines:", en.getTransparency(OdTvGeometryData_GeometryTypes.kPolylines), out polylines));
        panel.Children.Add(CreateTextLayout("Faces:", en.getTransparency(OdTvGeometryData_GeometryTypes.kFaces), out faces));
        panel.Children.Add(CreateTextLayout("Text:", en.getTransparency(OdTvGeometryData_GeometryTypes.kText), out text));
        if (CreateDialog("Entity transparencies", new Size(270, 230), panel).ShowDialog() == true)
        {
            double val = double.Parse(all.Text);
            if (!en.getTransparency(OdTvGeometryData_GeometryTypes.kAll).getValue().Equals(val))
                en.setTransparency(new OdTvTransparencyDef(val), OdTvGeometryData_GeometryTypes.kAll);
            val = double.Parse(polylines.Text);
            if (!en.getTransparency(OdTvGeometryData_GeometryTypes.kPolylines).getValue().Equals(val))
                en.setTransparency(new OdTvTransparencyDef(val), OdTvGeometryData_GeometryTypes.kPolylines);
            val = double.Parse(faces.Text);
            if (!en.getTransparency(OdTvGeometryData_GeometryTypes.kFaces).getValue().Equals(val))
                en.setTransparency(new OdTvTransparencyDef(val), OdTvGeometryData_GeometryTypes.kFaces);
            val = double.Parse(text.Text);
            if (!en.getTransparency(OdTvGeometryData_GeometryTypes.kText).getValue().Equals(val))
                en.setTransparency(new OdTvTransparencyDef(val), OdTvGeometryData_GeometryTypes.kText);
            Update();
        }
        _mm.StopTransaction(mtr);
    }

    #endregion

    #region Entity layer

    private void FillEntitiesLayer(Grid grid, int[] arr)
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvEntity en = _enId.openObject() ?? _gEnId.openAsSubEntity();
        OdTvLayerDef lyDef = en.getLayer();
        Label lbl = new Label() { Content = "Layer:" };

        ComboBox cb = AddLabelAndComboBox("", GetLayersList(), GetLayerName(lyDef), null);
        cb.SelectionChanged += Layer_SelectionChanged;
        if (lyDef.getType() == OdTvLayerDef_LayerType.kDefault)
            lbl.Foreground = new SolidColorBrush() { Color = Colors.Gray };

        Button btn = CreateButton();
        btn.Click += ShowEntitiesLayers_Click;
        SetGridWithElements(grid, arr, lbl, cb, btn);
        _mm.StopTransaction(mtr);
    }

    private void Layer_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ComboBox cb = sender as ComboBox;
        if (cb == null)
            return;
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvEntity en = _enId.openObject(OdTv_OpenMode.kForWrite) ?? _gEnId.openAsSubEntity(); ;
        if (GetLayerName(en.getLayer()) != cb.SelectedItem.ToString())
        {
            en.setLayer(GetLayerDef(cb.SelectedItem.ToString()));
            Update();
        }
        _mm.StopTransaction(mtr);
    }

    private void ShowEntitiesLayers_Click(object sender, RoutedEventArgs e)
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvEntity en = _enId.openObject() ?? _gEnId.openAsSubEntity();

        StackPanel panel = new StackPanel() { Margin = new Thickness(10), VerticalAlignment = VerticalAlignment.Stretch };
        OdTvLayerDef lyDef = en.getLayer(OdTvGeometryData_GeometryTypes.kAll);
        ComboBox all, polylines, edges, faces, vertices, text;
        panel.Children.Add(CreateComboboxLayout("All:", GetLayersList(), GetLayerName(lyDef),
            lyDef.getType() == OdTvLayerDef_LayerType.kDefault, out all));
        lyDef = en.getLayer(OdTvGeometryData_GeometryTypes.kPolylines);
        panel.Children.Add(CreateComboboxLayout("Polylines:", GetLayersList(), GetLayerName(lyDef),
            lyDef.getType() == OdTvLayerDef_LayerType.kDefault, out polylines));
        lyDef = en.getLayer(OdTvGeometryData_GeometryTypes.kEdges);
        panel.Children.Add(CreateComboboxLayout("Edges:", GetLayersList(), GetLayerName(lyDef),
            lyDef.getType() == OdTvLayerDef_LayerType.kDefault, out edges));
        lyDef = en.getLayer(OdTvGeometryData_GeometryTypes.kFaces);
        panel.Children.Add(CreateComboboxLayout("Faces:", GetLayersList(), GetLayerName(lyDef),
            lyDef.getType() == OdTvLayerDef_LayerType.kDefault, out faces));
        lyDef = en.getLayer(OdTvGeometryData_GeometryTypes.kVertices);
        panel.Children.Add(CreateComboboxLayout("Vertices:", GetLayersList(), GetLayerName(lyDef),
            lyDef.getType() == OdTvLayerDef_LayerType.kDefault, out vertices));
        lyDef = en.getLayer(OdTvGeometryData_GeometryTypes.kText);
        panel.Children.Add(CreateComboboxLayout("Text:", GetLayersList(), GetLayerName(lyDef),
            lyDef.getType() == OdTvLayerDef_LayerType.kDefault, out text));

        if (CreateDialog("Entity layers", new Size(270, 280), panel).ShowDialog() == true)
        {
            if (all.SelectedItem.ToString() != GetLayerName(en.getLayer(OdTvGeometryData_GeometryTypes.kAll)))
                en.setLayer(GetLayerDef(all.SelectedItem.ToString()), OdTvGeometryData_GeometryTypes.kAll);
            if (polylines.SelectedItem.ToString() != GetLayerName(en.getLayer(OdTvGeometryData_GeometryTypes.kPolylines)))
                en.setLayer(GetLayerDef(polylines.SelectedItem.ToString()), OdTvGeometryData_GeometryTypes.kPolylines);
            if (edges.SelectedItem.ToString() != GetLayerName(en.getLayer(OdTvGeometryData_GeometryTypes.kEdges)))
                en.setLayer(GetLayerDef(edges.SelectedItem.ToString()), OdTvGeometryData_GeometryTypes.kEdges);
            if (faces.SelectedItem.ToString() != GetLayerName(en.getLayer(OdTvGeometryData_GeometryTypes.kFaces)))
                en.setLayer(GetLayerDef(faces.SelectedItem.ToString()), OdTvGeometryData_GeometryTypes.kFaces);
            if (vertices.SelectedItem.ToString() != GetLayerName(en.getLayer(OdTvGeometryData_GeometryTypes.kVertices)))
                en.setLayer(GetLayerDef(vertices.SelectedItem.ToString()), OdTvGeometryData_GeometryTypes.kVertices);
            if (text.SelectedItem.ToString() != GetLayerName(en.getLayer(OdTvGeometryData_GeometryTypes.kText)))
                en.setLayer(GetLayerDef(text.SelectedItem.ToString()), OdTvGeometryData_GeometryTypes.kText);

            Update();
        }
        _mm.StopTransaction(mtr);
    }

    private new string GetLayerName(OdTvLayerDef lyDef)
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        string name = "";
        if (lyDef.getType() == OdTvLayerDef_LayerType.kId)
            name = lyDef.getLayer().openObject().getName();
        _mm.StopTransaction(mtr);
        return name;
    }


    #endregion

    #region Entity visibility

    private void FillEntitiesVisibility(Grid grid, int[] arr)
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvEntity en = _enId.openObject() ?? _gEnId.openAsSubEntity();
        OdTvVisibilityDef vDef = en.getVisibility();
        Label lbl = new Label() { Content = "Visibility:" };

        CheckBox cb = new CheckBox
        {
            VerticalContentAlignment = VerticalAlignment.Center,
            IsChecked = vDef.getType() != OdTvVisibilityDef_VisibilityType.kInvisible,
        };
        cb.Click += Visibility_Click;
        if (vDef.getType() == OdTvVisibilityDef_VisibilityType.kDefault)
            lbl.Foreground = new SolidColorBrush() { Color = Colors.Gray };

        Button btn = CreateButton();
        btn.Click += ShowEntitiesVisibility_Click;
        SetGridWithElements(grid, arr, lbl, cb, btn);
        _mm.StopTransaction(mtr);
    }

    private void Visibility_Click(object sender, RoutedEventArgs e)
    {
        CheckBox cb = sender as CheckBox;
        if (cb == null)
            return;
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvEntity en = _enId.openObject(OdTv_OpenMode.kForWrite) ?? _gEnId.openAsSubEntity(OdTv_OpenMode.kForWrite);
        if (en != null)
        {
            en.setVisibility(new OdTvVisibilityDef(cb.IsChecked == true));
            Update();
        }
        _mm.StopTransaction(mtr);
    }

    private Grid CreateBoolLayout(string label, OdTvVisibilityDef vDef, out CheckBox cb)
    {
        Grid grid = new Grid() { Margin = new Thickness(2) };
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());

        Label lbl = new Label() { Content = label };
        Grid.SetColumn(lbl, 0);
        cb = new CheckBox
        {
            VerticalContentAlignment = VerticalAlignment.Center,
            IsChecked = vDef.getType() != OdTvVisibilityDef_VisibilityType.kInvisible,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        if (vDef.getType() == OdTvVisibilityDef_VisibilityType.kDefault)
            lbl.Foreground = new SolidColorBrush() { Color = Colors.Gray };
        Grid.SetColumn(cb, 1);

        grid.Children.Add(lbl);
        grid.Children.Add(cb);

        return grid;
    }

    private void ShowEntitiesVisibility_Click(object sender, RoutedEventArgs e)
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvEntity en = _enId.openObject(OdTv_OpenMode.kForWrite) ?? _gEnId.openAsSubEntity();
        StackPanel panel = new StackPanel() { Margin = new Thickness(10), VerticalAlignment = VerticalAlignment.Stretch };
        CheckBox all, polylines, edges, faces, vertices, text;
        panel.Children.Add(CreateBoolLayout("All:", en.getVisibility(OdTvGeometryData_GeometryTypes.kAll), out all));
        panel.Children.Add(CreateBoolLayout("Polylines:", en.getVisibility(OdTvGeometryData_GeometryTypes.kPolylines), out polylines));
        panel.Children.Add(CreateBoolLayout("Edges:", en.getVisibility(OdTvGeometryData_GeometryTypes.kEdges), out edges));
        panel.Children.Add(CreateBoolLayout("Faces:", en.getVisibility(OdTvGeometryData_GeometryTypes.kFaces), out faces));
        panel.Children.Add(CreateBoolLayout("Vertices:", en.getVisibility(OdTvGeometryData_GeometryTypes.kVertices), out vertices));
        panel.Children.Add(CreateBoolLayout("Text:", en.getVisibility(OdTvGeometryData_GeometryTypes.kText), out text));
        if (CreateDialog("Entity visibilities", new Size(270, 280), panel).ShowDialog() == true)
        {
            bool val = all.IsChecked == true;
            if (val != (en.getVisibility(OdTvGeometryData_GeometryTypes.kAll).getType() != OdTvVisibilityDef_VisibilityType.kInvisible))
                en.setVisibility(new OdTvVisibilityDef(val), OdTvGeometryData_GeometryTypes.kAll);
            val = polylines.IsChecked == true;
            if (val != (en.getVisibility(OdTvGeometryData_GeometryTypes.kPolylines).getType() != OdTvVisibilityDef_VisibilityType.kInvisible))
                en.setVisibility(new OdTvVisibilityDef(val), OdTvGeometryData_GeometryTypes.kPolylines);
            val = edges.IsChecked == true;
            if (val != (en.getVisibility(OdTvGeometryData_GeometryTypes.kEdges).getType() != OdTvVisibilityDef_VisibilityType.kInvisible))
                en.setVisibility(new OdTvVisibilityDef(val), OdTvGeometryData_GeometryTypes.kEdges);
            val = faces.IsChecked == true;
            if (val != (en.getVisibility(OdTvGeometryData_GeometryTypes.kFaces).getType() != OdTvVisibilityDef_VisibilityType.kInvisible))
                en.setVisibility(new OdTvVisibilityDef(val), OdTvGeometryData_GeometryTypes.kFaces);
            val = vertices.IsChecked == true;
            if (val != (en.getVisibility(OdTvGeometryData_GeometryTypes.kVertices).getType() != OdTvVisibilityDef_VisibilityType.kInvisible))
                en.setVisibility(new OdTvVisibilityDef(val), OdTvGeometryData_GeometryTypes.kVertices);
            val = text.IsChecked == true;
            if (val != (en.getVisibility(OdTvGeometryData_GeometryTypes.kText).getType() != OdTvVisibilityDef_VisibilityType.kInvisible))
                en.setVisibility(new OdTvVisibilityDef(val), OdTvGeometryData_GeometryTypes.kText);
            Update();
        }
        _mm.StopTransaction(mtr);
    }


    #endregion

    #region Entity linetype scale

    private void FillEntitiesLinetypeScale(Grid grid, int[] arr)
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvEntity en = _enId.openObject() ?? _gEnId.openAsSubEntity();
        Label lbl = new Label() { Content = "Linetype scale:" };

        DoubleTextBox tb = new DoubleTextBox { VerticalContentAlignment = VerticalAlignment.Center, Text = en.getLinetypeScale().ToString() };
        tb.LostKeyboardFocus += LinetypeScale_LostKeyboardFocus;

        Button btn = CreateButton();
        btn.Click += ShowEntitiesLinetypeScales_Click;
        SetGridWithElements(grid, arr, lbl, tb, btn);
        _mm.StopTransaction(mtr);
    }

    private void LinetypeScale_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
    {
        IntTextBox iTb = sender as IntTextBox;
        if (iTb == null)
            return;
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvEntity en = _enId.openObject(OdTv_OpenMode.kForWrite) ?? _gEnId.openAsSubEntity();
        if (!en.getLinetypeScale().Equals(double.Parse(iTb.Text)))
        {
            en.setLinetypeScale(double.Parse(iTb.Text));
            Update();
        }
        _mm.StopTransaction(mtr);
    }

    private Grid CreateTextLayout(string label, double val, out DoubleTextBox tb)
    {
        Grid grid = new Grid() { Margin = new Thickness(2) };
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());

        Label lbl = new Label() { Content = label };
        Grid.SetColumn(lbl, 0);
        tb = new DoubleTextBox { VerticalContentAlignment = VerticalAlignment.Center, Text = val.ToString() };
        Grid.SetColumn(tb, 1);

        grid.Children.Add(lbl);
        grid.Children.Add(tb);

        return grid;
    }

    private void ShowEntitiesLinetypeScales_Click(object sender, RoutedEventArgs e)
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvEntity en = _enId.openObject() ?? _gEnId.openAsSubEntity();
        StackPanel panel = new StackPanel() { Margin = new Thickness(10), VerticalAlignment = VerticalAlignment.Stretch };
        DoubleTextBox all, polylines, edges;
        panel.Children.Add(CreateTextLayout("All:", en.getLinetypeScale(OdTvGeometryData_GeometryTypes.kAll), out all));
        panel.Children.Add(CreateTextLayout("Polylines:", en.getLinetypeScale(OdTvGeometryData_GeometryTypes.kPolylines), out polylines));
        panel.Children.Add(CreateTextLayout("Edges:", en.getLinetypeScale(OdTvGeometryData_GeometryTypes.kEdges), out edges));
        if (CreateDialog("Entity linetype scales", new Size(270, 190), panel).ShowDialog() == true)
        {
            double val = double.Parse(all.Text);
            if (!en.getLinetypeScale(OdTvGeometryData_GeometryTypes.kAll).Equals(val))
                en.setLinetypeScale(val, (ushort)OdTvGeometryData_GeometryTypes.kAll);
            val = double.Parse(polylines.Text);
            if (!en.getLinetypeScale(OdTvGeometryData_GeometryTypes.kPolylines).Equals(val))
                en.setLinetypeScale(val, (ushort)OdTvGeometryData_GeometryTypes.kPolylines);
            val = double.Parse(edges.Text);
            if (en.getLinetypeScale(OdTvGeometryData_GeometryTypes.kEdges).Equals(val))
                en.setLinetypeScale(val, (ushort)OdTvGeometryData_GeometryTypes.kEdges);
            Update();
        }
        _mm.StopTransaction(mtr);
    }

    #endregion
}
