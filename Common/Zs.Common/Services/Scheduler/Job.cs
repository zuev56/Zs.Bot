using System;
using System.Threading;
using System.Threading.Tasks;

namespace Zs.Common.Services.Scheduler
{
    public interface IJob
    {
        long Counter { get; }
        string Description { get; set; }
        bool IsRunning { get; }
        /// <summary> Количество холостых выполнений, когда логика джоба не будет выполняться. Для откладывания выполнения джоба </summary>
        int IdleStepsCount { get; set; }
        IJobExecutionResult LastResult { get; }
        DateTime? LastRunDate { get; }
        DateTime? NextRunDate { get; }
        TimeSpan Period { get; }

        event Action<IJob, IJobExecutionResult> ExecutionCompleted;

        Task Execute();
    }

    /// <summary>
    /// A specified series of operations performed sequentially by <see cref="Scheduler"/>
    /// </summary>
    public abstract class Job : IJob
    {
        private readonly object _locker = new object();
        private int _idleStepsCount;

        public long Counter { get; private set; }
        public bool IsRunning { get; protected set; }        
        public int IdleStepsCount
        {
            get => _idleStepsCount;
            set => Interlocked.Exchange(ref _idleStepsCount, value);
        }
        public DateTime? NextRunDate { get; protected set; }
        public DateTime? LastRunDate { get; protected set; }
        public TimeSpan Period { get; protected set; }
        public IJobExecutionResult LastResult { get; protected set; }
        public string Description { get; set; }

        public event Action<IJob, IJobExecutionResult> ExecutionCompleted;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="period">Time interval between executions</param>
        /// <param name="startDate">First execution date</param>
        public Job(TimeSpan period, DateTime? startDate = null)
        {
            Period = period;
            NextRunDate = startDate;
        }

        protected abstract IJobExecutionResult GetExecutionResult();

        public Task Execute()
        {
            lock (_locker)
            {
                try
                {
                    if (_idleStepsCount > 0)
                    {
                        Interlocked.Decrement(ref _idleStepsCount);
                        return Task.CompletedTask;
                    }

                    IsRunning = true;

                    if (Period == default)
                    {
                        NextRunDate = LastRunDate = DateTime.MaxValue;
                        Period = TimeSpan.MaxValue;
                        throw new ArgumentException($"{nameof(Period)} can't have default value");
                    }

                    LastResult = GetExecutionResult();

                    Volatile.Read(ref ExecutionCompleted)?.Invoke(this, LastResult);
                }
                catch
                {
                    throw;
                }
                finally
                {
                    LastRunDate = DateTime.Now;
                    NextRunDate = DateTime.Now + Period;
                    Counter++;
                    IsRunning = false;
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
