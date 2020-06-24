using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Zs.Common.Interfaces;

namespace Zs.Common.Modules.CycleWorker
{
    public class CycleWorker
    {
        private readonly IZsLogger _logger;
        private Timer _timer;

        public long Counter { get; private set; }
        public List<Job> Jobs { get;}


        public CycleWorker(IZsLogger logger = null)
        {
            _logger = logger;
            Jobs = new List<Job>();
        }

        public void Start(uint dueTimeMs, uint periodMs)
        {
            try
            {
                _timer = new Timer(new TimerCallback(DoWork));
                _timer.Change(dueTimeMs, periodMs);

#if DEBUG
                Trace.WriteLine($"{nameof(CycleWorker)} Start. ThreadId: {Thread.CurrentThread.ManagedThreadId}");
#endif
                _logger?.LogInfo($"{nameof(CycleWorker)} запущен", nameof(CycleWorker));
            }
            catch (Exception e)
            {
                _logger?.LogError(e, nameof(CycleWorker));
            }
        }

        public void Stop()
        {
            try
            {
                _timer.Dispose();
#if DEBUG
                Trace.WriteLine($"{nameof(CycleWorker)} Stop. ThreadId: {Thread.CurrentThread.ManagedThreadId}");
#endif
                _logger?.LogInfo($"{nameof(CycleWorker)} остановлен", nameof(CycleWorker));
            }
            catch (Exception e)
            {
                _logger?.LogError(e, nameof(CycleWorker));
            }
        }

        private void DoWork(object state)
        {
            try
            {
#if DEBUG
                Trace.WriteLine($"DoWork: [{Counter}], ThreadId: {Thread.CurrentThread.ManagedThreadId}");
#endif
                foreach (var job in Jobs)
                {
                    if (job.NextRunDate == null
                        || (job.NextRunDate < DateTime.Now && !job.IsRunning))
                    {
                        Task.Run(() => job.Execute());
                    }
                }
                Counter++;
            }
            catch (Exception e)
            {
                _logger?.LogError(e, nameof(CycleWorker));
            }
        }

    }
}
