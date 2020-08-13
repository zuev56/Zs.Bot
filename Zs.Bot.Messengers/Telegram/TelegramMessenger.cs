using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Zs.Bot.Helpers;
using Zs.Bot.Model.Db;
using Zs.Bot.Modules.Messaging;
using Zs.Common.Enums;
using Zs.Common.Interfaces;

namespace Zs.Bot.Telegram
{
    public class TelegramMessenger : IMessenger
    {
        private readonly IZsLogger _logger = Logger.GetInstance();
        private readonly int _sendingRetryLimit = 5;
        private readonly TelegramBotClient _botClient;
        private readonly Buffer<TgMessage> _inputMessageBuffer = new Buffer<TgMessage>();
        private readonly Buffer<TgMessage> _outputMessageBuffer = new Buffer<TgMessage>();
        private readonly object _locker = new object();

        public event Action<MessageActionEventArgs> MessageEdited;
        public event Action<MessageActionEventArgs> MessageReceived;
        public event Action<MessageActionEventArgs> MessageSent;
        public event Action<MessageActionEventArgs> MessageDeleted;
        public bool ApiLogIsEnabled { get; set; } = false;
        public IToGenegalItemConverter ItemConverter { get; set; } = new ItemConverter();


        public TelegramMessenger(string token, IWebProxy webProxy = null)
        {
            try
            {
                _inputMessageBuffer.OnEnqueue += InputMessageBuffer_OnEnqueue;
                _outputMessageBuffer.OnEnqueue += OutputMessageBuffer_OnEnqueue;

                _botClient = webProxy != null
                           ? new TelegramBotClient(token, webProxy)
                           : new TelegramBotClient(token);

                _botClient.IsReceiving = true;
                _botClient.Timeout = TimeSpan.FromSeconds(5);

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
#if DEBUG
                ApiLogIsEnabled = false;
#endif
            }
            catch (Exception e)
            {
                var te = new TypeInitializationException(typeof(TelegramMessenger).FullName, e);
                _logger.LogError(te, nameof(TelegramMessenger));
            }
        }


        #region Обработчики событий TelegramBotClient

        private void BotClient_ApiResponseReceived(object sender, ApiResponseEventArgs e)
        {
            if (ApiLogIsEnabled)
                _logger.LogInfo("ApiResponseReceived", e.ResponseMessage, "Telegram.Bot.API");
        }

        private void BotClient_MakingApiRequest(object sender, ApiRequestEventArgs e)
        {
            if (ApiLogIsEnabled)
                _logger.LogInfo("MakingApiRequest", e, "Telegram.Bot.API");
        }

        private void BotClient_OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            if (ApiLogIsEnabled)
                _logger.LogInfo("OnCallbackQuery", e, "Telegram.Bot.API");
        }

        private void BotClient_OnInlineQuery(object sender, InlineQueryEventArgs e)
        {
            if (ApiLogIsEnabled)
                _logger.LogInfo("OnInlineQuery", e, "Telegram.Bot.API");
        }

        private void BotClient_OnInlineResultChosen(object sender, ChosenInlineResultEventArgs e)
        {
            if (ApiLogIsEnabled)
                _logger.LogInfo("OnInlineResultChosen", e, "Telegram.Bot.API");
        }

        private void BotClient_OnMessage(object sender, MessageEventArgs args)
        {
            try
            {
                if (ApiLogIsEnabled)
                    _logger.LogInfo("OnMessage", args, "Telegram.Bot.API");

                _inputMessageBuffer.Enqueue(new TgMessage(args.Message));
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Не удалось поместить входящее сообщение в буфер. TelegramMessageId={args?.Message?.MessageId}");
            }
        }

        private void BotClient_OnMessageEdited(object sender, MessageEventArgs e)
        {
            if (ApiLogIsEnabled)
                _logger.LogInfo("OnMessageEdited", e, "Telegram.Bot.API");

            _inputMessageBuffer.Enqueue(new TgMessage(e.Message));
        }

