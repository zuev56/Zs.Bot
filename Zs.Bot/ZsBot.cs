using System;
using Microsoft.EntityFrameworkCore;
using Zs.Bot.Helpers;
using Zs.Bot.Model.Db;
using Zs.Bot.Modules.Command;
using Zs.Bot.Modules.Messaging;
using Zs.Common.Interfaces;

namespace Zs.Bot
{
    public class ZsBot
    {
        private readonly IZsConfiguration _configuration;
        private readonly Logger _logger = Logger.GetInstance();

        public CommandManager CommandManager { get; set; }
        public IMessenger Messenger { get; set; }


        public ZsBot(IZsConfiguration configuration, IMessenger messenger)
        {
            try
            {
                _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

                var optionsBuilder = new DbContextOptionsBuilder<ZsBotDbContext>();
                optionsBuilder.UseNpgsql(_configuration.ConnectionString);
                ZsBotDbContext.Initialize(optionsBuilder.Options);

                Messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
                Messenger.MessageReceived += Messenger_MessageReceived;
                Messenger.MessageSent += Messenger_MessageSent;
                Messenger.MessageDeleted += Messenger_MessageDeleted;

                CommandManager = new CommandManager();
                CommandManager.CommandCompleted += CommandManager_CommandCompleted;
            }
            catch (Exception e)
            {
                var te = new TypeInitializationException(typeof(ZsBot).FullName, e);
                _logger.LogError(te, nameof(ZsBot));
            }
        }

        private void CommandManager_CommandCompleted(CommandResult result)
        {
            var chat = DbChat.GetChat(result.ChatIdForAnswer);
            Messenger.AddMessageToOutbox(chat, result.Text);
        }

        private void Messenger_MessageReceived(MessageActionEventArgs args)
        {
            try
            {
                var message = args.Message;

                SaveMessageData(args);

                // 1. Проверка авторизации
                //if (!Authorization(tgMessage) && session.SessionCurrentStep != IsWaitingForPassword)
                //    return;

                // 2. Обрабатываем в зависимости от того, команда это или данные                           
                if (BotCommand.IsCommand(message.MessageText))
                {
                    var botCommand = BotCommand.ParseMessage(message);

                    if (botCommand != null)
                        CommandManager.EnqueueCommand(botCommand);
                    else
                        Messenger.AddMessageToOutbox(args.Chat, $"Unknown command '{message.MessageText}'");
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(ZsBot));
            }
        }

        private void Messenger_MessageSent(MessageActionEventArgs args)
        {
            SaveMessageData(args);
        }

        private void Messenger_MessageDeleted(MessageActionEventArgs args)
        {
            SaveMessageData(args);
        }

        private void SaveMessageData(MessageActionEventArgs args)
        {
            try
            {
                if (args.User.UserName == null)
                    args.User.UserName = args.User.UserFullName ?? "noName";

                DbUser.SaveToDb(args.User);
                DbChat.SaveToDb(args.Chat);
                args.Message.ChatId = DbChat.GetId(args.Chat);
                args.Message.UserId = DbUser.GetId(args.User);
                if (args.Message.MessageText == null)
                    args.Message.MessageText = "Empty/service message";
                DbMessage.SaveToDb(args.Message);
            }
            catch (Exception e)
            {
                e.Data.Add("User", args.User);
                e.Data.Add("Chat", args.Chat);
                e.Data.Add("Message", args.Message);
                _logger.LogError(e, nameof(ZsBot));
            }
        }

    }
}
