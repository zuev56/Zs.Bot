using System;
using System.Diagnostics;
using System.Threading;

namespace Zs.Common.Modules.CycleWorker
{
    /// <summary>
    /// <see cref="Job"/> based on program code
    /// </summary>
    public sealed class ProgramJob : Job
    {
        private readonly Action _method;
        private readonly object _parameter;


        public ProgramJob(TimeSpan period, Action method, object parameter = null)
        {
            Period = period != default ? period : throw new ArgumentException($"{nameof(period)} can't have default value");

            _method = method ?? throw new ArgumentNullException(nameof(method));
            _parameter = parameter;
        }

        protected override void JobBody()
        {
#if DEBUG
            Trace.WriteLine($"ProgramJobBody: [{Counter}], ThreadId: {Thread.CurrentThread.ManagedThreadId}");
#endif
            _method.Invoke();
        }
    }
}
