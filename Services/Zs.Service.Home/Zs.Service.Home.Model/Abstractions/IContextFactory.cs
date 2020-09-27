using Zs.Bot.Model.Data;
using Zs.Service.Home.Model.Data;

namespace Zs.Service.Home.Model.Abstractions
{
    public interface IContextFactory
    {
        BotContext GetBotContext();
        HomeContext GetHomeContext();
    }
}
