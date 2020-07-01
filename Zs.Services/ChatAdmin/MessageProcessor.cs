using System;
using System.Collections.Generic;
using System.Linq;
using Zs.Bot.Helpers;
using Zs.Bot.Model.Db;
using Zs.Bot.Modules.Messaging;
using Zs.Common.Interfaces;

namespace Zs.Service.ChatAdmin
{
    internal sealed class MessageProcessor
    {
        private int       _defaultChatId;                 // Идентификатор чата, с которым работает бот
        private int       _limitHi               = -1;    // Верхняя предупредительная уставка
        private int       _limitHiHi             = -1;    // Верхняя аварийная уставка
        private int       _limitAfterBan         =  5;    // Количество сообщений, доступное пользователю после бана до конца дня
        private int       _accauntingStartsAfter = -1;    // Общее количество сообщений в чате, после которого включается ограничитель
        private bool      _doNotBanAdmins        = true;  // Банить или не банить админов
        private bool      _limitsAreDefined;              // Нужен для понимания, были ли уже переопределены лимиты после восстановления интернета
        private DateTime? _accountingStartDate;           // Время начала учёта сообщений и включения ограничений
        private DateTime? _internetRepairDate;            // Время восстановления соединения с интернетом
        private IMessenger _messenger;
        private IZsLogger _logger = Logger.GetInstance();


