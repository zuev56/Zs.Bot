using System;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Zs.Bot.Model.Db;
using Zs.Common.Abstractions;
using Zs.Common.Enums;

namespace Zs.Bot.Modules.Logging
{
    public class Logger : IZsLogger
    {
        private static Logger _instance;
        private string _emergencyLogDirrectory = AppDomain.CurrentDomain.BaseDirectory;
        private readonly object _locker = new object();
        private readonly IContextFactory<ZsBotDbContext> _contextFactory;

        public string EmergencyLogDirrectory
        {
            get => _emergencyLogDirrectory;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException($"{nameof(EmergencyLogDirrectory)} не может быть пустым!");

                if (!Directory.Exists(value))
                {
                    var p = Directory.CreateDirectory(value);
                    if (!p.Exists)
                        throw new Exception($"Не удалось создать каталог для хранения логов ({value})");
                }
                _emergencyLogDirrectory = value;
            }
        }

        public Logger(IContextFactory<ZsBotDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void LogError(Exception e, [CallerMemberName] string initiator = null)
        {
            var jsonData = JsonConvert.SerializeObject(e, Formatting.Indented);
            TrySave(LogType.Error, e.Message, initiator, jsonData);
        }

        public void LogInfo(string message, [CallerMemberName] string initiator = null)
        {
            TrySave(LogType.Info, message, initiator);
        }

        public void LogInfo<T>(string message, T data, [CallerMemberName] string initiator = null)
        {
            var jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
            TrySave(LogType.Info, message, initiator, jsonData);
        }

        public void LogWarning(string message = null, [CallerMemberName] string initiator = null)
        {
            TrySave(LogType.Warning, message, initiator);
        }

        public void LogWarning<T>(string message, T data, [CallerMemberName] string initiator = null)
        {
            var jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
            TrySave(LogType.Warning, message, initiator, jsonData);
        }

        private void TrySave(LogType type, string message, string initiator = null, string data = null)
        {
            try
            {
                var isSaved = DbLogExtensions.SaveToDb(type, message, _contextFactory.GetContext(), initiator, data);

                if (!isSaved)
                {
                    var formattedType = type switch
                    {
                        LogType.Warning => "Warning",
                        LogType.Error   => "ERROR  ",
                        _               => "Info   "
                    };

                    var text = $"{formattedType}  {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}  {initiator}   {message}"
                             + $"{(data is {} ? $"\n{data}" : "")}\n";

                    var fileName = $"log_{DateTime.Now.Date:yyyy-MM-dd}.log";

                    lock (_locker)
                    {
                        File.AppendAllText(fileName, text);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
