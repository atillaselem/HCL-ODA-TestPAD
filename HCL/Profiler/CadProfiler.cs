// Copyright © 2018 by Hilti Corporation – all rights reserved

using System;
using System.Diagnostics;
using System.Globalization;
using CommonServiceLocator;
using HCL_ODA_TestPAD.Settings;
using Microsoft.Extensions.DependencyInjection;


namespace HCL_ODA_TestPAD.HCL.Profiler
{
    public abstract class CadProfiler : IDisposable
    {
        #region Initializers
        private readonly Stopwatch _stopWatch;
        protected string Title { get; }
        protected string Caption { get; }

        protected CadProfiler(string title, string caption)
        {
            Title = string.IsNullOrWhiteSpace(title) ? "Title" : title;
            Caption = string.IsNullOrWhiteSpace(caption) ? "Caption" : caption;
            _stopWatch = Stopwatch.StartNew();
        }
        #endregion

        #region Protected Members
        private const long NsInSec = 1_000_000_000L;
        protected const long NsInMsSec = 1_000_000L;
        protected const long MsInSec = 1_000L;
        private long ElapsedTicks() => _stopWatch.ElapsedTicks;

        protected double ElapsedTicksInMs() => NsPerTick() * ElapsedTicks() / NsInMsSec;

        protected void RestartWatch() => _stopWatch.Restart();
        protected string ElapsedText() => $"[Performance] : [{Caption}] => {Title} : {string.Format(CultureInfo.InvariantCulture, $"[{ElapsedTicksInMs(),5:000.##0} ms]")}";

        protected abstract void WriteLineElapsed();

        #endregion

        #region Virtual Members
        public virtual void Finish() => Dispose();
        #endregion

        #region Static Factories
        private static bool _sUseProfiler;

        public static CadProfiler Debug(string title, string caption)
            => _sUseProfiler ? new CadDebugProfiler(title, caption) : CadNullProfiler.Instance;

        public static CadProfiler Log(string title, string caption)
            => _sUseProfiler ? new CadLogProfiler(title, caption) : CadNullProfiler.Instance;

        public static CadProfiler LogFps(string title, string caption)
            => _sUseProfiler ? CadFpsProfiler.Instance(title, caption) : CadNullProfiler.Instance;
        #endregion

        #region Static Methods
        public static bool TryUseIt() => _sUseProfiler = ServiceLocator.Current.GetService<IServiceFactory>().AppSettings.UseCadProfiler;
        protected static double NsPerTick() => (double)NsInSec / Stopwatch.Frequency;
        #endregion

        #region Disposable CadProfiler
        private bool IsDisposed { get; set; }
        ~CadProfiler()
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

                WriteLineElapsed();
            }
            finally
            {
                IsDisposed = true;
                _stopWatch.Stop();
            }
        }
        #endregion
    }
}
