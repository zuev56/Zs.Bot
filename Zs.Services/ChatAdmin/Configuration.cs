using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Zs.Common.Interfaces;

namespace Zs.Service.ChatAdmin
{
    /// <summary>
    /// Configuration json file example:
    /// {
    ///     "BotToken": "vZgtmWSxB7oHgRCL1zqZFP8Wb5OE",
    ///     "ConnectionString": "Host=localhost;Port=1;Database=Db;Username=usr;Password=pwd;",	
    ///     "WorkPath": "C:\\",
    ///     "DefaultChatId": "2"
    ///     "ProxySocket": "123.123.123.123:45678",
    ///     "ProxyUser": "pUsr",
    ///     "ProxyPassword": "pPwd"
    /// }
    /// </summary>
    public class Configuration : IZsConfiguration
    {
        private readonly Dictionary<string, object> _configDictionary = new Dictionary<string, object>();
        
        public object this[string key]
        {
            get => _configDictionary.ContainsKey(key) ? _configDictionary[key] : null;
            set
            {
                if (_configDictionary.ContainsKey(key))
                    _configDictionary[key] = value;
                else
                    throw new ArgumentOutOfRangeException(nameof(key));
            }
        }

        public Configuration(string configurationPath)
        {
            var configText = File.ReadAllText(configurationPath);
            _configDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(configText);
        }

        public bool Contains(string key)
        {
            return _configDictionary.ContainsKey(key ?? throw new ArgumentNullException(nameof(key)));
        }
    }
}
