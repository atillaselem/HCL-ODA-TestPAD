// Copyright © 2018 by Hilti Corporation – all rights reserved

namespace HCL_ODA_TestPAD.HCL.Profiler;

public class CadDebugProfiler : CadProfiler
{
    public CadDebugProfiler(string title, string caption) : base(title, caption)
    {
    }

    protected override void WriteLineElapsed()
    {
        System.Diagnostics.Debug.WriteLine(ElapsedText());
    }
}