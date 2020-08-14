using System;
using Zs.Bot.Model.Db;

namespace Zs.Bot
{
    public static class DbEntityFactory
    {
        
        public static DbMessage NewMessage(string messageText = null, int chatId = default)
        {
            return new DbMessage
            {
                MessageText = messageText,
                ChatId = chatId,
                InsertDate = DateTime.Now,
                UpdateDate = DateTime.Now
            };
        }

        public static DbChat NewChat()
        {
            return new DbChat
            {
                InsertDate = DateTime.Now,
                UpdateDate = DateTime.Now
            };
        }

        public static DbUser NewUser()
        {
            return new DbUser
            {
                InsertDate = DateTime.Now,
                UpdateDate = DateTime.Now
            };
        }
    }
}
