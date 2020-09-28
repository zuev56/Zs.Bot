using Zs.Bot.Model.Data;
using Zs.App.Home.Model.Data;

namespace Zs.App.Home.Model.Abstractions
{
    public interface IContextFactory
    {
        BotContext GetBotContext();
        HomeContext GetHomeContext();
    }
}
