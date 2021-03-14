using System;
using System.Threading.Tasks;
using Zs.Common.Abstractions;

namespace Zs.Common.Services.Abstractions
{
    public interface IJob : IJobBase
    {
        IServiceResult LastResult { get; }

        event Action<IJob, IServiceResult> ExecutionCompleted;
    }

    public interface IJob<TResult> : IJobBase
    {
        IServiceResult<TResult> LastResult { get; }

        event Action<IJob<TResult>, IServiceResult<TResult>> ExecutionCompleted;
    }
}
