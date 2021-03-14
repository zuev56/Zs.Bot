using Microsoft.Extensions.Logging;
using System;
using Zs.Common.Abstractions;
using Zs.Common.Models;
using Zs.Common.Services.Abstractions;

namespace Zs.Common.Services.Scheduler
{
    /// <summary>
    /// <see cref="Job"/> based on program code
    /// </summary>
    public sealed class ProgramJob : Job
    {
        private readonly Action _action;
        private readonly object _parameter;

        public ProgramJob(
            TimeSpan period,
            Action method,
            object parameter = null,
            DateTime? startDate = null,
            string description = null,
            ILogger logger = null)
            : base(period, startDate, logger)
        {
            Period = period != default ? period : throw new ArgumentException($"{nameof(period)} can't have default value");
            _action = method ?? throw new ArgumentNullException(nameof(method));
            _parameter = parameter;
            Description = description;
        }

        protected override IServiceResult<string> GetExecutionResult()
        {
            _action.Invoke();

            return ServiceResult<string>.Success(default);
        }
    }

    /// <summary>
    /// <see cref="Job"/> based on program code
    /// </summary>
    public sealed class ProgramJob<TResult> : Job<TResult>
    {
        private readonly Func<TResult> _func;
        private readonly object _parameter;

        public ProgramJob(
            TimeSpan period,
            Func<TResult> method,
            object parameter = null,
            DateTime? startDate = null,
            string description = null,
            ILogger logger = null)
            : base(period, startDate, logger)
        {
            Period = period != default ? period : throw new ArgumentException($"{nameof(period)} can't have default value");
            _func = method ?? throw new ArgumentNullException(nameof(method));
            _parameter = parameter;
            Description = description;
        }

        protected override IServiceResult<TResult> GetExecutionResult()
        {
            TResult result = _func.Invoke();

            return ServiceResult<TResult>.Success(result);
        }
    }
}
