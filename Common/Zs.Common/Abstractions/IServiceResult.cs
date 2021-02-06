using System.Collections.Generic;
using Zs.Common.Enums;

namespace Zs.Common.Abstractions
{
    /// <summary> Результат выполнения метода сервиса, содержащий информацию в удобном виде для вывода пользователю </summary>
    public interface IServiceResult
    {
        bool IsSuccess { get; }
        IList<IInfoMessage> Messages { get; }

    }

    public interface IServiceResult<TResult> : IServiceResult
    {
        TResult Result { get; }
    }

    public interface IInfoMessage
    {
        InfoMessageType Type { get; }
        string Text { get; }
    }
}
