using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zs.Common.Abstractions;
using Zs.Common.Services.Abstractions;

namespace Zs.Common.Services.Scheduler
{
    /// <summary>
    /// A specified series of operations performed sequentially by <see cref="Scheduler"/>
    /// </summary>
    public abstract class Job : IJob
    {
        private int _idleStepsCount;
        private const int STOPPED = 0, RUNNING = 1; 
        private int _isRunning;
        private readonly ILogger _logger;

        public bool IsRunning => _isRunning == RUNNING;
        public long Counter { get; private set; }
        public int IdleStepsCount
        {
            get => _idleStepsCount;
            set => Interlocked.Exchange(ref _idleStepsCount, value);
        }
        public DateTime? NextRunDate { get; protected set; }
        public DateTime? LastRunDate { get; protected set; }
        public TimeSpan Period { get; protected set; }
        public IServiceResult<string> LastResult { get; protected set; }
        public string Description { get; init; }


        public event Action<IJob, IServiceResult<string>> ExecutionCompleted;

        /// <summary>  </summary>
        /// <param name="period">Time interval between executions</param>
        /// <param name="startDate">First execution date</param>
        public Job(TimeSpan period, DateTime? startDate = null, ILogger logger = null)
        {
            Period = period;
            NextRunDate = startDate;
            _logger = logger;
        }

        protected abstract IServiceResult<string> GetExecutionResult();
        
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

                    LastResult = GetExecutionResult();

                    Volatile.Read(ref ExecutionCompleted)?.Invoke(this, LastResult);
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
