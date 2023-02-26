using System;
using System.Threading;
using System.Threading.Tasks;

namespace HCL_ODA_TestPAD.Utility
{
    public class EasyTask
    {
        #region Private Instance Variables 
        private CancellationTokenSource _cTokenSource;
        private CancellationToken _cToken;
        private Task _task;
        private Action _refAction;
        private string _taskName;
        #endregion

        #region Private Instance Methods 

        private Task StartAction(Action workAction)
        {
            try
            {
                _refAction = workAction;
                _task = Task.Factory.StartNew(() => ActionCaller(), _cToken);
                return _task;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{TaskName} - Exception : {ex.Message}");
            }
            return null;
        }

        private void ActionCaller()
        {
            if (!string.IsNullOrWhiteSpace(_taskName))
            {
                Console.WriteLine($"{TaskName} started..");
            }
            _refAction?.Invoke();
            if (!string.IsNullOrWhiteSpace(_taskName))
            {
                Console.WriteLine($"{TaskName} finished..");
            }
        }
        #endregion

        #region Constructors & Finalizers 

        protected EasyTask(string taskName)
        {
            _taskName = taskName;
            _cTokenSource = new CancellationTokenSource();
            _cToken = _cTokenSource.Token;
        }

        #endregion

        #region Public Instance Properties 
        public bool IsCancellationRequested => _cToken.IsCancellationRequested;
        public Task RunningTask => _task;
        public CancellationToken CancelToken => _cToken;
        public string TaskName => $"[EasyTASK] : {_taskName}";
        public bool IsRunning
        {
            get => _isRunning;
            private set => _isRunning = value;
        }

        #endregion

        #region Public Instance Methods 
        /// <summary>
        /// Starts the task.
        /// </summary>
        /// <param name="workAction">The work action.</param>
        public static EasyTask Start(Action workAction, string taskName = "")
        {
            var easyTask = new EasyTask(taskName);
            easyTask.IsRunning = true;
            easyTask.StartTask(workAction);
            return easyTask;
        }

        public static EasyTask Start(Action workAction, Action cancelCallback, string taskName = "")
        {
            var easyTask = new EasyTask(taskName);
            easyTask.CancelToken.Register(cancelCallback);
            easyTask.StartTask(workAction);
            return easyTask;
        }

        public static EasyTask Start(Action workAction, Action cancelCallback, Action completedCallback, string taskName = "")
        {
            var easyTask = new EasyTask(taskName);
            easyTask.CancelToken.Register(cancelCallback);
            easyTask.StartTask(workAction).ContinueWith(_ => completedCallback());
            return easyTask;
        }

        public static EasyTask Start(Action workAction, Action completedCallback, bool continueWith = true, string taskName = "")
        {
            var easyTask = new EasyTask(taskName);
            if (continueWith)
            {
                easyTask.StartTask(workAction).ContinueWith(_ => completedCallback());
            }
            else
            {
                easyTask.StartTask(workAction);
            }
            return easyTask;
        }

        /// <summary>
        /// Starts the task.
        /// </summary>
        /// <param name="workAction">The work action.</param>
        protected Task StartTask(Action workAction)
        {
            return StartAction(workAction);
        }

        /// <summary>
        /// Cancels the task.
        /// </summary>
        public void CancelTask()
        {
            if (RunningTask == null) return;
            if (_cToken.CanBeCanceled)
            {
                _cTokenSource?.Cancel();
                _isRunning = false;
            }
        }

        /// <summary>
        /// Stops the task.
        /// </summary>
        public void StopTask()
        {
            CancelTask();
            WaitTask();
        }

        //public void JoinTask()
        //{
        //    WaitTask();
        //}

        public void WaitTask()
        {
            try
            {
                //RunningTask.Wait(_cToken);
                RunningTask?.Wait();
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions == null)
                {
                    Console.WriteLine($"TASK (NOT INNER) EXCEPTION : Source [{TaskName}] - Exception type {ex.GetType()}");
                }
                else
                {
                    foreach (Exception inner in ex.InnerExceptions)
                    {
                        Console.WriteLine($"TASK INNER EXCEPTION : Source [{TaskName}] - Exception type {inner.GetType()}");
                    }
                }
            }
        }

        private Thread _thread;
        private volatile bool _isRunning;

        public static EasyTask StartThread(Action workAction, string taskName = "")
        {
            var easyTask = new EasyTask(taskName);
            easyTask.IsRunning = true;
            easyTask.ThreadStart(workAction);
            return easyTask;
        }

        public void StopThread()
        {
            IsRunning = false;
            WaitForThreadTermination().ConfigureAwait(false);
        }

        private async Task WaitForThreadTermination(int delay = 100)
        {
            while ((_thread.ThreadState & (ThreadState.Unstarted | ThreadState.Stopped)) == 0)
            {
                await Task.Delay(delay);
            }
        }

        private void ThreadStart(Action workAction)
        {
            _thread = new Thread(() => workAction()) { IsBackground = true };
            _thread.Start();
        }

        public void ThrowIfCancellationRequested()
        {
            _cToken.ThrowIfCancellationRequested();
        }
        #endregion
    }
}
