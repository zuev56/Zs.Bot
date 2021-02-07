using Zs.Common.Enums;

namespace Zs.Common.Abstractions
{
    public interface IInfoMessage
    {
        InfoMessageType Type { get; }
        public bool IsEmpty { get; }
        string Text { get; }
    }
}
