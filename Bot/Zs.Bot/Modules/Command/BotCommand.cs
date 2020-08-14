using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zs.Bot.Model.Db;

namespace Zs.Bot.Modules.Command
{
    /// <summary>
    /// Команда, которую понимает бот. 
    /// Совпадает с конандой из БД только по имени. Это вообще другой объект
    /// </summary>
    public class BotCommand
    {
        public int FromUserId { get; private set; }
        public int ChatIdForAnswer { get; private set; }
        public string Name { get; private set; }
        public List<object> Parametres { get; set; }


        private BotCommand()
        {
        }


        /// <summary> Create <see cref="BotCommand"/> from <see cref="IMessage"/> </summary>
        public static async Task<BotCommand> ParseMessageAsync(IMessage message)
        {
            return await Task.Run(() => ParseMessage(message));
        }

        /// <summary> Create <see cref="BotCommand"/> from <see cref="IMessage"/> </summary>
        public static BotCommand ParseMessage(IMessage message)
        {
            try
            {
                if (IsCommand(message.MessageText))
                {
                    // Если есть кавычки, их должно быть чётное количество
                    if (message.MessageText.Count(c => c == '"') % 2 != 0)
                    {
                        var aex = new ArgumentException("Кавычек должно быть чётное количество!");
                        aex.Data.Add("Message", message);
                        throw aex;
                    }

                    // 1. Получаем команду и её параметры из текста сообщения
                    var messageWords = MessageSplitter(message.MessageText);

                    if (messageWords?.Count > 1 && messageWords[0].ToUpper() == "ERROR")
                        throw new Exception(messageWords[1]); // TODO: Сделать нормальный вывод для пользователя

                    string commandName = messageWords[0].ToLower().Replace("_", "\\_");

                    if (commandName.Contains('@'))
                        commandName = commandName.Substring(0, commandName.IndexOf('@'));

                    messageWords.RemoveAt(0);

                    var parameters = new List<object>();
                    messageWords.ForEach(w => parameters.Add(w));

                    BotCommand botCommand = null;

                    // 2. Проверяем наличие команды в БД
                    using (var ctx = new ZsBotDbContext())
                    {
                        //throw new NotImplementedException("Раскомментировать текст ниже после восстановления модели данных");

                        var dbItem = ctx.Commands.FirstOrDefault(c => c.CommandName == commandName);

                        if (dbItem != null)
                        {
                            // Если пользователь не передавал параметры, пробуем получить дефолтный набор
                            if (parameters.Count == 0)
                            {
                                var defaultArgs = dbItem.CommandDefaultArgs?.Split(';')?.ToList();

                                if (defaultArgs?.Count > 0)
                                    defaultArgs.ForEach(a => parameters.Add(a.Trim()));
                            }

                            botCommand = new BotCommand()
                            {
                                Name = "/" + commandName.Trim().ToLower().Replace("/", ""),
                                FromUserId = message.UserId,
                                ChatIdForAnswer = message.ChatId,
                                Parametres = parameters
                            };
                        }

                        return botCommand;
                    }
                }
                else
                    throw new ArgumentException("Сообщение не является командой для бота");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary> Check if message is <see cref="BotCommand"/> </summary>
        public static bool IsCommand(string message)
        {
            var text = message?.Trim();
            return text?[0] == '/' && text?.Length > 1;
        }

        private static List<string> MessageSplitter(string argumentsLine)
        {
            // Если есть кавычки, их должно быть чётное количество
            if (argumentsLine.Count(c => c == '"') % 2 != 0)
                return new List<string>() { "ERROR", "Неверный формат записи аргументов!" };

            // Example: /cmd arg1 "arg 2", arg3;"arg4"

            // Сначала выделяем аргументы в кавычках в отдельную группу <индекс, значение>
            // а на их место вставляем заглушку в формате <<индекс>>
            var quotedArgs = new Dictionary<int, string>();

            int begIndex = -1;
            for (int i = 0; i < argumentsLine.Length; i++)
            {
                // Начало составного аргумента
                if (argumentsLine[i] == '"' && begIndex == -1)
                {
                    begIndex = i;
                }
                // Дошли до конца составного аргумента
                else if (argumentsLine[i] == '"' && begIndex > -1)
                {
                    // Добавляем значение в список 
                    quotedArgs.Add(quotedArgs.Count, argumentsLine.Substring(begIndex + 1, i - begIndex - 1));

                    // Заменяем значение в строке аргументов на индекс
                    argumentsLine = argumentsLine.Remove(begIndex, i - begIndex + 1);
                    argumentsLine = argumentsLine.Insert(begIndex, $" <[<{quotedArgs.Count - 1}>]> ");

                    i = begIndex;
                    begIndex = -1;
                }
            }

            // Обрабатываем строку с аргументами, будто там нет составных значений
            var words = argumentsLine.Replace(',', ' ')
                                     .Replace(';', ' ')
                                     .Trim().Split(' ').ToList();

            words.RemoveAll(w => w.Trim() == "");

            // Убираем лишние символы из простых аргументов
            words.ForEach(w => w = w.Replace(",", "")
                                    .Replace(";", "")
                                    .Replace("-", "")
                                    .Replace("=", "")
                                    .Trim());


            // Заменяем в массиве индексы на их значения
            for (int i = 0; i < words.Count; i++)
            {
                // Получаем временный индекс
                int mapIndex = -1;
                if (words[i].Contains("<[<") && words[i].Contains(">]>"))
                {
                    mapIndex = int.Parse(words[i].Replace("<[<", "").Replace(">]>", ""));

                    // Присваиваем значение, соответствующее этому индексу
                    words[i] = quotedArgs[mapIndex];
                }
            }

            return words;
        }
    }
}
