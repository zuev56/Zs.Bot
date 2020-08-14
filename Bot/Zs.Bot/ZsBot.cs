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
        private readonly bool _detailedLogging;

        public CommandManager CommandManager { get; set; }
        public IMessenger Messenger { get; set; }


        public ZsBot(IZsConfiguration configuration, IMessenger messenger)
        {
            try
            {
                _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
                
                if (_configuration.Contains("DetailedLogging"))
                    bool.TryParse(_configuration["DetailedLogging"].ToString(), out _detailedLogging);

                var optionsBuilder = new DbContextOptionsBuilder<ZsBotDbContext>();
                optionsBuilder.UseNpgsql(_configuration["ConnectionString"].ToString());
                optionsBuilder.EnableSensitiveDataLogging(true);
                optionsBuilder.EnableDetailedErrors(true);
                ZsBotDbContext.Initialize(optionsBuilder.Options);

                Messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
                Messenger.MessageReceived += Messenger_MessageReceived;
                Messenger.MessageSent += Messenger_MessageSent;
                Messenger.MessageEdited += Messenger_MessageEdited;
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

        private void Messenger_MessageEdited(MessageActionEventArgs args)
        {
            {
                int? identicalMessageId = Messenger.GetIdenticalMessageId(args.Message);
                if (identicalMessageId != null)
                {
                    if (args.Message.MessageText == null)
                        args.Message.MessageText = "[Empty]";
                    
                    DbMessage.UpdateRawData((int)identicalMessageId, args.Message);
                }
                else
                    _logger.LogWarning("The edited message is not found in the database", args.Message, nameof(ZsBot));
            }
        }

        private void Messenger_MessageDeleted(MessageActionEventArgs args)
        {
            SaveMessageData(args);
        }

        private void SaveMessageData(MessageActionEventArgs args)
        {
            try
            {
                if (args.User != null)
                {
                    int? identicalUserId = Messenger.GetIdenticalUserId(args.User);
                    
                    if (identicalUserId != null)
                        DbUser.UpdateRawData((int)identicalUserId, args.User);
                    else
                        DbUser.SaveToDb(args.User);

                    args.Message.UserId = identicalUserId ?? DbUser.GetId(args.User);
                }

                {
                    int? identicalChatId = Messenger.GetIdenticalChatId(args.Chat);
                    if (identicalChatId != null)
                        DbChat.UpdateRawData((int)identicalChatId, args.Chat);
                    else
                        DbChat.SaveToDb(args.Chat);
                    
                    args.Message.ChatId = DbChat.GetId(args.Chat);
                }

                {
                    if (args.Message.MessageText == null)
                        args.Message.MessageText = "Empty/service message";
                    DbMessage.SaveToDb(args.Message);
                }
            }
            catch (Exception ex)
            {
                ex.Data.Add("User", args?.User);
                ex.Data.Add("Chat", args?.Chat);
                ex.Data.Add("Message", args?.Message);
                _logger.LogError(ex, nameof(ZsBot));
            }
        }

    }
}
