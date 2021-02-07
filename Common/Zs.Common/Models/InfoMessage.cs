using Zs.Common.Abstractions;
using Zs.Common.Enums;

namespace Zs.Common.Models
{
    public class InfoMessage : IInfoMessage
    {
        public bool IsEmpty => string.IsNullOrWhiteSpace(Text);
        public InfoMessageType Type { get; init; }
        public string Text { get; init; }


        public static InfoMessage Success(string text)
            => new InfoMessage
            {
                Type = InfoMessageType.Info,
                Text = text
            };

        public static InfoMessage Warning(string text)
            => new InfoMessage
            {
                Type = InfoMessageType.Warning,
                Text = text
            };

        public static InfoMessage Error(string text)
            => new InfoMessage
            {
                Type = InfoMessageType.Error,
                Text = text
            };
    }
}
