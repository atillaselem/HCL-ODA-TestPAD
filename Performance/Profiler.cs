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
    public void Restart()
    {
        _stopWatch.Restart();
    }

    private void ElapsedWriteLine()
    {
        long ticks = _stopWatch.ElapsedTicks;
        double ns = _nanoSecPerTick * (double)ticks;
        double ms = ns / 1000000.0;
        double s = ms / 1000;
        var elapsedText = $"{_title,-50}{string.Format($"[{ms.ToString("000.##0"),5} ms]")}";
        Console.WriteLine("[PROFILER] : " + elapsedText);
    }
    public double ElapsedNanoSec(Action<double> writeAction = default)
    {
        var (_, ns, _, _) = Elapsed();
        writeAction?.Invoke(ns);
        //var elapsedText = $"{_title,-50}{string.Format($"[{ns.ToString("000.##0"),5} ns]")}";
        //Console.WriteLine("[PROFILER] : " + elapsedText);
        return ns;
    }
    public double ElapsedMiliSec(Action<double> writeAction = default)
    {
        var (_, _, ms, _) = Elapsed();
        writeAction?.Invoke(ms);
        //var elapsedText = $"{_title,-50}{string.Format($"[{ms.ToString("000.##0"),5} ms]")}";
        //Console.WriteLine("[PROFILER] : " + elapsedText);
        return ms;
    }
    public double ElapsedSec(Action<double> writeAction = default)
    {
        var (_, _, _, sec) = Elapsed();
        writeAction?.Invoke(sec);
        //var elapsedText = $"{_title,-50}{string.Format($"[{sec.ToString("000.##0"),5} v]")}";
        //Console.WriteLine("[PROFILER] : " + elapsedText);
        return sec;
    }
    private (long ticks, double ns, double ms, double s) Elapsed()
    {
        long ticks = _stopWatch.ElapsedTicks;
        double ns = _nanoSecPerTick * (double)ticks;
        double ms = ns / 1000000.0;
        double s = ms / 1000;
        return (ticks, ns, ms, s);  
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
                ElapsedWriteLine();
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
        ElapsedWriteLine();
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
