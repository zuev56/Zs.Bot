using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Zs.Bot.Data.Abstractions;
using Zs.Bot.Data.Models;
using Zs.Bot.Services.Commands;
using Zs.Bot.Services.DataSavers;
using Zs.Bot.Services.Messaging;
using Zs.Common.Enums;
using Zs.Common.Helpers;
using TelegramChat = Telegram.Bot.Types.Chat;
using TelegramMessage = Telegram.Bot.Types.Message;
using TelegramMessageType = Telegram.Bot.Types.Enums.MessageType;
using TelegramUser = Telegram.Bot.Types.User;

namespace Zs.Bot.Messenger.Telegram
{
    public class TelegramMessenger : IMessenger
    {
        private readonly ILogger<TelegramMessenger> _logger;
        private readonly IItemsWithRawDataRepository<Chat, int> _chatsRepo;
        private readonly IItemsWithRawDataRepository<User, int> _usersRepo;
        private readonly IItemsWithRawDataRepository<Message, int> _messagesRepo;
        private readonly IMessageDataSaver _messageDataSaver;
        private readonly ICommandManager _commandManager;
        private readonly int _sendingRetryLimit = 5;
        private readonly TelegramBotClient _botClient;
        private readonly Buffer<TgMessage> _inputMessageBuffer = new Buffer<TgMessage>();
        private readonly Buffer<TgMessage> _outputMessageBuffer = new Buffer<TgMessage>();
        private readonly object _locker = new object();

        public event EventHandler<MessageActionEventArgs> MessageEdited;
        public event EventHandler<MessageActionEventArgs> MessageReceived;
        public event EventHandler<MessageActionEventArgs> MessageSent;
        public event EventHandler<MessageActionEventArgs> MessageDeleted;

        public IToGenegalItemConverter ItemConverter { get; set; } = new ItemConverter();


        public TelegramMessenger(
            string token,
            IItemsWithRawDataRepository<Chat, int> chatsRepo,
            IItemsWithRawDataRepository<User, int> usersRepo,
            IItemsWithRawDataRepository<Message, int> messagesRepo,
            IMessageDataSaver messageDataSaver = null,
            ICommandManager commandManager = null,
            IWebProxy webProxy = null,
            ILogger<TelegramMessenger> logger = null)
        {
            try
            {
                _logger = logger;
                _chatsRepo = chatsRepo;
                _usersRepo = usersRepo;
                _messagesRepo = messagesRepo;
                
                _messageDataSaver = messageDataSaver;
                if (commandManager is not null)
                {
                    _commandManager = commandManager;
                    _commandManager.CommandCompleted += CommandManager_CommandCompleted;
                }

                _inputMessageBuffer.OnEnqueue += InputMessageBuffer_OnEnqueue;
                _outputMessageBuffer.OnEnqueue += OutputMessageBuffer_OnEnqueue;

                _botClient = webProxy != null
                           ? new TelegramBotClient(token, webProxy)
                           : new TelegramBotClient(token);

                _botClient.IsReceiving = true;
                //_botClient.Timeout = TimeSpan.FromSeconds(10);

                _botClient.ApiResponseReceived += BotClient_ApiResponseReceived;
                _botClient.OnCallbackQuery += BotClient_OnCallbackQuery;
                _botClient.OnInlineQuery += BotClient_OnInlineQuery;
                _botClient.OnInlineResultChosen += BotClient_OnInlineResultChosen;
                _botClient.OnMessage += BotClient_OnMessage;
                _botClient.OnMessageEdited += BotClient_OnMessageEdited;
                _botClient.OnReceiveError += BotClient_OnReceiveError;
                _botClient.OnReceiveGeneralError += BotClient_OnReceiveGeneralError;
                _botClient.OnUpdate += BotClient_OnUpdate;

                _botClient.StartReceiving(new UpdateType[]
                {
                    UpdateType.Unknown,
                    UpdateType.Message,
                    UpdateType.InlineQuery,
                    UpdateType.ChosenInlineResult,
                    UpdateType.CallbackQuery,
                    UpdateType.EditedMessage,
                    UpdateType.ChannelPost,
                    UpdateType.EditedChannelPost,
                    UpdateType.ShippingQuery,
                    UpdateType.PreCheckoutQuery,
                    UpdateType.Poll,
                    UpdateType.PollAnswer
                });
            }
            catch (Exception ex)
            {
                var tex = new TypeInitializationException(typeof(TelegramMessenger).FullName, ex);
                if (_logger != null)
                    _logger?.LogError(tex, $"{nameof(TelegramMessenger)} initialization error");
                else
                    throw;
            }
        }

