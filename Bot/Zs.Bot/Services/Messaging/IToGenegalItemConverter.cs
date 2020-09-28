using Zs.Bot.Model.Abstractions;
using Zs.Common.Enums;

namespace Zs.Bot.Services.Messaging
{
    /// <summary> Converts specific types to general types </summary>
    public interface IToGenegalItemConverter
    {
        /// <summary> Convert specific message to <see cref="IMessage"/> </summary>
        IMessage ToGeneralMessage(object specificMessage);

        /// <summary> Convert specific chat to <see cref="IChat"/> </summary>
        IChat ToGeneralChat(object specificChat);

        /// <summary> Convert specific user to <see cref="IUser"/> </summary>
        IUser ToGeneralUser(object specificUser);

        /// <summary> Convert specific chat type to <see cref="ChatType"/> </summary>
        ChatType ToGeneralChatType(object specificChatType);
    }
}
