using System;
using System.Threading.Tasks;

namespace Zs.Common.Services.Abstractions
{
    public interface IJobBase
    {
        ///// <summary> Определяет возможность выполнения джоба в то время, пока ещё выполняется его предыдущий вызов </summary>
        //bool AllowMultipleRunning { get; }
        long Counter { get; }
        string Description { get; init; }
        int IdleStepsCount { get; set; }
        bool IsRunning { get; }
        DateTime? LastRunDate { get; }
        DateTime? NextRunDate { get; }
        TimeSpan Period { get; }

        Task Execute();
    }
}