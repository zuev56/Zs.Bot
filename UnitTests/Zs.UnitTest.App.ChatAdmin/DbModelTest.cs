using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zs.Bot.Model;
using Zs.App.ChatAdmin.Model;

namespace Zs.UnitTest.App.ChatAdmin
{
    [TestClass]
    public class DbModelTest : DataBaseClient
    {
        [TestMethod]
        public void DbModel_SelectTest()
        {
            try
            {
                using (var ctx = _contextFactory.GetChatAdminContext())
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
                using var ctx = _contextFactory.GetChatAdminContext();
                
                var accounting   = ctx.Accountings.FirstOrDefault();
                var word         = ctx.AuxiliaryWords.FirstOrDefault();
                var ban          = ctx.Bans.FirstOrDefault();
                var notification = ctx.Notifications.FirstOrDefault();

                var accountingId   = accounting.Id;
                var theWord        = word.TheWord;
                var banId          = ban.Id;
                var notificationId = notification.Id;

                var newUpdateDate = DateTime.Now;
                accounting.UpdateDate      = newUpdateDate;
                //word.UpdateDate            = newUpdateDate;
                ban.UpdateDate             = newUpdateDate;
                notification.UpdateDate    = newUpdateDate;

                ctx.SaveChanges();

                Assert.IsTrue(newUpdateDate == ctx.Accountings.First(a => a.Id == accountingId).UpdateDate);
                //Assert.IsTrue(newUpdateDate == ctx.AuxiliaryWords.First(w => w.TheWord == theWord).UpdateDate);
                Assert.IsTrue(newUpdateDate == ctx.Bans.First(b => b.Id == banId).UpdateDate);
                Assert.IsTrue(newUpdateDate == ctx.Notifications.First(r => r.Id == notificationId).UpdateDate);
                
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
                using (var ctx = _contextFactory.GetBotContext())
                {
                    userId = ctx.Users.FirstOrDefault()?.Id ?? -99;
                    chatId = ctx.Chats.FirstOrDefault()?.Id ?? -99;
                }

                var accounting   = new Accounting    { Id = -2, StartDate = DateTime.Now };
                var word         = new AuxiliaryWord { TheWord = "UNITTEST1" };
                var ban          = new Ban           { Id = -2, UserId = userId, ChatId = chatId};
                var notification = new Notification  { Id = -2, Message = "UNITTEST1", Day = 1, Hour = 1, Minute = 1 };

                using (var ctx = _contextFactory.GetChatAdminContext())
                {
                    if (!ctx.Accountings.Any(a => a.Id == accounting.Id))
                        ctx.Accountings.Add(accounting);

                    if (!ctx.AuxiliaryWords.Any(w => w.TheWord == word.TheWord))
                        ctx.AuxiliaryWords.Add(word);

                    if (!ctx.Bans.Any(b => b.Id == ban.Id))
                        ctx.Bans.Add(ban);

                    if (!ctx.Notifications.Any(n => n.Id == notification.Id))
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
                using (var ctx = _contextFactory.GetBotContext())
                {
                    userId = ctx.Users.FirstOrDefault()?.Id ?? -99;
                    chatId = ctx.Chats.FirstOrDefault()?.Id ?? -99;
                }

                var accounting   = new Accounting    { Id = -1, StartDate = DateTime.Now };
                var word         = new AuxiliaryWord { TheWord = "UNITTEST0" };
                var ban          = new Ban           { Id = -1, UserId = userId, ChatId = chatId };
                var notification = new Notification  { Id = -1, Message = "UNITTEST1", Day = 1, Hour = 1, Minute = 1 };

                using (var ctx = _contextFactory.GetChatAdminContext())
                {
                    ctx.Accountings.Add(accounting);
                    ctx.AuxiliaryWords.Add(word);
                    ctx.Bans.Add(ban);
                    ctx.Notifications.Add(notification);

                    ctx.SaveChanges();
                }

                // Удаление из БД
                using (var ctx = _contextFactory.GetChatAdminContext())
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
