using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Zs.Bot.Data;
using Zs.Bot.Data.Abstractions;
using Zs.Bot.Data.Models;
using Zs.Common.Abstractions;
using Zs.Common.Enums;

namespace Zs.Bot.Services.Logging
{
    public class Logger : IZsLogger
    {
        private static int _logMessageMaxLength = -1;
        private static Logger _instance;
        private string _emergencyLogDirrectory = AppDomain.CurrentDomain.BaseDirectory;
        private readonly object _locker = new object();
        private readonly IRepository<Log, int> _logsRepo;
        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            MaxDepth = 64
        };

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

        public Logger(IRepository<Log, int> logsRepo)
        {
            _logsRepo = logsRepo ?? throw new ArgumentNullException(nameof(logsRepo));
        }

        public async Task LogErrorAsync(Exception e, [CallerMemberName] string initiator = null)
        {
            var jsonData = JsonSerializer.Serialize(e, _jsonSerializerOptions);
            await TrySaveToDatabase(InfoMessageType.Error, e.Message, initiator, jsonData);
        }

        public async Task LogInfoAsync(string message, [CallerMemberName] string initiator = null)
        {
            await TrySaveToDatabase(InfoMessageType.Info, message, initiator);
        }

        public async Task LogInfoAsync<T>(string message, T data, [CallerMemberName] string initiator = null)
        {
            var jsonData = JsonSerializer.Serialize(data, _jsonSerializerOptions);
            await TrySaveToDatabase(InfoMessageType.Info, message, initiator, jsonData);
        }

        public async Task LogWarningAsync(string message = null, [CallerMemberName] string initiator = null)
        {
            await TrySaveToDatabase(InfoMessageType.Warning, message, initiator);
        }

        public async Task LogWarningAsync<T>(string message, T data, [CallerMemberName] string initiator = null)
        {
            var jsonData = JsonSerializer.Serialize(data, _jsonSerializerOptions);
            await TrySaveToDatabase(InfoMessageType.Warning, message, initiator, jsonData);
        }

        private async Task TrySaveToDatabase(InfoMessageType type, string message, string initiator = null, string data = null)
        {
            try
            {
                if (_logMessageMaxLength == -1)
                {
                    var attribute = (StringLengthAttribute)typeof(Log).GetProperty(nameof(Log.Message)).GetCustomAttributes(true)
                        .FirstOrDefault(a => a is StringLengthAttribute);
                    _logMessageMaxLength = attribute?.MaximumLength ?? 100;
                }

                if (message.Length > _logMessageMaxLength)
                    message = message.Substring(0, _logMessageMaxLength - 3) + "...";

                var logItem = new Log
                {
                    Type       = type.ToString(),
                    Message    = message,
                    Initiator  = initiator,
                    Data       = data,
                    InsertDate = DateTime.Now
                };

                if (!await _logsRepo.SaveAsync(logItem))
                {
                    TrySaveInFile(type, message, initiator, data);
                }
            }
            catch (Exception ex)
            {
                TrySaveInFile(type, message, initiator, data);
            }
        }
        
        private void TrySaveInFile(InfoMessageType type, string message, string initiator, string data)
        {
            var formattedType = type switch
            {
                InfoMessageType.Warning => "Warning",
                InfoMessageType.Error   => "ERROR  ",
                _               => "Info   "
            };

            var text = $"{formattedType}  {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}  {initiator}   {message}"
                             + $"{(data is {} ? $"\n{data}" : "")}\n";

            var fileName = $"log_{DateTime.Now.Date:yyyy-MM-dd}.log";

            File.AppendAllText(fileName, text);
        }
    }
}
