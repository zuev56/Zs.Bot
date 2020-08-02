using System;
using System.Threading;
using System.Threading.Tasks;

namespace Zs.Common.Modules.CycleWorker
{
    /// <summary>
    /// A specified series of operations performed sequentially by <see cref="CycleWorker"/>
    /// </summary>
    public abstract class Job
    {
        private readonly object _locker = new object();

        public long      Counter     { get; private set; }
        public bool      IsRunning   { get; protected set; }
        public DateTime? NextRunDate { get; protected set; }
        public DateTime? LastRunDate { get; protected set; }
        public TimeSpan  Period      { get; protected set; }
        public IJobExecutionResult   LastResult  { get; protected set; }

        public event Action<IJobExecutionResult> ExecutionCompleted;

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
                    IsRunning = true;

                    if (Period == default)
                    {
                        NextRunDate = LastRunDate = DateTime.MaxValue;
                        Period = TimeSpan.MaxValue;
                        throw new ArgumentException($"{nameof(Period)} can't have default value");
                    }

                    LastResult = GetExecutionResult();

                    LastRunDate = DateTime.Now;
                    NextRunDate = DateTime.Now + Period;
                    Counter++;

                    Volatile.Read(ref ExecutionCompleted)?.Invoke(LastResult);

                    IsRunning = false;
                }
                catch { } 
            }
            return Task.CompletedTask;
        }

    }
}
