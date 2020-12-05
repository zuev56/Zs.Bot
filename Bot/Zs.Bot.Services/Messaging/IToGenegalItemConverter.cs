using Zs.Bot.Data.Models;
using Zs.Common.Enums;

namespace Zs.Bot.Services.Messaging
{
    /// <summary> Converts specific types to general types </summary>
    public interface IToGenegalItemConverter
    {
        /// <summary> Convert specific message to <see cref="Message"/> </summary>
        Message ToGeneralMessage(object specificMessage);

        /// <summary> Convert specific chat to <see cref="Chat"/> </summary>
        Chat ToGeneralChat(object specificChat);

        /// <summary> Convert specific user to <see cref="User"/> </summary>
        User ToGeneralUser(object specificUser);

        /// <summary> Convert specific chat type to <see cref="ChatType"/> </summary>
        Common.Enums.ChatType ToGeneralChatType(object specificChatType);
    }
}
