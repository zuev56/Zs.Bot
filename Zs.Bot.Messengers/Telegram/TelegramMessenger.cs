using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Zs.Bot.Helpers;
using Zs.Bot.Model.Db;
using Zs.Bot.Modules.Messaging;
using Zs.Common.Enums;

namespace Zs.Bot.Telegram
{
    public class TelegramMessenger : IMessenger
    {
        private readonly Logger _logger = Logger.GetInstance();
        private readonly int _sendingRetryLimit = 5;
        private readonly TelegramBotClient _botClient;
        private readonly Buffer<TgMessage> _inputMessageBuffer = new Buffer<TgMessage>();
        private readonly Buffer<TgMessage> _outputMessageBuffer = new Buffer<TgMessage>();

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
                        continue;
                    }

                    OnMessageReceived(args);

                    msgForLog = null;
#if DEBUG
                    Trace.WriteLine($"InputMessageProcessed: {tgMessage?.Text}, ThreadId: {Thread.CurrentThread.ManagedThreadId}");
#endif
                }
            }
            catch (Exception e)
            {
                e.Data.Add("MessageId", msgForLog?.MessageId);
                _logger.LogError(e, nameof(TelegramMessenger));
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

                    sendingResult = SendMessageFinaly(tgMessage, currentTask);

                    if (sendingResult == OperationResult.Retry)
                        continue;

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
                var users = ctx.Users.Where(u => userRoleCodes.Contains(u.UserRoleCode))
                                 .ToList();// Для исключения Npgsql.NpgsqlOperationInProgressException: A command is already in progress

                var chats = ctx.Chats.Where(c => users.Select(u => u.UserId).Contains(c.ChatId)).ToList();

                var tgChats = chats.Select(c => System.Text.Json.JsonSerializer.Deserialize<Chat>(c.RawData));

                foreach (var chat in tgChats)
                    _outputMessageBuffer.Enqueue(new TgMessage(chat, messageText));
            }
            catch (Exception e)
            {
                _logger.LogError(e, nameof(TelegramMessenger));
            }
        }
        
        /// <inheritdoc />
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
                    ChatType = ItemConverter.ToGeneralChatType(tgMessage.Chat.Type),
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
            // Telegram.Bot.API не позволяет отправлять сообщения, содержащие вида */command@botName*
            try
            {
                Message tgMessage = null;

                switch (message.Type)
                {
                    case MessageType.Text:
                        if (string.IsNullOrWhiteSpace(message.Text))
                            throw new Exception("Text message have no text");

                        Message tmp = message.ReplyToMessage is null
                                ? _botClient.SendTextMessageAsync(
                                    message.Chat.Id,
                                    message.Text,
                                    ParseMode.Markdown).GetAwaiter().GetResult()
                                : _botClient.SendTextMessageAsync(
                                    message.Chat.Id,
                                    message.Text,
                                    ParseMode.Markdown,
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
            catch (Exception e)
            {
                if (e is ApiRequestException)
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
                    message.FailDescription = JsonConvert.SerializeObject(e, Formatting.Indented);
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
                        e.Data.Add("Message", message);
                        _logger.LogError(e, nameof(TelegramMessenger));
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
