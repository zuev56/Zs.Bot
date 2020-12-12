using System;
using Zs.Bot.Data.Models;

namespace Zs.Bot.Data.Factories
{
    public static class EntityFactory
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
