using Zs.App.ChatAdmin.Data;
using Zs.Bot.Data;

namespace Zs.App.ChatAdmin.Abstractions
{
    public interface IContextFactory
    {
        BotContext GetBotContext();
        ChatAdminContext GetChatAdminContext();
    }
}
