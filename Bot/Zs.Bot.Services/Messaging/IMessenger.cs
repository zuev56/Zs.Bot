using System;
using System.Threading.Tasks;
using Zs.Bot.Data.Models;

namespace Zs.Bot.Services.Messaging
{
    /// <summary>
    /// General interface for all messengers
    /// </summary>
    public interface IMessenger
    {
        /// <summary> Converts specific messages, users, chats, etc. to general types </summary>
        IToGenegalItemConverter ItemConverter { get; set; }

        /// <summary> Occurs when message is sent </summary>
        public event EventHandler<MessageActionEventArgs> MessageSent;

        /// <summary> Occurs when message is edited </summary>
        public event EventHandler<MessageActionEventArgs> MessageEdited;

        /// <summary> Occurs when message is received </summary>
        public event EventHandler<MessageActionEventArgs> MessageReceived;

        /// <summary> Occurs when message is deleted </summary>
        public event EventHandler<MessageActionEventArgs> MessageDeleted;


        /// <summary> Add a text message to the queue for sending </summary>
        Task AddMessageToOutboxAsync(Chat chat, string messageText, Message messageToReply = null);

        /// <summary> Add a text message to the queue for sending </summary>
        Task AddMessageToOutboxAsync(string messageText, params string[] userRoleCodes);

        /// <summary> Delete message from chat </summary>
        Task<bool> DeleteMessage(Message message);
    }
}