        #region Обработчики событий TelegramBotClient

        private void BotClient_ApiResponseReceived(object sender, ApiResponseEventArgs e)
        {
            _logger?.LogTrace("ApiResponseReceived", e);
        }

        private void BotClient_MakingApiRequest(object sender, ApiRequestEventArgs e)
        {
            _logger?.LogTrace("MakingApiRequest", e);
        }

        private void BotClient_OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            _logger?.LogTrace("OnCallbackQuery", e);
        }

        private void BotClient_OnInlineQuery(object sender, InlineQueryEventArgs e)
        {
            _logger?.LogTrace("OnInlineQuery", e);
        }

        private void BotClient_OnInlineResultChosen(object sender, ChosenInlineResultEventArgs e)
        {
            _logger?.LogTrace("OnInlineResultChosen", e);
        }

        private void BotClient_OnMessage(object sender, MessageEventArgs args)
        {
            try
            {
                _logger?.LogTrace("OnMessage", args);

                _inputMessageBuffer.Enqueue(new TgMessage(args.Message));
            }
            catch (Exception e)
            {
                _logger?.LogError(e, $"Input message buffering error. TelegramMessageId={args?.Message?.MessageId}", args);
            }
        }

        private void BotClient_OnMessageEdited(object sender, MessageEventArgs e)
        {
            _logger?.LogTrace("OnMessageEdited", e);

            _inputMessageBuffer.Enqueue(new TgMessage(e.Message));
        }

        private void BotClient_OnReceiveError(object sender, ReceiveErrorEventArgs e)
        {
            if (e?.ApiRequestException?.Message != "Request timed out")
                _logger?.LogError(e.ApiRequestException, "Telegram.Bot.API error", e);
        }

        private void BotClient_OnReceiveGeneralError(object sender, ReceiveGeneralErrorEventArgs e)
        {
            if (e.Exception is System.Net.Http.HttpRequestException)
                return;

            _logger?.LogError(e.Exception, "Telegram.Bot.API general error");
        }

        private void BotClient_OnUpdate(object sender, UpdateEventArgs e)
        {
            _logger?.LogTrace("OnUpdate", e);
        }

        #endregion
        private async void CommandManager_CommandCompleted(object sender, CommandResult result)
        {
            var chat = await _chatsRepo.FindByKeyAsync(result.ChatIdForAnswer);
            await AddMessageToOutbox(chat, result.Text);
        }

        private void InputMessageBuffer_OnEnqueue(object sender, TgMessage item)
        {
//#if DEBUG
            //Trace.WriteLine($"InputMessageEnqueue: {item?.Text}, ThreadId: {Thread.CurrentThread.ManagedThreadId}");
//#endif
            Task task = null;
            task = Task.Run(() => ProcessInputMessages(task));
        }

        private void OutputMessageBuffer_OnEnqueue(object sender, TgMessage item)
        {
//#if DEBUG
            //Trace.WriteLine($"OutputMessageEnqueue: {item?.Text}, ThreadId: {Thread.CurrentThread.ManagedThreadId}");
//#endif
            Task task = null;
            task = Task.Run(() => ProcessOutputMessages(task));
        }
      
