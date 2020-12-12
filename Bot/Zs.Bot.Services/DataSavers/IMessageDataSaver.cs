using System.Threading.Tasks;
using Zs.Bot.Services.Messaging;

namespace Zs.Bot.Services.DataSavers
{
    /// <summary>
    /// Saves message data
    /// </summary>
    public interface IMessageDataSaver
    {
        Task SaveNewMessageData(MessageActionEventArgs args);
        Task EditSavedMessage(MessageActionEventArgs args);
    }
}
