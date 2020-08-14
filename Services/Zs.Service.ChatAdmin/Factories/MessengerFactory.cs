using System;
using System.Net;
using Zs.Bot.Modules.Messaging;
using Zs.Bot.Messenger.Telegram;

namespace Zs.Service.ChatAdmin.Factories
{
    public static class MessengerFactory
    {
        public static IMessenger ProvideMessenger(string messengerName, string token, IWebProxy webProxy = null)
        {
            return messengerName.ToUpperInvariant() switch
            {
                "TELEGRAM" => new TelegramMessenger(token, webProxy),
                _ => throw new ArgumentOutOfRangeException("Unknown messenger code")
            };
        }
    }
}
