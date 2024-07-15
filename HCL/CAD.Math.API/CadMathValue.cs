// Copyright © 2018 by Hilti Corporation – all rights reserved

using System;

namespace HCL_ODA_TestPAD.HCL.CAD.Math.API;

public abstract record CadMathValue<T> : IDisposable where T : IDisposable, new()
{
    protected CadMathValue(CadMathValue<T> _)
    {
        Value = new T();
    }
    protected CadMathValue()
    {
        Value = new T();
    }
    protected CadMathValue(T value, bool assign = true)
    {
        Value = assign ? value : new T();
    }
    protected CadMathValue(Func<T> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        Value = factory();
    }
    public T Value { get; protected init; }

    #region Equality

    protected bool IsNotEqual(Func<double> diff)
    {
        ArgumentNullException.ThrowIfNull(diff);
        return System.Math.Abs(diff()) >= double.Epsilon;
    }
    #endregion

    #region Disposable CadMathValue

    private bool IsDisposed { get; set; }
    ~CadMathValue()
    {
        Dispose(false);
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool isDisposing)
    {
        try
        {
            if (IsDisposed || !isDisposing)
            {
                return;
            }

            Value.Dispose();
        }
        finally
        {
            IsDisposed = true;
        }
    }
    #endregion
}