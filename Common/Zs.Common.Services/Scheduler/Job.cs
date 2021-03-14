using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Zs.Common.Abstractions;
using Zs.Common.Services.Abstractions;

namespace Zs.Common.Services.Scheduler
{
    /// <summary>
    /// A specified series of operations performed sequentially by <see cref="Scheduler"/>
    /// </summary>
    public abstract class Job : JobBase, IJob
    {
        public IServiceResult LastResult { get; protected set; }

        public event Action<IJob, IServiceResult> ExecutionCompleted;

        /// <summary>  </summary>
        /// <param name="period">Time interval between executions</param>
        /// <param name="startDate">First execution date</param>
        public Job(TimeSpan period, DateTime? startDate = null, ILogger logger = null)
            : base(period, startDate, logger)
        {
        }

        protected abstract IServiceResult GetExecutionResult();

        protected override void AfterExecution()
        {
            LastResult = GetExecutionResult();
            Volatile.Read(ref ExecutionCompleted)?.Invoke(this, LastResult);
        }
    }

    /// <summary>
    /// A specified series of operations performed sequentially by <see cref="Scheduler"/>
    /// </summary>
    public abstract class Job<TResult> : JobBase, IJob<TResult>
    {
        public IServiceResult<TResult> LastResult { get; protected set; }

        public event Action<IJob<TResult>, IServiceResult<TResult>> ExecutionCompleted;

        /// <summary>  </summary>
        /// <param name="period">Time interval between executions</param>
        /// <param name="startDate">First execution date</param>
        public Job(TimeSpan period, DateTime? startDate = null, ILogger logger = null)
            : base(period, startDate, logger)
        {
        }

        protected abstract IServiceResult<TResult> GetExecutionResult();
        
        protected override void AfterExecution()
        {
            LastResult = GetExecutionResult();
            Volatile.Read(ref ExecutionCompleted)?.Invoke(this, LastResult);
        }
    }

}
