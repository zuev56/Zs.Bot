using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Zs.Bot.Data.Abstractions;
using Zs.Bot.Data.Models;
using Zs.Bot.Services.Commands;
using Zs.Bot.Services.Messaging;
using Zs.Common.Abstractions;

namespace Zs.Bot.Services.DataSavers
{
    /// <summary>
    /// Saves message data to a database (repositories)
    /// </summary>
    public class MessageDataDBSaver : IMessageDataSaver
    {
        private readonly IZsLogger _logger;
        private readonly IItemsWithRawDataRepository<Chat, int> _chatsRepo;
        private readonly IItemsWithRawDataRepository<User, int> _usersRepo;
        private readonly IItemsWithRawDataRepository<Message, int> _messagesRepo;


        public MessageDataDBSaver(
            IItemsWithRawDataRepository<Chat, int> chatsRepo,
            IItemsWithRawDataRepository<User, int> usersRepo,
            IItemsWithRawDataRepository<Message, int> messagesRepo,
            IZsLogger logger = null)
        {
            try
            {
                _chatsRepo = chatsRepo;
                _usersRepo = usersRepo;
                _messagesRepo = messagesRepo;
                _logger = logger;
            }
            catch (Exception ex)
            {
                var tex = new TypeInitializationException(typeof(MessageDataDBSaver).FullName, ex);
                _logger?.LogErrorAsync(tex, nameof(MessageDataDBSaver));
            }
        }

        public async Task SaveNewMessageData(MessageActionEventArgs args)
        {
            try
            {
                if (args.User != null)
                {
                    await _usersRepo.SaveAsync(args.User);
                    args.Message.UserId = args.User.Id;
                }

                await _chatsRepo.SaveAsync(args.Chat);
                args.Message.ChatId = args.Chat.Id;
                
                if (args.Message.Text == null)
                    args.Message.Text = "Empty/service message";
                
                await _messagesRepo.SaveAsync(args.Message);
            }
            catch (Exception ex)
            {
                ex.Data.Add("User", args?.User);
                ex.Data.Add("Chat", args?.Chat);
                ex.Data.Add("Message", args?.Message);
                _logger?.LogErrorAsync(ex, nameof(MessageDataDBSaver));
            }
        }

        public async Task EditSavedMessage(MessageActionEventArgs args)
        {
            if (args.Message.Id != default)
            {
                if (args.Message.Text == null)
                    args.Message.Text = "[Empty]";

                await _messagesRepo.SaveAsync(args.Message);
            }
            else
                _logger?.LogWarningAsync("The edited message is not found in the database", args.Message, nameof(MessageDataDBSaver));
        }

    }
}
