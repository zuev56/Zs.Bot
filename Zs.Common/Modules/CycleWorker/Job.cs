using System;
using System.Threading.Tasks;

namespace Zs.Common.Modules.CycleWorker
{
    /// <summary>
    /// A specified series of operations performed sequentially by <see cref="CycleWorker"/>
    /// </summary>
    public abstract class Job
    {
        public long      Counter     { get; private set; }
        public bool      IsRunning   { get; protected set; }
        public DateTime? NextRunDate { get; protected set; }
        public DateTime? LastRunDate { get; protected set; }
        public TimeSpan  Period      { get; protected set; }
        public object    LastResult  { get; protected set; }

        protected abstract void JobBody();

        public Task Execute()
        {
            if (Period == default)
            {
                NextRunDate = LastRunDate = DateTime.MaxValue;
                Period = TimeSpan.MaxValue;
                throw new ArgumentException($"{nameof(Period)} can't have default value");
            }

            IsRunning = true;

            JobBody();

            LastRunDate = DateTime.Now;
            NextRunDate = DateTime.Now + Period;
            IsRunning = false;
            Counter++;
            return Task.CompletedTask;
        }

    }
}