        private void BotClient_OnReceiveError(object sender, ReceiveErrorEventArgs e)
        {
            if (e?.ApiRequestException?.Message != "Request timed out")
                _logger.LogError(e.ApiRequestException, "Telegram.Bot.API");
        }

        private void BotClient_OnReceiveGeneralError(object sender, ReceiveGeneralErrorEventArgs e)
        {
            if (e.Exception is System.Net.Http.HttpRequestException)
                return;

            _logger.LogError(e.Exception, "Telegram.Bot.API");
        }

        private void BotClient_OnUpdate(object sender, UpdateEventArgs e)
        {
            if (ApiLogIsEnabled)
                _logger.LogInfo("OnUpdate", e, "Telegram.Bot.API");
        }

        #endregion
      
        private void InputMessageBuffer_OnEnqueue(object sender, TgMessage item)
        {
#if DEBUG
            Trace.WriteLine($"InputMessageEnqueue: {item?.Text}, ThreadId: {Thread.CurrentThread.ManagedThreadId}");
#endif
            Task task = null;
            task = Task.Run(() => ProcessInputMessages(task));
        }

        private void OutputMessageBuffer_OnEnqueue(object sender, TgMessage item)
        {
#if DEBUG
            Trace.WriteLine($"OutputMessageEnqueue: {item?.Text}, ThreadId: {Thread.CurrentThread.ManagedThreadId}");
#endif
            Task task = null;
            task = Task.Run(() => ProcessOutputMessages(task));
        }
      
        private void ProcessInputMessages(Task currentTask)
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

                    if (tgMessage.EditDate is { })
                    {
                        OnMessageEdited(args);
                    }
                    else
                    {
                        OnMessageReceived(args);
                    }

