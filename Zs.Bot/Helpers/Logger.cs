﻿using System;
using System.IO;
using Newtonsoft.Json;
using Zs.Bot.Model.Db;
using Zs.Common.Enums;
using Zs.Common.Interfaces;

namespace Zs.Bot.Helpers
{
    public class Logger : IZsLogger
    {
        private static Logger _instance;
        private string _emergencyLogDirrectory = AppDomain.CurrentDomain.BaseDirectory;

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


        protected Logger()
        {
        }

        public static Logger GetInstance()
        {
            if (_instance == null)
                _instance = new Logger();

            return _instance;
        }

        public void LogError(Exception e, string logGroup = null)
        {
            var jsonData = JsonConvert.SerializeObject(e, Formatting.Indented);
            DbLog.SaveToDb(LogType.Error, e.Message, logGroup, jsonData);
        }

        public void LogInfo(string message, string logGroup = null)
        {
            DbLog.SaveToDb(LogType.Info, message, logGroup);
        }

        public void LogInfo<T>(string message, T logData, string logGroup = null)
        {
            var jsonData = JsonConvert.SerializeObject(logData, Formatting.Indented);
            DbLog.SaveToDb(LogType.Info, message, logGroup, jsonData);
        }

        public void LogWarning(string message = null, string logGroup = null)
        {
            DbLog.SaveToDb(LogType.Warning, message, logGroup);
        }

    }
}
