// Copyright © 2018 by Hilti Corporation – all rights reserved

using CommonServiceLocator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace HCL_ODA_TestPAD.HCL.Profiler;

public class CadLogProfiler : CadProfiler
{
    public CadLogProfiler():base(string.Empty, string.Empty)
    {
        
    }
    public CadLogProfiler(string title, string caption) : base(title, caption)
    {
    }
    protected override void WriteLineElapsed()
    {
        var logger = ServiceLocator.Current.GetInstance<ILogger<CadLogProfiler>>();
        logger.LogInformation("{ElapsedText}", ElapsedText());
    }
}