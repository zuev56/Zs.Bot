using System;
using Zs.Common.Services.Abstractions;

namespace Zs.Common.Services.Scheduler
{
    /// <summary>
    /// <see cref="Job"/> based on program code
    /// </summary>
    public sealed class ProgramJob : Job
    {
        private readonly Action _method;
        private readonly object _parameter;


        public ProgramJob(TimeSpan period,
            Action method,
            object parameter = null,
            DateTime? startDate = null,
            string description = null)
            : base(period, startDate)
        {
            Period = period != default ? period : throw new ArgumentException($"{nameof(period)} can't have default value");

            _method = method ?? throw new ArgumentNullException(nameof(method));
            _parameter = parameter;
            Description = description;
        }

        protected override IJobExecutionResult GetExecutionResult()
        {
//#if DEBUG
//            Trace.WriteLine($"ProgramJobBody: [{Counter}], ThreadId: {Thread.CurrentThread.ManagedThreadId}");
//#endif
             _method.Invoke();
            return default;
        }
    }
}
