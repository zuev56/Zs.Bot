using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zs.Common.Abstractions;
using Zs.Common.Services.Abstractions;

namespace Zs.Common.Services.Scheduler
{
    public class Scheduler : IScheduler
    {
        private readonly IZsLogger _logger;
        private Timer _timer;
        private readonly bool _detailedLogging;

        public List<IJob> Jobs { get; } = new List<IJob>();


        public Scheduler(IConfiguration configuration = null, IZsLogger logger = null)
        {
            if (bool.TryParse(configuration?["DetailedLogging"], out bool detailedLogging))
            {
                _detailedLogging = detailedLogging;
            }
            _logger = logger;
        }

        public void Start(uint dueTimeMs, uint periodMs)
        {
            try
            {
                _timer = new Timer(new TimerCallback(DoWork));
                _timer.Change(dueTimeMs, periodMs);

                _logger?.LogInfoAsync($"{nameof(Scheduler)} started", nameof(Scheduler));
            }
            catch (Exception ex)
            {
                _logger?.LogErrorAsync(ex, nameof(Scheduler));
            }
        }

        public void Stop()
        {
            try
            {
                _timer.Dispose();
                _logger?.LogInfoAsync($"{nameof(Scheduler)} stopped", nameof(Scheduler));
            }
            catch (Exception ex)
            {
                _logger?.LogErrorAsync(ex, nameof(Scheduler));
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
                                        _logger.LogErrorAsync(aex.InnerExceptions[0], nameof(Scheduler));
                                    else
                                        _logger.LogErrorAsync(task.Exception, nameof(Scheduler));
                                }
                            }, TaskContinuationOptions.OnlyOnFaulted);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogErrorAsync(ex, nameof(Scheduler));
            }
        }
    }
}
