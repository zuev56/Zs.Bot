using System;
using Zs.Bot.Model.Db;

namespace Zs.Bot.Modules.Messaging
{
    /// <summary>
    /// General interface for all messengers
    /// </summary>
    public interface IMessenger
    {
        IToGenegalItemConverter ItemConverter { get; set; }

        /// <summary> Occurs when message is sent </summary>
        public event Action<MessageActionEventArgs> MessageSent;

        /// <summary> Occurs when message is edited </summary>
        public event Action<MessageActionEventArgs> MessageEdited;

        /// <summary> Occurs when message is received </summary>
        public event Action<MessageActionEventArgs> MessageReceived;

        /// <summary> Occurs when message is deleted </summary>
        public event Action<MessageActionEventArgs> MessageDeleted;


        /// <summary> Add a text message to the queue for sending </summary>
        void AddMessageToOutbox(IChat chat, string messageText, IMessage messageToReply = null);

        /// <summary> Add a text message to the queue for sending </summary>
        void AddMessageToOutbox(string messageText, params string[] userRoleCodes);

        /// <summary> Delete message from chat </summary>
        bool DeleteMessage(IMessage message);

        /// <summary>
        /// Get an identical user ID in the database by comparing RawData fields
        /// </summary>
        /// <param name="user"></param>
        /// <returns>UserId if it is found. Otherwise null</returns>
        int? GetIdenticalUserId(IUser user);

        /// <summary>
        /// Get an identical chat ID in the database by comparing RawData fields
        /// </summary>
        /// <param name="chat"></param>
        /// <returns>ChatId if it is found. Otherwise null</returns>
        int? GetIdenticalChatId(IChat chat);
        
    }
}