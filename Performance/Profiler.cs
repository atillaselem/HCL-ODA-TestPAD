using System;
using System.Diagnostics;

namespace HCL_ODA_TestPAD.Performance;

public class Profiler : IDisposable
{
    private Stopwatch _stopWatch;
    private string _title;

    public static int NanoSecondPerTick => _nanoSecPerTick;
    private static int _nanoSecPerTick;
    private static bool _useProfiler;

    private static readonly NullProfiler NullProfiler = new NullProfiler();
    static Profiler()
    {
        _nanoSecPerTick = GetNanoSecondPerTick();
    }

    public Profiler(string title = "")
    {
        _title = (string.IsNullOrWhiteSpace(title) ? "Elapsed Time" : title);
        Start();
    }

    protected void Start()
    {
        _stopWatch = Stopwatch.StartNew();
    }

    private void Elapsed()
    {
        long ticks = _stopWatch.ElapsedTicks;
        double ns = _nanoSecPerTick * (double)ticks;
        double ms = ns / 1000000.0;
        double s = ms / 1000;
        var elapsedText = $"{_title,-50}{string.Format($"[{ms.ToString("000.##0"),5} ms]")}";
        Console.WriteLine("[PROFILER] : " + elapsedText);
    }
    private static int GetNanoSecondPerTick()
    {
        long frequency = Stopwatch.Frequency;
        var nanoSecPerTick = (int)((1000L * 1000L * 1000L) / frequency);
        return nanoSecPerTick;
    }

    #region IDisposable Support

    private bool disposedValue = false; // To detect redundant calls

    /// <summary>
    /// 
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Elapsed();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
    }


    internal static Profiler Start(string title = "")
    {
        Profiler profiler = _useProfiler ? new Profiler(title) : NullProfiler;
        return profiler;
    }

    public virtual void Stop()
    {
        Elapsed();
    }

    internal static void Enable(bool useProfiler = true)
    {
        _useProfiler = useProfiler;
    }

    #endregion
}

public class NullProfiler : Profiler
{

    public override void Stop()
    {

    }
}