        private async Task ProcessInputMessages(Task currentTask)
        {
            TgMessage msgForLog = null;

            try
            {
                while (_inputMessageBuffer.TryDequeue(out TgMessage tgMessage))
                {
                    msgForLog = tgMessage;
                    tgMessage.IsSucceed = true;

                    var args = new MessageActionEventArgs()
                    {
                        Message  = ItemConverter.ToGeneralMessage(tgMessage),
                        Chat     = ItemConverter.ToGeneralChat(tgMessage.Chat),
                        User     = ItemConverter.ToGeneralUser(tgMessage.From),
                        ChatType = ItemConverter.ToGeneralChatType(tgMessage.Chat.Type),
                        Action   = MessageAction.Received
                    };

                    if (tgMessage.EditDate is not null)
                    {
                        if (await TrySetExistingMessageId(args.Message))
                        {
                            await _messageDataSaver?.EditSavedMessage(args);
                            OnMessageEdited(args);
                        }
                        else
                           _logger.LogWarning("The message not found in the database", args);
                    }
                    else
                    {
                        await TrySetExistingUserId(args.User);
                        await TrySetExistingChatId(args.Chat);
                        await _messageDataSaver?.SaveNewMessageData(args);
                        OnMessageReceived(args);

                        // 1. Проверка авторизации
                        //if (!Authorization(tgMessage) && session.SessionCurrentStep != IsWaitingForPassword)
                        //    return;

                        // 2. Обрабатываем в зависимости от того, команда это или данные                           
                        if (_commandManager is not null
                            && BotCommand.IsCommand(args.Message.Text) 
                            && !await _commandManager.TryEnqueueCommandAsync(args.Message))
                        {
                            await AddMessageToOutbox(args.Chat, $"Unknown command '{args.Message.Text}'");
                        }
                        //else if (File)
                        //{
                        //
                        //}

                    }

                    msgForLog = null;
                }
            }
            catch (Exception ex)
            {
                ex.Data.Add("Message", msgForLog);
                _logger?.LogError(ex, "Input message processing error");
            }
        }

        private async Task ProcessOutputMessages(Task currentTask)
        {
            TgMessage msgForLog = null;
            try
            {
                while (_outputMessageBuffer.TryDequeue(out TgMessage tgMessage))
                {
                    msgForLog = tgMessage;
                    OperationResult sendingResult;

                    //lock (_locker)
                    //{
                        sendingResult = await SendMessageFinalyAsync(tgMessage, currentTask);
                    //}

                    if (sendingResult == OperationResult.Retry)
                        continue;

                    // When an error occured during sending
                    if (tgMessage.From is null)
                        tgMessage.From = await _botClient.GetMeAsync();

                    var args = new MessageActionEventArgs()
                    {
                        Message  = ItemConverter.ToGeneralMessage(tgMessage),
                        Chat     = ItemConverter.ToGeneralChat(tgMessage.Chat),
                        User     = ItemConverter.ToGeneralUser(tgMessage.From),
                        ChatType = ItemConverter.ToGeneralChatType(tgMessage.Chat.Type),
                        Action   = MessageAction.Sent
                    };

                    await TrySetExistingUserId(args.User);
                    await TrySetExistingChatId(args.Chat);
                    await _messageDataSaver?.SaveNewMessageData(args);
                    OnMessageSent(args);

                    msgForLog = null;
                }
            }
            catch (Exception ex)
            {
                ex.Data.Add("Message", msgForLog);
                _logger?.LogError(ex, "Output message processing error");
            }
        }
        
        /// <inheritdoc />
        public Task AddMessageToOutbox(Chat chat, string messageText, Message messageToReply = null)
        {
            try
            {
                if (chat is null)
                    throw new ArgumentNullException(nameof(chat));

                if (string.IsNullOrEmpty(messageText))
                    throw new ArgumentNullException(nameof(messageText), "Message must have a body!");

                var tgChat = System.Text.Json.JsonSerializer.Deserialize<TelegramChat>(chat.RawData);
                var tgMessage = messageToReply is { }
                              ? System.Text.Json.JsonSerializer.Deserialize<TelegramMessage>(messageToReply.RawData)
                              : null;

                var msg = new TgMessage(tgChat, messageText)
                {
                    ReplyToMessage = tgMessage
                };

                _outputMessageBuffer.Enqueue(msg);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Adding message to outbox error");
                throw;
            }
            return Task.CompletedTask;
        }
        
