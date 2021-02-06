using Zs.Common.Abstractions;
using Zs.Common.Enums;

namespace Zs.App.Home.Web.Models
{
    public class InfoMessageVM : IInfoMessage
    {
        public InfoMessageType Type { get; init; }
        public string Text { get; init; }

        public static InfoMessageVM Success(string text)
            => new InfoMessageVM
            {
                Type = InfoMessageType.Info,
                Text = text
            };

        public static InfoMessageVM Warning(string text)
            => new InfoMessageVM
            {
                Type = InfoMessageType.Warning,
                Text = text
            };

        public static InfoMessageVM Error(string text)
            => new InfoMessageVM
            {
                Type = InfoMessageType.Error,
                Text = text
            };

    }
}
