using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zs.Bot.Model.Db;
using Zs.Service.ChatAdmin.Model;

namespace Zs.UnitTest.Services.ChatAdmin
{
    [TestClass]
    public class DbModelTest : DataBaseClient
    {
        [TestMethod]
        public void DbModel_SelectTest()
        {
            try
            {
                using (var ctx = _caContextFactory.GetContext())
                {
                    Assert.IsNotNull(ctx.Accountings.FirstOrDefault());
                    Assert.IsNotNull(ctx.AuxiliaryWords.FirstOrDefault());
                    Assert.IsNotNull(ctx.Bans.FirstOrDefault());
                    Assert.IsNotNull(ctx.Notifications.FirstOrDefault());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [TestMethod]
        public void DbModel_UpdateTest()
        {
            try
            {
                using var ctx = _caContextFactory.GetContext();
                
                var accounting   = ctx.Accountings.FirstOrDefault();
                var word         = ctx.AuxiliaryWords.FirstOrDefault();
                var ban          = ctx.Bans.FirstOrDefault();
                var notification = ctx.Notifications.FirstOrDefault();

                var accountingId   = accounting.AccountingId;
                var theWord        = word.TheWord;
                var banId          = ban.BanId;
                var notificationId = notification.NotificationId;

                var newUpdateDate = DateTime.Now;
                accounting.UpdateDate      = newUpdateDate;
                //word.UpdateDate            = newUpdateDate;
                ban.UpdateDate             = newUpdateDate;
                notification.UpdateDate    = newUpdateDate;

                ctx.SaveChanges();

                Assert.IsTrue(newUpdateDate == ctx.Accountings.First(a => a.AccountingId == accountingId).UpdateDate);
                //Assert.IsTrue(newUpdateDate == ctx.AuxiliaryWords.First(w => w.TheWord == theWord).UpdateDate);
                Assert.IsTrue(newUpdateDate == ctx.Bans.First(b => b.BanId == banId).UpdateDate);
                Assert.IsTrue(newUpdateDate == ctx.Notifications.First(r => r.NotificationId == notificationId).UpdateDate);
                
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [TestMethod]
        public void DbModel_FirstInsertTest()
        {
            try
            {
                int userId, chatId;
                using (var ctx = _botContextFactory.GetContext())
                {
                    userId = ctx.Users.FirstOrDefault()?.UserId ?? -99;
                    chatId = ctx.Chats.FirstOrDefault()?.ChatId ?? -99;
                }

                var accounting   = new DbAccounting    { AccountingId = -2, AccountingStartDate = DateTime.Now };
                var word         = new DbAuxiliaryWord { TheWord = "UNITTEST1" };
                var ban          = new DbBan           { BanId = -2, UserId = userId, ChatId = chatId};
                var notification = new DbNotification  { NotificationId = -2, NotificationMessage = "UNITTEST1", NotificationDay = 1, NotificationHour = 1, NotificationMinute = 1 };

                using (var ctx = _caContextFactory.GetContext())
                {
                    if (!ctx.Accountings.Any(a => a.AccountingId == accounting.AccountingId))
                        ctx.Accountings.Add(accounting);

                    if (!ctx.AuxiliaryWords.Any(w => w.TheWord == word.TheWord))
                        ctx.AuxiliaryWords.Add(word);

                    if (!ctx.Bans.Any(b => b.BanId == ban.BanId))
                        ctx.Bans.Add(ban);

                    if (!ctx.Notifications.Any(n => n.NotificationId == notification.NotificationId))
                        ctx.Notifications.Add(notification);

                        ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [TestMethod]
        public void DbModel_InsertDeleteTest()
        {
            try
            {
                int userId, chatId;
                using (var ctx = _botContextFactory.GetContext())
                {
                    userId = ctx.Users.FirstOrDefault()?.UserId ?? -99;
                    chatId = ctx.Chats.FirstOrDefault()?.ChatId ?? -99;
                }

                var accounting   = new DbAccounting    { AccountingId = -1, AccountingStartDate = DateTime.Now };
                var word         = new DbAuxiliaryWord { TheWord = "UNITTEST0" };
                var ban          = new DbBan           { BanId = -1, UserId = userId, ChatId = chatId };
                var notification = new DbNotification  { NotificationId = -1, NotificationMessage = "UNITTEST1", NotificationDay = 1, NotificationHour = 1, NotificationMinute = 1 };

                using (var ctx = _caContextFactory.GetContext())
                {
                    ctx.Accountings.Add(accounting);
                    ctx.AuxiliaryWords.Add(word);
                    ctx.Bans.Add(ban);
                    ctx.Notifications.Add(notification);

                    ctx.SaveChanges();
                }

                // �������� �� ��
                using (var ctx = _caContextFactory.GetContext())
                {
                    ctx.Accountings.Remove(accounting);
                    ctx.AuxiliaryWords.Remove(word);
                    ctx.Bans.Remove(ban);
                    ctx.Notifications.Remove(notification);

                    ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