        /// <inheritdoc />
        public async Task AddMessageToOutboxAsync(string messageText, params string[] userRoleIds)
        {
            try
            {
                var dbUsers = await _usersRepo.FindAllAsync(u => userRoleIds.Contains(u.UserRoleId));

                var tgUsers = dbUsers.Select(u => System.Text.Json.JsonSerializer.Deserialize<TelegramUser>(u.RawData));

                var tgChats = (await _chatsRepo.FindAllAsync()).Select(c => System.Text.Json.JsonSerializer.Deserialize<TelegramChat>(c.RawData))
                    .Where(c => c.Id >= int.MinValue && c.Id <= int.MaxValue 
                             && tgUsers.Select(u => u.Id).Contains((int)c.Id)).ToList();

                //var userIds = string.Join(',', tgUsers.Select(u => u.Id));
                //var tgChat2 = ctx.Chats.FromSqlRaw(
                //    $"select * from bot.chats " +
                //    $"where cast(raw_data ->> 'Id' as bigint) in ({userIds})").ToList();

                foreach (var chat in tgChats)
                    _outputMessageBuffer.Enqueue(new TgMessage(chat, messageText));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Adding message to outbox error");
            }
        }
        
        /// <inheritdoc />
        public async Task<bool> DeleteMessage(Message dbMessage)
        {
            if (dbMessage is null)
                throw new ArgumentNullException(nameof(dbMessage));

            var dbChat = await _chatsRepo.FindAsync(c => c.Id == dbMessage.ChatId);
            if (dbChat == null)
            {
                _logger?.LogWarning($"Chat with ChatId = {dbMessage.ChatId} not found in database", dbMessage);
                return false;
            }

            return await DeleteMessageAsync(dbChat, dbMessage) == OperationResult.Success
                ? true
                : false;
        }

        /// <summary> Tries find the same item in the database and set it's Id  </summary>
        /// <param name="user"></param>
        /// <returns><see langword="true"/> if the <see cref="user"/> found in the database, otherwise false</returns>
        public async Task<bool> TrySetExistingUserId(User user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            var jObject = JObject.Parse(user.RawData);
            if (jObject.ContainsKey("Id"))
            {
                var tgUserId = (int)jObject["Id"];
                var identicalUser = await _usersRepo.FindBySqlAsync($"select * from bot.users where cast(raw_data ->> 'Id' as integer) = {tgUserId}");

                if (identicalUser is not null)
                {
                    user.Id = identicalUser.Id;
                    return true;
                }
            }

            return false;
        }

        /// <summary> Tries find the same item in the database and set it's Id  </summary>
        /// <param name="chat"></param>
        /// <returns><see langword="true"/> if the <see cref="chat"/> found in the database, otherwise false</returns>
        public async Task<bool> TrySetExistingChatId(Chat chat)
        {
            if (chat is null)
                throw new ArgumentNullException(nameof(chat));

            var jObject = JObject.Parse(chat.RawData);
            if (jObject.ContainsKey("Id"))
            {
                var tgChatId = (long)jObject["Id"];
                var identicalChat = await _chatsRepo.FindBySqlAsync($"select * from bot.chats where cast(raw_data ->> 'Id' as bigint) = {tgChatId}");

                if (identicalChat is not null)
                {
                    chat.Id = identicalChat.Id;
                    return true;
                }
            }

            return false;
        }

        /// <summary> Tries find the same item in the database and set it's Id  </summary>
        /// <param name="message"></param>
        /// <returns><see langword="true"/> if the <see cref="message"/> found in the database, otherwise false</returns>
        public async Task<bool> TrySetExistingMessageId(Message message)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            //Вместо изменения текущего сообщения, 
            //было создано новое с некорректными ChatId и UserId

            var jObject = JObject.Parse(message.RawData);
            if (jObject.ContainsKey("MessageId") && jObject.ContainsKey("ChatId"))
            {
                var tgMessageId = (int)jObject["MessageId"];
                var tgChatId = (long)jObject["ChatId"];
                var identicalMessage = await _messagesRepo.FindBySqlAsync(
                    $"select * from bot.messages " +
                    $" where cast(raw_data ->> 'MessageId' as integer) = {tgMessageId}" +
                    $"   and cast(raw_data ->> 'ChatId' as bigint) = {tgChatId}");

                if (identicalMessage is not null)
                {
                    message.Id = identicalMessage.Id;
                    return true;
                }
            }

