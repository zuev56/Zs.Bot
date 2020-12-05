using System;
using Zs.Bot.Data.Models;
using Zs.Common.Enums;

namespace Zs.Bot.Services.Messaging
{
    /// <summary>
    /// Набор необходимых свойств, которые надо передавать при получении, отправке или удалении сообщения
    /// </summary>
    public class MessageActionEventArgs : EventArgs
    {
        public Message Message { get; init; }
        public Chat Chat { get; init; }
        public User User { get; init; }
        public Common.Enums.ChatType ChatType { get; init; }
        public MessageAction Action { get; init; }
    }
}
