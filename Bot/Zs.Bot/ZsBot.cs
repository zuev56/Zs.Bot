using System;
using Microsoft.Extensions.Configuration;
using Zs.Bot.Model.Db;
using Zs.Bot.Modules.Command;
using Zs.Bot.Modules.Messaging;
using Zs.Common.Abstractions;

namespace Zs.Bot
{
    public class ZsBot
    {
        private readonly IConfiguration _configuration;
        private readonly IZsLogger _logger;
        private readonly IContextFactory<ZsBotDbContext> _contextFactory;
        private readonly bool _detailedLogging;

        public CommandManager CommandManager { get; set; }
        public IMessenger Messenger { get; set; }


        public ZsBot(
            IConfiguration configuration,
            IMessenger messenger,
            IContextFactory<ZsBotDbContext> contextFactory,
            IZsLogger logger)
        {
            try
            {
                _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
                
                //if (_configuration["DetailedLogging"] != null)
                bool.TryParse(_configuration["DetailedLogging"]?.ToString(), out _detailedLogging);

                Messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
                Messenger.MessageReceived += Messenger_MessageReceived;
                Messenger.MessageSent     += Messenger_MessageSent;
                Messenger.MessageEdited   += Messenger_MessageEdited;
                Messenger.MessageDeleted  += Messenger_MessageDeleted;

                _contextFactory = contextFactory;
                _logger = logger;

                CommandManager = new CommandManager(_contextFactory, _logger);
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
            var chat = DbModelExtensions.GetChat(result.ChatIdForAnswer, _contextFactory.GetContext());
            Messenger.AddMessageToOutbox(chat, result.Text);
        }

        private void Messenger_MessageReceived(MessageActionEventArgs args)
        {
            try
            {
                SaveMessageData(args);

                // 1. Проверка авторизации
                //if (!Authorization(tgMessage) && session.SessionCurrentStep != IsWaitingForPassword)
                //    return;

                // 2. Обрабатываем в зависимости от того, команда это или данные                           
                if (BotCommand.IsCommand(args.Message.MessageText))
                {
                    var botCommand = BotCommand.ParseMessage(args.Message, CommandManager.GetDbCommand);

                    if (botCommand != null)
                        CommandManager.EnqueueCommand(botCommand);
                    else
                        Messenger.AddMessageToOutbox(args.Chat, $"Unknown command '{args.Message.MessageText}'");
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

                    args.Message.MessageId = (int)identicalMessageId;
                    args.Message.UpdateRawData(_contextFactory.GetContext());
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
                    {
                        args.User.UserId = (int)identicalUserId;
                        args.User.UpdateRawData(_contextFactory.GetContext());
                    }
                    else
                    {
                        args.User.SaveToDb(_contextFactory.GetContext());
                    }

                    args.Message.UserId = identicalUserId ?? args.User.GetActualId(_contextFactory.GetContext());
                }

                {
                    int? identicalChatId = Messenger.GetIdenticalChatId(args.Chat);
                    if (identicalChatId != null)
                    {
                        args.Chat.ChatId = (int)identicalChatId;
                        args.Chat.UpdateRawData(_contextFactory.GetContext());
                    }
                    else
                    {
                        args.Chat.SaveToDb(_contextFactory.GetContext());
                    }

                    args.Message.ChatId = args.Chat.GetActualId(_contextFactory.GetContext());
                }

                {
                    if (args.Message.MessageText == null)
                        args.Message.MessageText = "Empty/service message";

                    args.Message.SaveToDb(_contextFactory.GetContext());
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
