using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zs.Bot.Helpers;
using Zs.Bot.Model.Db;
using Zs.Bot.Modules.Messaging;
using Zs.Common.Interfaces;
using Zs.Service.ChatAdmin.DbModel;

namespace Zs.Service.ChatAdmin
{
    internal sealed class MessageProcessor
    {
        private int       _defaultChatId;                 // Идентификатор чата, с которым работает бот
        private int       _limitHi               = -1;    // Верхняя предупредительная уставка
        private int       _limitHiHi             = -1;    // Верхняя аварийная уставка
        private int       _limitAfterBan         =  5;    // Количество сообщений, доступное пользователю после бана до конца дня
        private int       _accountingStartsAfter = -1;    // Общее количество сообщений в чате, после которого включается ограничитель
        private bool      _doNotBanAdmins        = true;  // Банить или не банить админов
        private bool      _limitsAreDefined;              // Нужен для понимания, были ли уже переопределены лимиты после восстановления интернета
        private DateTime? _accountingStartDate;           // Время начала учёта сообщений и включения ограничений
        private DateTime? _internetRepairDate;            // Время восстановления соединения с интернетом
        private readonly IMessenger _messenger;
        private readonly IZsLogger _logger = Logger.GetInstance();
        private readonly IZsConfiguration _configuration;
        private readonly bool _detailedLogging;
        private readonly int _waitAfterConnectionRepairedSec = 5;

        public event Action<string> LimitsDefined;


        internal MessageProcessor(IZsConfiguration configuration, IMessenger messenger)
        {
            try
            {
                if (configuration is null)
                    throw new ArgumentNullException(nameof(configuration));

                if (messenger is null)
                    throw new ArgumentNullException(nameof(messenger));

                _configuration = configuration;
                _messenger = messenger;
                _defaultChatId = _configuration.Contains("DefaultChatId") ? checked((int)(long)configuration["DefaultChatId"]) : -1;

                if (_configuration.Contains("DetailedLogging"))
                    bool.TryParse(_configuration["DetailedLogging"].ToString(), out _detailedLogging);

                ResetLimits();
            }
            catch (Exception ex)
            {
                throw new TypeInitializationException(typeof(MessageProcessor).FullName, ex);
            }        
        }

