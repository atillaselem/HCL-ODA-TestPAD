﻿using HCL_ODA_TestPAD.Mvvm.Events;
using System;

namespace HCL_ODA_TestPAD.Settings;

public class TestPadSettings
{
    private readonly IServiceFactory _serviceFactory;
    public event Action<string, object> OneOfTheSettingsChanged;
    
    public TestPadSettings(IServiceFactory serviceFactory)
    {
        _serviceFactory = serviceFactory;
    }

    public void OnDispatchChange(string itemName, string itemValue)
    {
        _serviceFactory.AppSettings = _serviceFactory.SettingsSrv.CompareSettings(_serviceFactory.AppSettings, itemValue);
        _serviceFactory.EventSrv.GetEvent<SettingsUpdateEvent>().Publish(_serviceFactory.AppSettings);
        OneOfTheSettingsChanged?.Invoke(itemName, itemValue);
    }
}