using Zs.Bot.Model.Data;
using Zs.App.ChatAdmin.Data;

namespace Zs.App.ChatAdmin.Abstractions
{
    public interface IContextFactory
    {
        BotContext GetBotContext();
        ChatAdminContext GetChatAdminContext();
    }
}
