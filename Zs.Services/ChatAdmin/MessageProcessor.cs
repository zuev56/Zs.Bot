﻿using System;
using Zs.Bot.Model.Db;
using Zs.Common.Interfaces;

namespace Zs.Service.ChatAdmin
{
    internal sealed class MessageProcessor
    {
        private int       _defaultChatId;                 // Идентификатор чата, с которым работает бот
        private int       _limitHi               = -1;    // Верхняя предупредительная уставка
        private int       _limitHiHi             = -1;    // Верхняя аварийная уставка
        private int       _limitAfterBan         =  5;    // Количество сообщений, доступное пользователю после бана до конца дня
        private long      _accauntingStartsAfter = -1;    // Общее количество сообщений в чате, после которого включается ограничитель
        private bool      _doNotBanAdmins        = true;  // Банить или не банить админов
        private bool      _limitsAreDefined      = false; // Нужен для понимания, были ли уже переопределены лимиты после восстановления интернета
        private DateTime? _accountingStartDate   = null;  // Время начала учёта сообщений и включения ограничений
        private DateTime? _internetRepairDate    = null;  // Время восстановления соединения с интернетом
        private IZsLogger _logger;


        public MessageProcessor(int defaultChatId)
        {
            _defaultChatId = defaultChatId;
        }


        /// <summary> Инициализация </summary>
        private void Initialize()
        {
            //string logData = $"_messageLimitHiHi        = {_messageLimitHiHi}"
            //               + $"\n_messageLimitHi        = {_messageLimitHi}"
            //               + $"\n_doNotBanAdmins        = {_doNotBanAdmins}"
            //               + $"\n_msgsBeforeRestriction = {_msgsBeforeRestriction}"
            //               + $"\n_accountingStartDate   = {_accountingStartDate}"
            //               + $"\n_banPeriod             = {_banPeriod}";
            //
            //_logger.LogInfo($"Начало определения лимитов\n{logData}", "Установка лимитов");
            //
            //
            //using (var rCtx = new RobotDbContext())
            //using (var caCtx = new ChatAdminDbContext())
            //{
            //    _messageLimitHiHi      = int.Parse(rCtx.Options.FirstOrDefault(p => p.OptionName == "ChatUserMessageCountHiHi").OptionValue);
            //    _messageLimitHi        = int.Parse(rCtx.Options.FirstOrDefault(p => p.OptionName == "ChatUserMessageCountHi").OptionValue);
            //    _doNotBanAdmins        = bool.Parse(rCtx.Options.FirstOrDefault(p => p.OptionName == "DoNotBanAdmins").OptionValue);
            //    _msgsBeforeRestriction = int.Parse(rCtx.Options.FirstOrDefault(p => p.OptionName == "ActivateLimiterAfterMsgCount").OptionValue ?? "-1");
            //    _banPeriod             = (BanPeriod)Enum.Parse(typeof(BanPeriod), rCtx.Options.FirstOrDefault(p => p.OptionName == "BanPeriod").OptionValue);
            //    _accountingStartDate   = caCtx.Accountings.FirstOrDefault(a => a.AccountingStartDate.Date == DateTime.Now.Date)?.AccountingStartDate ?? DateTime.MinValue;
            //}
            //
            //// Находим максимальное количество УЧТЁННЫХ сообщений за сутки от одного пользователя
            //var mostActiveUsersAccounted = GetTopTenAccountedForChat(_defaultChatId);
            //
            //// Если админов нельзя банить, то не учитываем их сообщения
            //if (_doNotBanAdmins)
            //{
            //    var adminIds = ZsBot.GetUsersIdByRole(UserRole.Owner, UserRole.Administrator);
            //    foreach (var i in adminIds)
            //        if (mostActiveUsersAccounted.ContainsKey(i))
            //            mostActiveUsersAccounted.Remove(i);
            //}
            //
            //int userMaxMsgCountAccounted = mostActiveUsersAccounted.Count() > 0
            //                    ? mostActiveUsersAccounted.Max(i => i.Value)
            //                    : 0;
            //
            //// Если максимальное количество уже имеющихся сообщений больше уставок из базы - корректируем уставки
            //if (_messageLimitHi < userMaxMsgCountAccounted || _messageLimitHiHi < userMaxMsgCountAccounted)
            //{
            //    _messageLimitHi = _messageLimitHi > 0 ? userMaxMsgCountAccounted + 1 : 0;    // Только если ограничение было задано
            //    _messageLimitHiHi = _messageLimitHi > 0 ? _messageLimitHi + 5 : 0;    // Только если ограничение было задано
            //}
            //
            //
            //var dayBeginDateTime = DateTime.Today - TimeSpan.FromHours(3); // Без этого ругается :(
            //using (var ctx = new RobotDbContext())
            //{
            //    int dailyMsgCount = ctx.ReceivedMessages
            //                    .Where(m => m.ReceivedMsgDate >= dayBeginDateTime
            //                             && m.ChatId == _defaultChatId
            //                             && m.IsDeleted == false).Count();
            //    _msgsBeforeRestriction = dailyMsgCount > _msgsBeforeRestriction
            //                           ? dailyMsgCount + 2
            //                           : _msgsBeforeRestriction;
            //}
            //
            //logData = $"userMaxMsgCount          = {userMaxMsgCountAccounted}"
            //        + $"\n_messageLimitHiHi      = {_messageLimitHiHi}"
            //        + $"\n_messageLimitHi        = {_messageLimitHi}"
            //        + $"\n_doNotBanAdmins        = {_doNotBanAdmins}"
            //        + $"\n_msgsBeforeRestriction = {_msgsBeforeRestriction}"
            //        + $"\n_accountingStartDate   = {_accountingStartDate}"
            //        + $"\n_banPeriod             = {_banPeriod}";
            

            // Оповещение владельцев и администраторов о переопределении лимитов
            //Initialized?.Invoke(GetBotConfiguration());
            
            //_logger.LogInfo($"Конец определения лимитов\n{logData}", "Установка лимитов");
        }