                    msgForLog = null;
#if DEBUG
                    Trace.WriteLine($"InputMessageProcessed: {tgMessage?.Text}, ThreadId: {Thread.CurrentThread.ManagedThreadId}");
#endif
                }
            }
            catch (Exception ex)
            {
                ex.Data.Add("MessageId", msgForLog?.MessageId);
                _logger.LogError(ex, nameof(TelegramMessenger));
            }
        }

        private void ProcessOutputMessages(Task currentTask)
        {
            TgMessage msgForLog = null;
            try
            {
                while (_outputMessageBuffer.TryDequeue(out TgMessage tgMessage))
                {
                    msgForLog = tgMessage;
                    OperationResult sendingResult;

                    lock (_locker)
                    {
                        sendingResult = SendMessageFinaly(tgMessage, currentTask);
                    }

                    if (sendingResult == OperationResult.Retry)
                        continue;

#warning Make awaitable!
                    // When an error occured during sending
                    if (tgMessage.From is null)
                        tgMessage.From = _botClient.GetMeAsync().GetAwaiter().GetResult();

                    var args = new MessageActionEventArgs()
                    {
                        Message  = ItemConverter.ToGeneralMessage(tgMessage),
                        Chat     = ItemConverter.ToGeneralChat(tgMessage.Chat),
                        User     = ItemConverter.ToGeneralUser(tgMessage.From),
                        ChatType = ItemConverter.ToGeneralChatType(tgMessage.Chat.Type),
                        Action   = MessageAction.Sent
                    };
                    OnMessageSent(args);

                    msgForLog = null;
#if DEBUG
                    Trace.WriteLine($"OutputMessageProcessed: {tgMessage?.Text}, Fails: {tgMessage.SendingFails}, ThreadId: {Thread.CurrentThread.ManagedThreadId}");
#endif
                }
            }
            catch (Exception e)
            {
                e.Data.Add("MessageId", msgForLog?.MessageId);
                _logger.LogError(e, nameof(TelegramMessenger));
            }
        }
        
        /// <inheritdoc />
        public void AddMessageToOutbox(IChat chat, string messageText, IMessage messageToReply = null)
        {
            try
            {
                if (chat is null)
                    throw new ArgumentNullException(nameof(chat));

                if (string.IsNullOrEmpty(messageText))
                    throw new ArgumentNullException(nameof(messageText), "Message must have a body!");

                var tgChat = System.Text.Json.JsonSerializer.Deserialize<Chat>(chat.RawData);
                var tgMessage = messageToReply is { }
                              ? System.Text.Json.JsonSerializer.Deserialize<Message>(messageToReply.RawData)
                              : null;

                var msg = new TgMessage(tgChat, messageText)
                {
                    ReplyToMessage = tgMessage
                };

                _outputMessageBuffer.Enqueue(msg);
            }
            catch (Exception e)
            {
                _logger.LogError(e, nameof(TelegramMessenger));
                throw;
            }
        }
        
        /// <inheritdoc />
        public void AddMessageToOutbox(string messageText, params string[] userRoleCodes)
        {
            try
            {
                using var ctx = new ZsBotDbContext();
                var dbUsers = ctx.Users.Where(u => userRoleCodes.Contains(u.UserRoleCode))
                                 .ToList();// Для исключения Npgsql.NpgsqlOperationInProgressException: A command is already in progress

                var tgUsers = dbUsers.Select(u => System.Text.Json.JsonSerializer.Deserialize<User>(u.RawData));

                var tgChats = ctx.Chats.ToList().Select(c => System.Text.Json.JsonSerializer.Deserialize<Chat>(c.RawData))
                    .Where(c => c.Id >= int.MinValue && c.Id <= int.MaxValue 
                             && tgUsers.Select(u => u.Id).Contains((int)c.Id)).ToList();

                //var userIds = string.Join(',', tgUsers.Select(u => u.Id));
                //var tgChat2 = ctx.Chats.FromSqlRaw(
                //    $"select * from bot.chats " +
                //    $"where cast(raw_data ->> 'Id' as bigint) in ({userIds})").ToList();


                foreach (var chat in tgChats)
                    _outputMessageBuffer.Enqueue(new TgMessage(chat, messageText));
            }
            catch (Exception e)
            {
                _logger.LogError(e, nameof(TelegramMessenger));
            }
        }
        
        /// <inheritdoc />
        public bool DeleteMessage(IMessage dbMessage)
        {
            if (dbMessage is null)
                throw new ArgumentNullException(nameof(dbMessage));

            using var ctx = new ZsBotDbContext();

            var dbChat = ctx.Chats.FirstOrDefault(c => c.ChatId == dbMessage.ChatId);
            if (dbChat == null)
            {
                _logger.LogWarning($"Chat with ChatId = {dbMessage.ChatId} not found in database", nameof(TelegramMessenger));
                return false;
            }

            return DeleteMessage(dbChat, dbMessage) == OperationResult.Success
                 ? true
                 : false;
        }

        /// <inheritdoc />
        public int? GetIdenticalUserId(IUser user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            using var ctx = new ZsBotDbContext();

            var jObject = JObject.Parse(user.RawData);
            if (jObject.ContainsKey("Id"))
            {
                var tgUserId = (int)jObject["Id"];
                var identicalUser = ctx.Users.FromSqlRaw($"select * from bot.users where cast(raw_data ->> 'Id' as integer) = {tgUserId}").FirstOrDefault();

                return identicalUser?.UserId;
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc />
        public int? GetIdenticalChatId(IChat chat)
        {
            if (chat is null)
                throw new ArgumentNullException(nameof(chat));

            using var ctx = new ZsBotDbContext();

            var jObject = JObject.Parse(chat.RawData);
            if (jObject.ContainsKey("Id"))
            {
                var tgChatId = (long)jObject["Id"];
                var identicalChat = ctx.Chats.FromSqlRaw($"select * from bot.chats where cast(raw_data ->> 'Id' as bigint) = {tgChatId}").FirstOrDefault();

                return identicalChat?.ChatId;
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc />
        public int? GetIdenticalMessageId(IMessage message)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            using var ctx = new ZsBotDbContext();

            //Вместо изменения текущего сообщения, 
            //было создано новое с некорректными ChatId и UserId

            var jObject = JObject.Parse(message.RawData);
            if (jObject.ContainsKey("MessageId") && jObject.ContainsKey("ChatId"))
            {
                var tgMessageId = (int)jObject["MessageId"];
                var tgChatId = (long)jObject["ChatId"];
                var identicalMessage = ctx.Messages.FromSqlRaw(
                    $"select * from bot.messages " +
                    $" where cast(raw_data ->> 'MessageId' as integer) = {tgMessageId}" +
                    $"   and cast(raw_data ->> 'ChatId' as bigint) = {tgChatId}").FirstOrDefault();

                return identicalMessage?.MessageId;
            }
            else
            {
                return null;
            }
        }

        public OperationResult DeleteMessage(IChat chat, IMessage message)
        {
            try
            {
                if (chat == null)
                    throw new ArgumentNullException(nameof(chat));

                if (message == null)
                    throw new ArgumentNullException(nameof(message));

                var tgChat = System.Text.Json.JsonSerializer.Deserialize<Chat>(chat.RawData);
                var tgMessage = System.Text.Json.JsonSerializer.Deserialize<Message>(message.RawData);
                
                _botClient.DeleteMessageAsync(tgChat.Id, tgMessage.MessageId).GetAwaiter().GetResult();

                message.IsDeleted = true;
                var args = new MessageActionEventArgs()
                {
                    Chat = chat,
                    Message = message,
                    ChatType = ItemConverter.ToGeneralChatType(tgChat.Type),
                    Action = MessageAction.Deleted
                };

                OnMessageDeleted(args);

                return OperationResult.Success;
            }
            catch (Exception e)
            {
                _logger.LogError(e, nameof(TelegramMessenger));
                return OperationResult.Failure;
            }
        }

        private OperationResult SendMessageFinaly(TgMessage message, Task currentTask)
        {
            // Telegram.Bot.API не позволяет отправлять сообщения, содержащие текст вида */command@botName*
            try
            {
                Message tgMessage = null;

                switch (message.Type)
                {
                    case MessageType.Text:
                        if (string.IsNullOrWhiteSpace(message.Text))
                            throw new Exception("Text message have no text");
#warning Make awaitable!
                        Message tmp = message.ReplyToMessage is null
                                ? _botClient.SendTextMessageAsync(
                                    message.Chat.Id,
                                    message.Text,
                                    ParseMode.Default).GetAwaiter().GetResult()
                                : _botClient.SendTextMessageAsync(
                                    message.Chat.Id,
                                    message.Text,
                                    ParseMode.Default,
                                    replyToMessageId: (int)message.ReplyToMessageId).GetAwaiter().GetResult();
                        tgMessage = new TgMessage(tmp);
                        break;
                    default:
                        _botClient.SendTextMessageAsync(
                                  message.Chat.Id,
                                  $"Unable to send message type of {message.Type} "
                                  ).GetAwaiter().GetResult();
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
                        _logger.LogError(ex, nameof(TelegramMessenger));
                        return OperationResult.Failure;
                    }
                    catch { return OperationResult.Failure; }
                }
            }
        }


        private void OnMessageEdited(MessageActionEventArgs args)
        {
            Volatile.Read(ref MessageEdited)?.Invoke(args);
        }
        private void OnMessageReceived(MessageActionEventArgs args)
        {
            Volatile.Read(ref MessageReceived)?.Invoke(args);
        }
        private void OnMessageSent(MessageActionEventArgs args)
        {
            Volatile.Read(ref MessageSent)?.Invoke(args);
        }
        private void OnMessageDeleted(MessageActionEventArgs args)
        {
            Volatile.Read(ref MessageDeleted)?.Invoke(args);
        }

    }
}
