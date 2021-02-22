using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zs.Common.Services.Abstractions;

namespace Zs.Common.Services.Scheduler
{
    public class Scheduler : IScheduler
    {
        private readonly ILogger<Scheduler> _logger;
        private Timer _timer;
        [Obsolete]
        private readonly bool _detailedLogging;

        public List<IJob> Jobs { get; } = new List<IJob>();

        public Scheduler(ILogger<Scheduler> logger = null)
        {
            _logger = logger;
        }

        public void Start(uint dueTimeMs, uint periodMs)
        {
            try
            {
                _timer = new Timer(new TimerCallback(DoWork));
                _timer.Change(dueTimeMs, periodMs);

                _logger?.LogInformation($"{nameof(Scheduler)} started");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"{nameof(Scheduler)} starting error");
            }
        }

        public void Stop()
        {
            try
            {
                _timer.Dispose();
                _logger?.LogInformation($"{nameof(Scheduler)} stopped");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"{nameof(Scheduler)} stopping error");
            }
        }

        private void DoWork(object state)
        {
            try
            {
                foreach (var job in Jobs)
                {
                    if (!job.IsRunning
                        && (job.NextRunDate == null || job.NextRunDate < DateTime.Now))
                    {
                        Task.Run(() => job.Execute())
                            .ContinueWith((task) =>
                            {
                                if (task.Exception is AggregateException aex)
                                {
                                    if (aex.InnerExceptions.Count == 1)
                                        _logger.LogError(aex.InnerExceptions[0], "Job executing error", job);
                                    else
                                        _logger.LogError(task.Exception, "Job executing error", job);
                                }
                            }, TaskContinuationOptions.OnlyOnFaulted);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"nameof(Scheduler) DoWork error");
            }
        }
    }
}
