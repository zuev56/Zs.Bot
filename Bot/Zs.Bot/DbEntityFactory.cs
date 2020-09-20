using System;
using Zs.Bot.Model;

namespace Zs.Bot
{
    public static class DbEntityFactory
    {
        
        public static Message NewMessage(string messageText = null, int chatId = default)
        {
            return new Message
            {
                Text = messageText,
                ChatId = chatId,
                InsertDate = DateTime.Now,
                UpdateDate = DateTime.Now
            };
        }

        public static Chat NewChat()
        {
            return new Chat
            {
                InsertDate = DateTime.Now,
                UpdateDate = DateTime.Now
            };
        }

        public static User NewUser()
        {
            return new User
            {
                InsertDate = DateTime.Now,
                UpdateDate = DateTime.Now
            };
        }
    }
}