        internal void ProcessGroupMessage(IMessage message)
        {
            try
            {
                if (message is null)
                    throw new ArgumentNullException(nameof(message));

                if (!_limitsAreDefined)
                {
                    _logger.LogInfo($"Limits are not defined. Group message won't be processed", nameof(MessageProcessor));
                    return;
                }

                if ((DateTime.Now - _internetRepairDate)?.TotalSeconds <= _waitAfterConnectionRepairedSec)
                {
                    _logger.LogInfo($"The message [{message.MessageId}] won't be processed. Internet was restored less then {_waitAfterConnectionRepairedSec} seconds ago", nameof(MessageProcessor));
                    return;
                }
                
                if (message.ChatId != _defaultChatId)
                    return;

                var accountingStartDate = _accountingStartDate is null
                    ? "null"
                    : $"'{_accountingStartDate}'";

                var query = "select zl.sf_process_group_message(\n" +
                                     $"_chat_id => {message.ChatId},\n" +
                                     $"_message_id => {message.MessageId},\n" +
                                     $"_accounting_start_date => {accountingStartDate},\n" +
                                     $"_msg_limit_hi => {_limitHi},\n" +
                                     $"_msg_limit_hihi => {_limitHiHi},\n" +
                                     $"_msg_limit_after_ban => {_limitAfterBan},\n" +
                                     $"_start_account_after => {_accountingStartsAfter}\n" +
                            ")";

                var jsonResult = ZsBotDbContext.GetStringQueryResult(query);

                if (_detailedLogging)
                {
                    var logData = new Dictionary<string, object>
                    {
                        { "JsonResult", jsonResult },
                        { "MessageId", message.MessageId }
                    };
                    _logger.LogInfo("Got message sql-process result", logData, nameof(MessageProcessor));
                }

                var dictResult = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string,string>>(jsonResult);

                if (dictResult.ContainsKey("Action"))
                {
                    using var ctx = new ZsBotDbContext();
                    var chat = ctx.Chats.First(c => c.ChatId == message.ChatId);

                    if (dictResult.ContainsKey("MessageText"))
                    {
                        if (dictResult["MessageText"].Contains("<UserName>", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var dbUser = ctx.Users.FirstOrDefault(u => u.UserId == message.UserId);
                            var userName = dbUser?.UserName != null
                                ? $"@{dbUser.UserName}"
                                : dbUser?.UserFullName ?? "UserName";
                            dictResult["MessageText"] = dictResult["MessageText"].Replace("<UserName>", userName, StringComparison.InvariantCultureIgnoreCase);
                        }
                    }

                    switch (dictResult["Action"])
                    {
                        case "Continue": return;
                        case "DeleteMessage":
                            if (!_messenger.DeleteMessage(message))
                            {
                                _messenger.AddMessageToOutbox("Message deleting failed", "OWNER", "ADMIN");
                                _logger.LogWarning("Message deleting failed", message, nameof(MessageProcessor));
                            }
                            return;
                        case "SetAccountingStartDate":
                            _messenger.AddMessageToOutbox(chat, dictResult["MessageText"]);
                            _accountingStartDate = DateTime.Parse(dictResult["AccountingStartDate"]) + TimeSpan.FromSeconds(2);
                            return;
                        case "SendMessageToGroup":
                            _messenger.AddMessageToOutbox(chat, dictResult["MessageText"], message);

                            if (dictResult.ContainsKey("BanId") && int.TryParse(dictResult["BanId"], out int banId))
                            {
                            //    using (var ctxZl = new ChatAdminDbContext())
                            //    {
                            //        var ban = ctxZl.Bans.FirstOrDefault(b => b.BanId == banId);
                            //
                            //        if (ban.WarningMessageId != default)
                            //        {
                            //            var oldWarningMessage = ctx.Messages.FirstOrDefault(m => m.MessageId == ban.WarningMessageId);
                            //            _messenger.DeleteMessage(oldWarningMessage);
                            //        }
                            //    #warning Very bad decision...
                            //        var warningMessage = ctx.Messages
                            //            .FirstOrDefault(m => m.ReplyToMessageId == message.MessageId
                            //                              && m.MessageText.Contains(dictResult["MessageText"]));
                            //
                            //        ban.WarningMessageId = warningMessage?.MessageId;
                            //        ctxZl.SaveChanges();
                            //    } 
                            }
                            return;
                        case "SendMessageToOwner":
                            _messenger.AddMessageToOutbox(dictResult["MessageText"], "OWNER", "ADMIN");
                            return;
                        default:
                            _messenger.AddMessageToOutbox("Unknown action", "OWNER", "ADMIN"); 
                            break;
                    }
                }
                else
                {
                    _messenger.AddMessageToOutbox("Unknown message process result", "OWNER", "ADMIN");
                }
                
                // Начало индивидуального учёта после 100 сообщений в чате 
                // от любых пользователей с 00:00 текущего дня
                //       С начала учёта каждому доступно максимум 30 сообщений.
                //       После 25-го сообщения с начала учёта выдать пользователю 
                //           предупреждение о приближении к лимиту. При этом создаётся запись
                //           в таблице Ban и ставится пометка о том, что пользователь предупреждён
                //       При достижении лимита пользователь банится на 3 часа. 
                //           Если лимит достигнут ближе к концу дня, бан продолжает своё действие 
                //           до окончания трёхчасового периода. Если 3 часа бана прошло, 
                //           а день не закончился, позволяем пользователю отправку 5-ти сообщений 
                //           до начала следующего дня
                //       
                //       После восстановления интернета через 1 минуту происходит 
                //           переопределение лимитов для того, чтобы не перетереть 
                //           только что полученные сообщения
                //     

            }
            //catch (PostgresException pex)
            //{
            //    
            //}
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(MessageProcessor));
            }
        }

        /// <summary> Reset parameters to theirs configuration values </summary>
        public void ResetLimits()
        {
            try
            {
                if (_configuration is null)
                    throw new ArgumentNullException(nameof(_configuration));

                _limitHi = checked((int)(long)_configuration["MessageLimitHi"]);
                _limitHiHi = checked((int)(long)_configuration["MessageLimitHiHi"]);
                _limitAfterBan = checked((int)(long)_configuration["MessageLimitAfterBan"]);
                _accountingStartsAfter = checked((int)(long)_configuration["AccountingStartsAfter"]);
                _accountingStartDate = null;

                if (_detailedLogging)
                {
                    var logData = new Dictionary<string, int>()
                    {
                        { "MessageLimitHi", _limitHi },
                        { "MessageLimitHiHi", _limitHiHi },
                        { "MessageLimitAfterBan", _limitAfterBan },
                        { "AccountingStartsAfter", _accountingStartsAfter }
                    };
                    _logger.LogInfo("Limits set from configuration file", logData, nameof(MessageProcessor));
                }

                Volatile.Read(ref LimitsDefined)?.Invoke(GetLimitInfo());
            }
            catch (Exception ex)
            {
                _limitHi = -1;
                _limitHiHi = -1;
                _limitAfterBan = -1;
                _accountingStartsAfter = -1; 
                _accountingStartDate = null;
                _logger.LogError(ex, nameof(MessageProcessor));
            }
        }

