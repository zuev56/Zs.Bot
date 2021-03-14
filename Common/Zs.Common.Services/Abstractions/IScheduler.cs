using System.Collections.Generic;

namespace Zs.Common.Services.Abstractions
{
    public interface IScheduler
    {
        List<IJobBase> Jobs { get; }
        void Start(uint dueTimeMs, uint periodMs);
        void Stop();
    }
}