        internal void TestInit()
        {
            _defaultChatId = 0;
            _limitHi = 25;
            _limitHiHi = 30;
            _limitAfterBan = 5;
            _accauntingStartsAfter = 100;
            _doNotBanAdmins = false;
            _limitsAreDefined = false;
            _accountingStartDate = null;
            _internetRepairDate = null;
        }

        internal void ProcessGroupMessage(IMessage message)
        {
           // _chat_id               integer,
           // _message_id            integer,
           // _accounting_start_date timestamp with time zone, -- важно переопределять во время выполнения
           // _msg_limit_hi          integer,                           -- важно переопределять во время выполнения
           // _msg_limit_hihi        integer,                         -- важно переопределять во время выполнения
           // _start_account_after   integer default 100)

            var query = "select zl.sf_process_group_message(\n" +
                                     $"_chat_id => {message.ChatId},\n" +
                                     $"_message_id => {message.MessageId},\n" +
                                     $"_accounting_start_date => {_accountingStartDate?.ToString() ?? "null"},\n" +
                                     $"_msg_limit_hi => {_limitHi},\n" +
                                     $"_msg_limit_hihi => {_limitHiHi},\n" +
                                     $"_start_account_after => {_accauntingStartsAfter}\n" +
                        ")";
            var result = ZsBotDbContext.GetStringQueryResult(query);

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







        /// <summary> Возвращает активный бан для пользователя, написавшего сообщение или null, если бана нет </summary>
        // private static DbBan GetActiveBanForUser(InMessage tgMessage)
        // {
        //     try
        //     {
        //         DbBan ban = null;
        //         using (var casCtx = new ChatAdminDbContext())
        //             ban = casCtx.Bans.OrderByDescending(b => b.InsertDate)
        //                              .FirstOrDefault(b => b.UserId == tgMessage.From.Id
        //                                                && b.ChatId == tgMessage.Chat.Id
        //                                                && b.BanIsActive
        //                                                && b.InsertDate > DateTime.Today);
        //         return ban;
        //
        //     }
        //     catch
        //     {
        //         throw;
        //         //return null;
        //     }
        // }







        // /// <summary> Выбор самых активны с начала учёта </summary>
        // private Dictionary<int, int> GetTopTenAccountedForChat(long chatId)
        // {
        //     var dict = new Dictionary<int, int>();
        //
        //     // Если время учёта ещё не началось (порог свободного количества сообщений (100) не преодолён
        //     if (_accountingStartDate == DateTime.MinValue)
        //         return dict;
        //
        //
        //     DbChat chat;
        //     IQueryable<DbReceivedMessage> accountedChatMessages;
        //     using (var ctx = new RobotDbContext())
        //     {
        //         chat = ctx.Chats.FirstOrDefault(c => c.ChatId == chatId);
        //         
        //         // Если дата не была задана, берём отчет от начала дня
        //         var shiftedAccountingDate = _accountingStartDate == DateTime.MinValue
        //                                   ? DateTime.Now.Date - TimeSpan.FromHours(3)
        //                                   : _accountingStartDate - TimeSpan.FromHours(3);
        //         
        //         accountedChatMessages = ctx.ReceivedMessages
        //                                    .Where(m => m.ReceivedMsgDate >= shiftedAccountingDate
        //                                             && m.ChatId == chat.ChatId
        //                                             && m.IsDeleted == false);
        //     
        //
        //         if (accountedChatMessages.Count() == 0)
        //             return dict;
        //         
        //         if (chat.ChatType != ChatType.Private.ToString())
        //         {
        //             var userId_msgCount_Pair = from m in accountedChatMessages.GroupBy(msg => msg.UserId)
        //                                        .OrderByDescending(msg => msg.Where(ms => ms.IsDeleted == false).Count())
        //                                        join u in ctx.Users on m.Key equals u.UserId
        //                                        select new
        //                                        {
        //                                            u.UserId,
        //                                            MessageCount = m.Where(msg => msg.IsDeleted == false).Count()
        //                                        };
        //             userId_msgCount_Pair = userId_msgCount_Pair.Take(10);
        //         
        //             foreach (var item in userId_msgCount_Pair)
        //                 dict.Add(item.UserId, item.MessageCount);
        //         }
        //     }
        //     return dict;
        // }









        // /// <summary> Проверяем, начался ли уже учёт и надо ли его начать </summary>
        // private bool AccountingHasBegan(DateTime messageDateMsk)
        // {
        //     using (var ctx = new RobotDbContext())
        //     {
        //         // 1. Если порог общего кол-ва сообщений задан, ограничение будет включено при достижении этого порога
        //         if (_msgsBeforeRestriction > 0)
        //         {
        //             // Подсчёт всех сообщений в этой группе за день
        //             int chatMsgCount = ctx.ReceivedMessages
        //                 .Count(m => m.ChatId == _defaultChatId
        //                          && m.ReceivedMsgDate > DateTime.Today - TimeSpan.FromHours(3));
        //
        //             // Если ещё не активирован режим ограничения - активируем
        //             // это условие будто не всегда выполняется
        //             if (chatMsgCount == _msgsBeforeRestriction
        //                 || (chatMsgCount > _msgsBeforeRestriction && _accountingStartDate == DateTime.MinValue)) // Когда каким-то образом пропустили момент с равенством
        //             {
        //                 // Посылаем предупреждение в чат
        //                 var chat = ctx.Chats.FirstOrDefault(c => c.ChatId == _defaultChatId);
        //                 _bot.Messenger.AddMessageToOutbox(chat.GetTelegramType(), $"В чате уже {chatMsgCount} сообщений. Начинаю персональный учёт.");
        //                 _accountingStartDate = messageDateMsk + TimeSpan.FromSeconds(1);
        //
        //                 // Запись в БД
        //                 using (var casCtx = new ChatAdminDbContext())
        //                 {
        //                     casCtx.Accountings.Add(new DbAccounting() { StartDate = _accountingStartDate, UpdateDate = DateTime.Now });
        //                     casCtx.SaveChanges();
        //                 }
        //             }
        //
        //             // Преодолён порог общего количества сообщений?
        //             if (chatMsgCount > _msgsBeforeRestriction)
        //                 return true;
        //             else
        //                 return false;
        //         }
        //         // 2. Если порог общего кол-ва сообщений НЕ был задан, ограничение будет работать постоянно
        //         else
        //         {
        //             _accountingStartDate = DateTime.Now.Date; // С начала дня
        //
        //             // Запись в БД
        //             using (var casCtx = new ChatAdminDbContext())
        //             {
        //                 casCtx.Accountings.Add(new DbAccounting() { StartDate = _accountingStartDate, UpdateDate = DateTime.Now });
        //                 casCtx.SaveChanges();
        //             }
        //
        //             return true;
        //         }
        //     }
        // }









        // /// <summary> В зависимости от настроек бота рассчитывает время, когда пользователь будет разлочен </summary>
        // private DateTime GetBanFinishTime()
        // {
        //     switch (_banPeriod)
        //     {
        //         case BanPeriod.ForOneHour:    return DateTime.Now + TimeSpan.FromHours(1);
        //         case BanPeriod.ForThreeHours: return DateTime.Now + TimeSpan.FromHours(3);
        //         case BanPeriod.UntilNextDay:  return DateTime.Now.Date + TimeSpan.FromDays(1);
        //         default:                      return DateTime.Now; // Бан как бы заканчивается прямо щас
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
