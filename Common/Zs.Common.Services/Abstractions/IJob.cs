using System;
using System.Threading.Tasks;
using Zs.Common.Abstractions;

namespace Zs.Common.Services.Abstractions
{
    public interface IJob
    {
        long Counter { get; }
        string Description { get; }
        bool IsRunning { get; }
        ///// <summary> Определяет возможность выполнения джоба в то время, пока ещё выполняется его предыдущий вызов </summary>
        //bool AllowMultipleRunning { get; }
        /// <summary> Количество холостых выполнений, когда логика джоба не будет выполняться. Для откладывания выполнения джоба </summary>
        int IdleStepsCount { get; set; }
        IServiceResult<string> LastResult { get; }
        DateTime? LastRunDate { get; }
        DateTime? NextRunDate { get; }
        TimeSpan Period { get; }

        event Action<IJob, IServiceResult<string>> ExecutionCompleted;

        Task Execute();
    }
}