            return false;
        }

        public async Task<OperationResult> DeleteMessageAsync(Chat chat, Message message)
        {
            try
            {
                if (chat == null)
                    throw new ArgumentNullException(nameof(chat));

                if (message == null)
                    throw new ArgumentNullException(nameof(message));

                var tgChat = System.Text.Json.JsonSerializer.Deserialize<TelegramChat>(chat.RawData);
                var tgMessage = System.Text.Json.JsonSerializer.Deserialize<TelegramMessage>(message.RawData);
                
                await _botClient.DeleteMessageAsync(tgChat.Id, tgMessage.MessageId);

                message.IsDeleted = true;
                var args = new MessageActionEventArgs()
                {
                    Chat = chat,
                    Message = message,
                    ChatType = ItemConverter.ToGeneralChatType(tgChat.Type),
                    Action = MessageAction.Deleted
                };

                await _messageDataSaver?.SaveNewMessageData(args);
                OnMessageDeleted(args);

                return OperationResult.Success;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Message deleting error", chat, message);
                return OperationResult.Failure;
            }
        }

        private async Task<OperationResult> SendMessageFinalyAsync(TgMessage message, Task currentTask)
        {
            // Telegram.Bot.API не позволяет отправлять сообщения, 
            // содержащие текст вида */command@botName*
            try
            {
                TelegramMessage tgMessage = null;

                switch (message.Type)
                {
                    case TelegramMessageType.Text:
                        if (string.IsNullOrWhiteSpace(message.Text))
                            throw new Exception("Text message have no text");

                        TelegramMessage tmp = message.ReplyToMessage is null
                                ? await _botClient.SendTextMessageAsync(
                                      message.Chat.Id,
                                      message.Text,
                                      ParseMode.Default)
                                : await _botClient.SendTextMessageAsync(
                                      message.Chat.Id,
                                      message.Text,
                                      ParseMode.Default,
                                      replyToMessageId: (int)message.ReplyToMessageId);
                        tgMessage = new TgMessage(tmp);
                        break;
                    default:
                        await _botClient.SendTextMessageAsync(
                            message.Chat.Id,
                            $"Unable to send message type of {message.Type}");
                        break;
                }

                
                // Сохраняем данные сообщения в БД
                if (tgMessage != null)
                {
                    message.Parse(tgMessage);
                    message.IsSucceed = true;
                }

                return OperationResult.Success;
            }
            catch (Exception ex)
            {
                if (ex is ApiRequestException)
                {
                    if (message.SendingFails > 1)
                    {
                        message.IsSucceed = false;
                        ex.Data.Add("Message", message);
                        _logger?.LogError(ex, "Message sending error");
                        return OperationResult.Failure;
                    }
                    else
                        currentTask.Wait(3000);
                }

                if (message.SendingFails < _sendingRetryLimit)
                {
                    message.FailDescription = JsonConvert.SerializeObject(ex, Formatting.Indented);
                    message.SendingFails++;
                    currentTask.Wait(2000 * message.SendingFails);
                    _outputMessageBuffer.Enqueue(message);
                    return OperationResult.Retry;
                }
                else
                {
                    try
                    {
                        message.IsSucceed = false;
                        ex.Data.Add("Message", message);
                        _logger?.LogError(ex, "Message sending error");
                        return OperationResult.Failure;
                    }
                    catch { return OperationResult.Failure; }
                }
            }
        }


        private void OnMessageEdited(MessageActionEventArgs args)
        {
            Volatile.Read(ref MessageEdited)?.Invoke(this, args);
        }
        private void OnMessageReceived(MessageActionEventArgs args)
        {
            Volatile.Read(ref MessageReceived)?.Invoke(this, args);
        }
        private void OnMessageSent(MessageActionEventArgs args)
        {
            Volatile.Read(ref MessageSent)?.Invoke(this, args);
        }
        private void OnMessageDeleted(MessageActionEventArgs args)
        {
            Volatile.Read(ref MessageDeleted)?.Invoke(this, args);
        }

    }
}
