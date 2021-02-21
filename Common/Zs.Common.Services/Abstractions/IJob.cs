using System;
using System.Threading.Tasks;

namespace Zs.Common.Services.Abstractions
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
}
