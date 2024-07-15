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
using System;
using ODA.Visualize.TV_Visualize;
using Microsoft.Win32;
using System.Windows.Media;
using HCL_ODA_TestPAD.Dialogs;

namespace HCL_ODA_TestPAD.ODA.Draggers;

public class OdTvSectioningOptions
{
    public static int _odtvCuttingPlanesMaxNum = 5;
    public bool IsNeedSaveSettings { get; set; }
    public bool IsShown { get; set; }
    public bool IsFilled { get; set; }
    public uint FillingColor { get; set; }
    public bool FillingPatternEnabled { get; set; }
    public OdTvGsView_CuttingPlaneFillStyle FillingPaternStyle { get; set; }
    public uint FillingPatternColor { get; set; }

    private const string SectioningOptionsSubkey = "WpfVisualizeViewer_SectioningOptions";
    private const string FillKey = "FILL";
    private const string FillingColorKey = "FillingColor";
    private const string FillingPatternEnabledKey = "FillingPatternEnabled";
    private const string FillingPatternStyleKey = "FillingPatternStyle";
    private const string FillingPatternColorKey = "FillingPatternColor";

    public OdTvSectioningOptions()
    {
        IsNeedSaveSettings = true;
        IsShown = true;

        RegistryKey key = Registry.CurrentUser.OpenSubKey(SectioningOptionsSubkey, true);
        if (key == null)
        {
            key = Registry.CurrentUser.CreateSubKey(SectioningOptionsSubkey);
            key.SetValue(FillKey, true);
            Color fillColor = new Color();
            fillColor.R = 255;
            fillColor.G = 0;
            fillColor.B = 0;
            fillColor.A = 255;
            key.SetValue(FillingColorKey, BasePaletteProperties.ColorToUInt(fillColor));
            key.SetValue(FillingPatternEnabledKey, true);
            key.SetValue(FillingPatternStyleKey, (int)OdTvGsView_CuttingPlaneFillStyle.kCheckerboard);
            key.SetValue(FillingPatternColorKey, BasePaletteProperties.ColorToUInt(new OdTvColorDef(0, 0, 255)));
        }

        IsFilled = Convert.ToBoolean(key.GetValue(FillKey));
        FillingColor = Convert.ToUInt32(key.GetValue(FillingColorKey));
        FillingPatternEnabled = Convert.ToBoolean(key.GetValue(FillingPatternEnabledKey));
        FillingPaternStyle = (OdTvGsView_CuttingPlaneFillStyle)Convert.ToInt32(key.GetValue(FillingPatternStyleKey));
        FillingPatternColor = Convert.ToUInt32(key.GetValue(FillingPatternColorKey));
    }

    public void SaveToRegister()
    {
        RegistryKey key = Registry.CurrentUser.OpenSubKey(SectioningOptionsSubkey, true);
        if (key == null)
            key = Registry.CurrentUser.CreateSubKey(SectioningOptionsSubkey);
        key.SetValue(FillKey, IsFilled);
        key.SetValue(FillingColorKey, FillingColor);
        key.SetValue(FillingPatternEnabledKey, FillingPatternEnabled);
        key.SetValue(FillingPatternStyleKey, (int)FillingPaternStyle);
        key.SetValue(FillingPatternColorKey, FillingPatternColor);
    }
}
