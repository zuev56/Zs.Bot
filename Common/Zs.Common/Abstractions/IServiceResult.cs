using System.Collections.Generic;

namespace Zs.Common.Abstractions
{
    /// <summary> Service method execution result </summary>
    public interface IServiceResult
    {
        bool IsSuccess { get; }
        bool HasWarnings { get; }
        IList<IInfoMessage> Messages { get; }

    }

    /// <summary> Service method execution result </summary>
    public interface IServiceResult<TResult> : IServiceResult
    {
        TResult Result { get; }
    }
}
