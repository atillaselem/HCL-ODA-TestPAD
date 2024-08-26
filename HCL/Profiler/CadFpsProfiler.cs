// Copyright © 2018 by Hilti Corporation – all rights reserved

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using CommonServiceLocator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HCL_ODA_TestPAD.HCL.Profiler;

public sealed class CadFpsProfiler : CadProfiler
{
    private static Lazy<CadFpsProfiler> s_lazy;
    private readonly Stopwatch _globalWatch;
    private double _lastElapsedTickInMs;
    private readonly List<double> _elapsedTicksInMs;
    public CadFpsProfiler() : base(string.Empty, string.Empty)
    {

    }
    public static CadFpsProfiler Instance(string title, string caption)
    {
        if (s_lazy != null && s_lazy.IsValueCreated)
        {
            var profiler = s_lazy.Value;
            if (profiler.Title != title || profiler.Caption != caption)
            {
                CreateNew(title, caption);
            }
            else
            {
                profiler.RestartWatch();
            }
            return s_lazy.Value;
        }

        return CreateNew(title, caption);
    }

    private static CadFpsProfiler CreateNew(string title, string caption)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(caption);
        s_lazy = new(() => new CadFpsProfiler(title, caption));
        return s_lazy.Value;
    }

    private CadFpsProfiler(string title, string caption) : base(title, caption)
    {
        _elapsedTicksInMs = [];
        _globalWatch = Stopwatch.StartNew();
    }
    protected override void WriteLineElapsed()
    {
        var fps = _elapsedTicksInMs
            .Select(tick => MsInSec / tick)
            .Average();
        _elapsedTicksInMs.Clear();
        var elapsedFps = $"[Performance] : [{Caption}] => {Title} : {string.Format(CultureInfo.InvariantCulture, $"[{fps,3:00.#0} FPS]")}";
        var logger = ServiceLocator.Current.GetService<ILogger<CadFpsProfiler>>();
        logger.LogInformation("{ElapsedText}", elapsedFps);
    }

    protected override void Dispose(bool isDisposing)
    {
        try
        {
            _lastElapsedTickInMs = ElapsedTicksInMs();
            var globalElapsedTime = _globalWatch.ElapsedTicks * NsPerTick() / NsInMsSec;
            _elapsedTicksInMs.Add(_lastElapsedTickInMs);
            if (globalElapsedTime <= MsInSec)
            {
                return;
            }

            WriteLineElapsed();
            _globalWatch.Restart();
        }
        finally
        {
            base.Dispose(false);
        }
    }
}