using HCL_ODA_TestPAD.Mvvm.Events;
using HCL_ODA_TestPAD.Performance;
using System;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace HCL_ODA_TestPAD.Utility
{
    public class ProgressNotifier
    {
        public int ProgressMax { get; set; }
        public int ProgressStep { get; set; }   

        private double _totalElapsedTime;
        public async void RunAsync<TEvent1, TEvent2>(CancellationTokenSource tokenSource,
            Func<TEvent1> ProgressStepFactory, Func<TEvent2> ProgressMaxFactory)
            where TEvent1 : ProgressStepChangedEvent
            where TEvent2 : ProgressMaxChangedEvent
        {
            CancellationToken ct = tokenSource.Token;
            ProgressMaxFactory().Publish(ProgressMax);
            Console.WriteLine($"Progress Notifier Created");
            await Task.Run(() =>
            {
                var profiler = new Profiler();
                Console.WriteLine($"Progress Notifier Started");
                while (!ct.IsCancellationRequested)
                {
                    if (_totalElapsedTime >= ProgressMax)
                    {
                        ProgressMax += 2000;
                        ProgressMaxFactory().Publish(ProgressMax);
                    }
                    var elapsedMiliSec = profiler.ElapsedMiliSec();
                    if (elapsedMiliSec >= ProgressStep)
                    {
                        try
                        {
                            _totalElapsedTime += elapsedMiliSec;
                            //var _title = "ElapsedMiliSec";
                            //var elapsedText = $"{_title,-50}{string.Format($"[{_totalElapsedTime.ToString("000.##0"),5} ms]")}";
                            //Console.WriteLine("[PROFILER] : " + elapsedText);

                            ProgressStepFactory().Publish((int)_totalElapsedTime);
                            profiler.Restart();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"ReplayTxWorker exception caught :{ex.Message}");
                        }
                    }
                }
                Console.WriteLine($"Progress Notifier Stopped");
            }, ct);
        }
        public static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        }
        public static void Yield(long ticks, Dispatcher dispatcher)
        {
            // Note: a tick is 100 nanoseconds
            long dtEnd = DateTime.Now.AddTicks(ticks).Ticks;

            while (DateTime.Now.Ticks < dtEnd)
            {
                dispatcher.Invoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate (object unused) { return null; }, null);
            }

        }
    }
    public static class DispatcherHelper
    {
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(ExitFrames), frame);
            try
            {
                Dispatcher.PushFrame(frame);
            }
            catch (InvalidOperationException) { }
        }
        private static object ExitFrames(object frame)
        {
            ((DispatcherFrame)frame).Continue = false;
            return null;
        }
    }
}
