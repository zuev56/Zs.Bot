using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zs.Common.Services.Abstractions;

namespace Zs.Common.Services.Scheduler
{
    /// <summary>
    /// Base class for Jobs
    /// </summary>
    public abstract class JobBase : IJobBase
    {
        protected int _idleStepsCount;
        protected const int STOPPED = 0, RUNNING = 1;
        protected int _isRunning;
        protected readonly ILogger _logger;

        public bool IsRunning => _isRunning == RUNNING;
        public long Counter { get; protected set; }
        public int IdleStepsCount
        {
            get => _idleStepsCount;
            set => Interlocked.Exchange(ref _idleStepsCount, value);
        }
        public DateTime? NextRunDate { get; protected set; }
        public DateTime? LastRunDate { get; protected set; }
        public TimeSpan Period { get; protected set; }
        public string Description { get; init; }

        /// <summary>  </summary>
        /// <param name="period">Time interval between executions</param>
        /// <param name="startDate">First execution date</param>
        protected JobBase(TimeSpan period, DateTime? startDate = null, ILogger logger = null)
        {
            Period = period;
            NextRunDate = startDate;
            _logger = logger;
        }

        public Task Execute()
        {
            if (Interlocked.Exchange(ref _isRunning, RUNNING) == STOPPED)
            {
                try
                {
                    if (_idleStepsCount > 0)
                    {
                        Interlocked.Decrement(ref _idleStepsCount);
                        return Task.CompletedTask;
                    }

                    if (Period == default)
                    {
                        NextRunDate = LastRunDate = DateTime.MaxValue;
                        Period = TimeSpan.MaxValue;
                        throw new ArgumentException($"{nameof(Period)} can't have default value");
                    }

                    AfterExecution();
                }
                finally
                {
                    LastRunDate = DateTime.Now;
                    NextRunDate = DateTime.Now + Period;
                    Counter++;
                    Interlocked.Exchange(ref _isRunning, STOPPED);
                }
            }

            return Task.CompletedTask;
        }

        protected abstract void AfterExecution();

        public static DateTime NextHour()
        {
            var nextHour = DateTime.Now.Hour < 23
                ? DateTime.Today + TimeSpan.FromHours(1)
                : DateTime.Today + TimeSpan.FromDays(1);

            while (DateTime.Now > nextHour)
                nextHour += TimeSpan.FromHours(1);
            return nextHour;
        }
    }

}
