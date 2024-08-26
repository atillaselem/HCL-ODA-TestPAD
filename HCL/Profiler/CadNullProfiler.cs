// Copyright © 2018 by Hilti Corporation – all rights reserved

using System;

namespace HCL_ODA_TestPAD.HCL.Profiler;

public sealed class CadNullProfiler : CadProfiler
{
    private static readonly Lazy<CadNullProfiler> s_lazy = new(() => new CadNullProfiler(string.Empty, string.Empty));

    public static CadNullProfiler Instance => s_lazy.Value;

    private CadNullProfiler(string title, string caption) : base(title, caption)
    {
    }
    public override void Finish()
    {
    }
    protected override void WriteLineElapsed()
    {
    }
    protected override void Dispose(bool isDisposing)
    {
        try
        {

        }
        finally
        {
            base.Dispose(isDisposing);
        }
    }
}