        internal MessageProcessor(IZsConfiguration configuration, IMessenger messenger)
        {
            try
            {
                if (configuration is null)
                    throw new ArgumentNullException(nameof(configuration));

                if (messenger is null)
                    throw new ArgumentNullException(nameof(messenger));

                _messenger = messenger;
                _defaultChatId = configuration.Contains("DefaultChatId") ? checked((int)(long)configuration["DefaultChatId"]) : -1;
                _limitHi = configuration.Contains("MessageLimitHi") ? checked((int)(long)configuration["MessageLimitHi"]) : 25;
                _limitHiHi = configuration.Contains("MessageLimitHiHi") ? checked((int)(long)configuration["MessageLimitHiHi"]) : 30;
                _limitAfterBan = configuration.Contains("MessageLimitAfterBan") ? checked((int)(long)configuration["MessageLimitAfterBan"]) : 5;
                _accauntingStartsAfter = configuration.Contains("AccountingStartsAfter") ? checked((int)(long)configuration["AccountingStartsAfter"]) : 100;

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

                if (message.ChatId != _defaultChatId)
                    return;

                // _chat_id               integer,
                // _message_id            integer,
                // _accounting_start_date timestamp with time zone, -- важно переопределять во время выполнения
                // _msg_limit_hi          integer,                           -- важно переопределять во время выполнения
                // _msg_limit_hihi        integer,                         -- важно переопределять во время выполнения
                // _start_account_after   integer default 100)

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
                                     $"_start_account_after => {_accauntingStartsAfter}\n" +
                            ")";

                var jsonResult = ZsBotDbContext.GetStringQueryResult(query);


                var dictResult = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string,string>>(jsonResult);


                if (dictResult.ContainsKey("Action"))
                {
                    using var ctx = new ZsBotDbContext();
                    var chat = ctx.Chats.First(c => c.ChatId == message.ChatId);

                    if (dictResult.ContainsKey("MessageText"))
                    {
                        if (dictResult["MessageText"].Contains("<UserName>", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var userName = ctx.Users.FirstOrDefault(u => u.UserId == message.UserId)?.UserName;
                            dictResult["MessageText"] = dictResult["MessageText"].Replace("<UserName>", userName, StringComparison.InvariantCultureIgnoreCase);
                        }
                    }

                    //СДЕЛАТЬ РЕТЮРНЫ В ХРАНИМКЕ ЧЕРЕЗ КАЖДЫЕ НЕСКОЛЬКО СТРОК чтобы локализовать проблему

                    // if (_doNotBanAdmins)
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
                            //_accauntingStartsAfter = int.Parse(dictResult["AccountingStartAfter"]);
                            return;
                        case "SendMessageToGroup":
                            _messenger.AddMessageToOutbox(chat, dictResult["MessageText"], message);
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

        /// <summary> Обработка сообщений из группы </summary>
        // private void Bot_GroupChatMessageHandler(IDbReceivedMessage message)
        // {
        //     string logData = $"MessageId        = {message.ReceivedMessageId}"
        //                    + $"\nMessageText    = {message.ReceivedMessageText ?? ""}"
        //                    + $"\nChatId         = {message.ChatId}"
        //                    //+ $"\nMessageDateMsk = {message.Date + TimeSpan.FromHours(3)}"
        //                    + $"\nAccountingFrom = {_accountingStartDate}";
        //                    //+ $"\nUserId         = {message.From.Id}";
        // 
        //     _logger.LogInfo($"Начало обработки сообщения\n{logData}", "Групповое сообщение");
        // 
        //     #region Условия прекращения анализа сообщения
        //
        //     // Если не дефолтный чат - выходим
        //     if (message.Chat.Id != _defaultChatId)
        //     {
        //         _logger.LogInfo("Групповое сообщение", "Прекращение обработки сообщения: сообщение не из чата по умолчанию", logData, message.MessageId);
        //         return;
        //     }
        //
        //     // Если не задана предупредительная уставка, то нет смысла дёргать БД
        //     if (_messageLimitHi <= 0)
        //     {
        //         _logger.LogInfo("Групповое сообщение", "Прекращение обработки сообщения: не задан даже предупредительный лимит, нет смысла идти дальше", logData, message.MessageId);
        //         return;
        //     }
        //
        //     // Если интернет появился менее минуты назад, то не обрабатываем сообщения
        //     if ((DateTime.Now - _internetRepairDate)?.Minutes <= 1)
        //     {
        //         _logger.LogInfo("Групповое сообщение", "Прекращение обработки сообщения: интернет появился менее минуты назад", logData, message.MessageId);
        //         return;
        //     }
        //
        //     // Проверяем, начался ли уже учёт
        //     if (!AccountingHasBegan(message.Date + TimeSpan.FromHours(3)))
        //     {
        //         _logger.LogInfo("Групповое сообщение", "Прекращение обработки сообщения: учёт сообщений ещё не начался, нет смысла идти дальше", logData, message.MessageId);
        //         return;
        //     }
        //     #endregion
        // 
        //     // Временно. Для исключения такого условия
        //     if (_accountingStartDate < DateTime.Now.Date - TimeSpan.FromDays(1))
        //         throw new Exception($"_accountingStartDate = {_accountingStartDate}");
        // 
        //     try
        //     {
        //         // Проверка наличия пользователя в списке предупреждённых/забаненных сегодня
        //         DbBan ban = GetActiveBanForUser(message);
        // 
        //         using (var ctx = new RobotDbContext())
        //         {
        //             bool SendAsAnswer = false; // Определяет, писать пользователю по UserName или как ответ на сообщение
        // 
        //             // Определяем пользователя
        //             var user = ctx.Users.FirstOrDefault(u => u.UserId == message.From.Id);
        // 
        //             // Одменов не трогаем! (настраивается в БД)
        //             if (_doNotBanAdmins && (UserRole)Enum.Parse(typeof(UserRole), user.RoleName) >= UserRole.Administrator)
        //             {
        //                 _logger.LogInfo($"Прекращение обработки сообщения: сообщение от администратора\nMessageId = {message.MessageId}", "Групповое сообщение");
        //                 return;
        //             }
        // 
        //             // Определение имени, по  которому будем обращаться к участнику чата
        //             string userName = $"@{user.UserName}";
        //             if (user.UserName == null)
        //             {
        //                 userName = (user.UserFirstName ?? "") + " " + (user.UserLastName ?? "").Trim();
        //                 SendAsAnswer = true;
        //             }
        //             logData += $"\nUserName       = {userName}";
        // 
        //             // 0. Подсчёт сообщений пользователя в этой группе с даты начала учёта
        //             var shiftedAccountingDate = _accountingStartDate - TimeSpan.FromHours(3);
        //             int userMsgCount = ctx.ReceivedMessages
        //                 .Count(m => m.ChatId == _defaultChatId
        //                          && m.UserId == message.From.Id
        //                          && m.ReceivedMsgDate >= shiftedAccountingDate
        //                          && m.IsDeleted == false); // Считаем только неудалённые
        // 
        //             logData += $"\nUserMsgCount   = {userMsgCount}";
        // 
        //             _logger.LogInfo($"Начинаются проверки\nMessageId = {message.MessageId}", "Групповое сообщение");
        // 
        // 
        //             // 1. Если достигнут предупредительный лимит
        //             //    и СЕГОДНЯ предупреждений не было
        //             if (userMsgCount >= _messageLimitHi && ban == null)
        //             {
        //                 using (var casCtx = new ChatAdminDbContext())
        //                 {
        //                     bool IsWarned = ban?.IsWarned ?? false;
        // 
        //                     // Если ранее не предупреждали
        //                     if (!IsWarned)
        //                     {
        //                         // Добавляем запись о предупреждении (заготовку для бана)
        //                         casCtx.Bans.Add(new DbBan()
        //                         {
        //                             UserId = message.From.Id,
        //                             ChatId = message.Chat.Id,
        //                             IsWarned = true,
        //                             IsActive = true,
        //                             InsertDate = DateTime.Now,
        //                             UpdateDate = DateTime.Now
        //                         });
        //                         casCtx.SaveChanges();
        // 
        //                         // Считываем её, чтобы взять ID
        //                         ban = GetActiveBanForUser(message);
        // 
        //                         // Делаем предупреждение (либо указанием пользователя, либо ответом на сообщение)
        //                         _bot.Messenger.AddMessageToOutbox(
        //                             message.Chat,
        //                             $"{userName}, количеcтво сообщений, отправленных Вами с начала учёта: {userMsgCount}\n"
        //                             + $"Лимит сообщений до конца дня: {_messageLimitHiHi} (у Вас осталось {_messageLimitHiHi - userMsgCount}).\nПри достижении лимита будут введены ограничения",
        //                             tag: $"{ban?.BanId ?? -1}@{ban?.UserId ?? -1}", // Пометка, чтобы удалять предыдущие сообщения от бота для заданного пользователя
        //                             replyToMessageId: (SendAsAnswer ? message.MessageId : -1));
        //
        //                         _logger.LogInfo($"Прекращение проверки: сделано предупреждение о приближении к лимиту\nMessageId = {message.MessageId}", "Групповое сообщение");
        //
        //                         // После предупреждения выходим
        //                         return;
        //                     }
        //                 }
        //             }
        // 
        //             // 2. Если достигнут аварийный лимит или бот перезагрузился, переопределил лимиты, но пользователь был забанен ранее
        //             if (userMsgCount >= _messageLimitHiHi || (ban != null && ban.BanFinishDate > DateTime.Now))
        //             {
        //                 // Проверяем, был ли пользователь забанен ранее
        //                 // Если был и бан активен - удаляем сообщение
        //                 // Если был и бан закончился -> Если userMsgCount < (_messageLimitHiHi + _messageLimitAfterBan)
        //                 //                                   позволяем оставить сообщение
        //                 //                              Иначе удаляем сообщение
        // 
        //                 // 2.1 Если пользователь не был забанен ранее - баним
        //                 if (ban.BanFinishDate == null)
        //                 {
        //                     // Сообщаем пользователю, что он ограничен до окончания периода
        //                     string msg = "";
        //                     if (_banPeriod == BanPeriod.ForOneHour || _banPeriod == BanPeriod.ForThreeHours)
        //                         msg = $"{userName}, Вы превысили лимит сообщений до конца дня ({_messageLimitHiHi}). Все последующие сообщения {DbBan.PeriodToString(_banPeriod, 1)} будут удаляться.\n"
        //                             + $"Потом до конца дня у Вас будет {_messageLimitAfterBan} сообщений";
        //                     else if (_banPeriod == BanPeriod.UntilNextDay)
        //                         msg = $"{userName}, Вы превысили лимит сообщений до конца дня ({_messageLimitHiHi}). Все последующие сообщения до конца дня будут удаляться";
        //                     else
        //                         throw new NotImplementedException();
        // 
        //                     _bot.Messenger.AddMessageToOutbox(message.Chat,
        //                                                          msg,
        //                                                          tag: $"{ban.BanId}@{ban.UserId}", // Пометка, чтобы удалять предыдущие сообщения от бота для заданного пользователя
        //                                                          replyToMessageId: (SendAsAnswer ? message.MessageId : -1));
        // 
        //                     // Удаляем из чата предыдущее предупреждение для пользователя от бота, чтоб не захламлять 
        //                     DeleteLastWarningMessageForUser(ban);
        // 
        //                     // Правим запись в таблице банов - задаём время окончания бана 
        //                     using (var casCtx = new ChatAdminDbContext())
        //                     {
        //                         var curBan = casCtx.Bans.First(b => b.BanId == ban.BanId)
        //                                                 .BanFinishDate = GetBanFinishTime();
        //                         casCtx.SaveChanges();
        //                     }
        // 
        //                     _logger.LogInfo($"Пользователь получил первый бан за день\nMessageId = {message.MessageId}", "Групповое сообщение");
        // 
        //                 }
        // 
        //                 // 2.2 Если бан закончился к этому моменту
        //                 if (ban.BanFinishDate < DateTime.Now)
        //                 {
        //                     // Если израсходовано резервное количество сообщений (_messageLimitAfterBan)
        //                     if (userMsgCount >= _messageLimitHiHi + _messageLimitAfterBan)
        //                         using (var casCtx = new ChatAdminDbContext())
        //                         {
        //                             // Делаем бан до конца дня
        //                             casCtx.Bans.First(b => b.BanId == ban.BanId).BanFinishDate = DateTime.Today + TimeSpan.FromDays(1);
        //                             casCtx.SaveChanges();
        // 
        //                             _bot.Messenger.AddMessageToOutbox(message.Chat,
        //                                                                  $"{userName}, на сегодня Вы полностью исчерпали свой лимит сообщений ({_messageLimitHiHi} + {_messageLimitAfterBan}).",
        //                                                                  replyToMessageId: (SendAsAnswer ? message.MessageId : -1));
        // 
        //                             // Удаляем предыдущее предупреждение для пользователя от бота
        //                             DeleteLastWarningMessageForUser(ban);
        // 
        //                             _logger.LogInfo($"Пользователь получил второй бан - до конца дня\nMessageId = {message.MessageId}", "Групповое сообщение");
        //                         }
        //                 }
        // 
        //                 // 2.3 Если пользователь забанен в данный момент
        //                 if (ban.BanFinishDate > DateTime.Now)
        //                 {
        //                     // Доп. проверка 1
        //                     if (message.Date + TimeSpan.FromHours(3) > _accountingStartDate)
        //                     {
        //                         // Доп. проверка 2 - если сообщение было отправлено не более чем минуту назад
        //                         if ((DateTime.Now - message.Date + TimeSpan.FromHours(3)) > TimeSpan.FromMinutes(1))
        //                         {
        //                             // Удаляем сообщение из чата
        //                             if (_bot.Messenger.DeleteMessage(message.Chat.Id, message.MessageId))
        //                                 _logger.LogInfo($"Сообщение забаненного пользователя удалено\nMessageId = {message.MessageId}", "Групповое сообщение");
        //                             else
        //                                 _logger.LogInfo($"Не удалось удалить ообщение забаненного пользователя!\nMessageId = {message.MessageId}", "Групповое сообщение");
        //                         }
        //                         else
        //                             _logger.LogInfo($"Сообщение забаненного пользователя НЕ удалено, т.к. оно существует больше минуты!\nMessageId = {message.MessageId}", "Групповое сообщение");
        //                     }
        //                     else
        //                     {
        //                         logData += $"\ntgMessage.Date = {message.Date}";
        //                         _logger.LogInfo($"Сообщение забаненного пользователя НЕ удалено, т.к. его дата создания раньше даты начала учёта!\nMessageId = {message.MessageId}", "Групповое сообщение");
        //                     }
        //                 }
        //             }
        //         }
        // 
        //         logData += $"\nMessageLimitHiHi = {_messageLimitHiHi}";
        //         _logger.LogInfo($"Обработка завершена\nMessageId = {message.MessageId}", "Групповое сообщение");
        //     }
        //     catch (Exception ex)
        //     {
        //         try { _logger?.LogError(ex); }
        //         catch { }
        //     }
        // }










        /// <summary> Удаление старого предупреждения от бота, чтобы не захламлять чат </summary>
        // private void DeleteLastWarningMessageForUser(DbBan ban)
        // {
        //     try
        //     {
        //         DbSentMsg msg;
        //         using (var ctx = new RobotDbContext())
        //             msg = ctx.SentMessages.FirstOrDefault(m => m.SentMsgTag == $"{ban.BanId}@{ban.UserId}"
        //                                                     && m.InsertDate >= DateTime.Today
        //                                                     && m.IsDeleted == false);
        //        
        //         int msgId = msg?.SentMsgMessageId ?? -1;
        //
        //         if (msgId != -1)
        //             _bot.Messenger.DeleteMessage(ban.ChatId, msgId);
        //
        //     }
        //     catch (Exception ex)      
        //     {
        //         _logger.LogError(ex);
        //     }
        // }













        // /// <summary> Получение данных об актуальной конфигурации чата </summary>
        // private string GetBotConfiguration()
        // {
        //     string banPeriod = "";
        //     switch (_banPeriod)
        //     {
        //         case BanPeriod.ForOneHour:    banPeriod = "в течение 1-го часа"; break;
        //         case BanPeriod.ForThreeHours: banPeriod = "в течение 3-х часов"; break;
        //         case BanPeriod.UntilNextDay:  banPeriod = "до начала следующего дня"; break;
        //         case BanPeriod.Undefined:     throw new NotImplementedException("Период бана не может быть неопределённым!");
        //     }
        //
        //     string msg;
        //     if (_messageLimitHi <= 0)
        //         msg = "Ограничение количества сообщений *не установлено*";
        //     else if (_messageLimitHi > 0 && _messageLimitHiHi <= 0)
        //         msg = $"Пользователи будут получать только предупреждение после *{_messageLimitHi}-го* сообщения"
        //             + "\n\nРежим ограничения количества сообщений отключен";
        //     else if (_messageLimitHi > 0 && _messageLimitHiHi > 0)
        //     {
        //         msg = $"Пользователи будут получать предупреждение после *{_messageLimitHi}-го* сообщения.\n"
        //             + $"Все сообщения после *{_messageLimitHiHi}-го* будут удаляться {banPeriod}."
        //             + (_msgsBeforeRestriction > 0
        //               ? $"\n\nУчёт начнётся после *{_msgsBeforeRestriction}-го* сообщения в чате за день"
        //               : "\n\nУчёт сообщений ведётся с начала дня");
        //     }
        //     else
        //         throw new NotImplementedException($"{nameof(GetBotConfiguration)} такого исхода не ожидалось");
        //
        //     if ((_banPeriod == BanPeriod.ForOneHour || _banPeriod == BanPeriod.ForThreeHours)
        //         && _messageLimitHi > 0 && _messageLimitHiHi > 0)
        //         msg += $"\n\nПо окончании периода бана пользователю будет выделено *{_messageLimitAfterBan} сообщений* до конца дня";
        //
        //     return msg;
        // }





        /// <summary> Установка времени восстановления соединения с интернетом </summary>
        public void SetInternetRepairDate(DateTime? date)
       {
           _internetRepairDate = date;
       }

    }
}
