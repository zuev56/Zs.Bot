using Zs.Bot.Model.Data;
using Zs.Service.ChatAdmin.Data;

namespace Zs.Service.ChatAdmin.Abstractions
{
    public interface IContextFactory
    {
        BotContext GetBotContext();
        ChatAdminContext GetChatAdminContext();
    }
}
