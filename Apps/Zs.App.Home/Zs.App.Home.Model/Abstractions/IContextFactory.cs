using Zs.App.Home.Model.Data;
using Zs.Bot.Data;

namespace Zs.App.Home.Model.Abstractions
{
    public interface IContextFactory
    {
        BotContext GetBotContext();
        HomeContext GetHomeContext();
    }
}