        /// <summary> Set date when connection to the internet was repaired </summary>
        public async Task SetInternetRepairDate(DateTime? date)
        {
            _internetRepairDate = date;

            if (_internetRepairDate is null)
            {
                _limitsAreDefined = false;
                _accountingStartDate = null;

                // Remove today Bans where BanFinishDate is null (warnings before ban)
                using var ctx = new ChatAdminDbContext();
                var warnings = await ctx.Bans
                    .Where(b => b.InsertDate > DateTime.Today
                             && b.BanFinishDate == null).ToListAsync();
                if (warnings.Count > 0)
                {
                    ctx.Bans.RemoveRange(warnings);
                    await ctx.SaveChangesAsync();
                }

                _logger.LogWarning("The internet connection has been lost. Today's ban-warnings removed", nameof(MessageProcessor));
            }
            else
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Factory.StartNew(async () =>
                {
                    await Task.Delay(_waitAfterConnectionRepairedSec * 1000);
                    DefineActualLimits();

                    Volatile.Read(ref LimitsDefined)?.Invoke(GetLimitInfo());
                });
#pragma warning restore CS4014
            }
        }

        /// <summary> Defines the limits depending on current situation </summary>
        private void DefineActualLimits()
        {
            int maxAcountedMessagesFromUser = -1;
            int dailyMsgCount = -1;
            try
            {
                // 1. Accounting has not started yet
                //   1.1. After service start => return
                //   1.2. After disconnecting from the internet => override limits
                // 2. Accounting has already started => override limits

                var logDataBefore = new Dictionary<string, object>()
                {
                    {"_limitHi", _limitHi },
                    {"_limitHiHi", _limitHiHi},
                    {"_accauntingStartsAfter", _accountingStartsAfter},
                    {"_accountingStartDate", _accountingStartDate}
                };

                _logger.LogInfo("Limits definition started", logDataBefore, nameof(MessageProcessor));

                using (var ctx = new ZsBotDbContext())
                {
                    // Telegram message date is GMT. But now everything depends on the InsertDate.

                    var selectDailyMessages = $"select * from bot.messages where chat_id = {_defaultChatId} and cast(raw_data ->> 'Date' as timestamptz) > now()::date";
                    var selectAccountedDailyMessages = _accountingStartDate is null
                        ? "select * from bot.messages where user_id = -666" // something unreal to get nothing
                        : $"{selectDailyMessages} and cast(raw_data ->> 'Date' as timestamptz) > '{_accountingStartDate}'";

                    var userMessageCounts = ctx.Messages
                        .FromSqlRaw(selectAccountedDailyMessages)
                        .GroupBy(m => m.UserId)
                        .Select(m => new { UserId = m.Key, Count = m.Count() });

                    maxAcountedMessagesFromUser = userMessageCounts.Count() > 0
                        ? userMessageCounts.Max(i => i.Count)
                        : 0;

                    dailyMsgCount = ctx.Messages
                        .FromSqlRaw(selectDailyMessages)
                        .Where(m => !m.IsDeleted).Count();

                    _accountingStartsAfter = dailyMsgCount > _accountingStartsAfter
                        ? dailyMsgCount + 2
                        : _accountingStartsAfter;

                    if (_accountingStartsAfter > dailyMsgCount)
                        _accountingStartDate = null;

                    // if the accounting hasn't started today, keep old limits
                    var configAccountingStartsAfter = checked((int)(long)_configuration["AccountingStartsAfter"]);

                    if (_accountingStartDate == null
                        && _accountingStartsAfter == configAccountingStartsAfter)
                    {
                        _limitsAreDefined = true;
                        return;
                    }

                    if (_limitHi < maxAcountedMessagesFromUser || _limitHiHi < maxAcountedMessagesFromUser)
                    {
                        _limitHi = _limitHi > 0 ? maxAcountedMessagesFromUser + 2 : -1;
                        _limitHiHi = _limitHi > 0 ? _limitHi + 5 : -1;
                    }
                }

                _limitsAreDefined = true;
            }
            catch (Exception ex)
            {
                _limitHi = -1;
                _limitHiHi = -1;
                _accountingStartsAfter = -1;
                _accountingStartDate = null;
                _limitsAreDefined = false;
                _logger.LogError(ex, nameof(MessageProcessor));
            }
            finally
            {
                var logDataAfter = new Dictionary<string, object>()
                {
                    {"_limitHi", _limitHi },
                    {"_limitHiHi", _limitHiHi},
                    {"_accountingStartsAfter", _accountingStartsAfter},
                    {"_accountingStartDate", _accountingStartDate},
                    {"maxAcountedMessagesFromUser", maxAcountedMessagesFromUser},
                    {"dailyMsgCount", dailyMsgCount}
                };

                _logger.LogInfo("Limits definition finished", logDataAfter, nameof(MessageProcessor));
            }
        }

        /// <summary> Get user-friendly information string about limits </summary>
        private string GetLimitInfo()
        {
            var accountingStatus = _accountingStartDate is null
                ? "Accounting not started."
                : $"Accounting started at {_accountingStartDate}.";

            return $"Warning after {_limitHi} messages.\n"
                 + $"Ban for three hours after {_limitHiHi} messages.\n"
                 + $"Accounting starts after {_accountingStartsAfter} messages per day.\n"
                 + accountingStatus;
        }


    }
}
