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
        public string ConnectionString { get; }
        public string WorkPath         { get; }
        public string BotToken         { get; }
        public string ProxySocket      { get; }
        public string ProxyLogin       { get; }
        public string ProxyPassword    { get; }
        public int    DefaultChatId    { get; }


        public Configuration(string configurationPath)
        {
            var configText = File.ReadAllText(configurationPath);
            var configDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(configText);

            ConnectionString = configDictionary["ConnectionString"];
            DefaultChatId    = int.TryParse(configDictionary["DefaultChatId"], out int id) ? id : -1;
            WorkPath         = Path.GetFullPath(configDictionary["WorkPath"]);
            BotToken         = configDictionary["BotToken"];
            ProxySocket      = configDictionary.ContainsKey("ProxySocket") ? configDictionary["ProxySocket"] : null;
            ProxyLogin       = configDictionary.ContainsKey("ProxyLogin") ? configDictionary["ProxyLogin"] : null;
            ProxyPassword    = configDictionary.ContainsKey("ProxyPassword") ? configDictionary["ProxyPassword"] : null;
        }
    }
}
