﻿using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Zs.Common.Interfaces;

namespace Zs.Service.ChatAdmin
{
    public class Configuration : IZsConfiguration
    {
        private readonly IConfiguration _configuration;

        public string ConnectionString { get; }
        public string WorkPath         { get; }
        public string BotToken         { get; }
        public string ProxySocket      { get; }
        public string ProxyLogin       { get; }
        public string ProxyPassword    { get; }
        public int    DefaultChatId    { get; }


        public Configuration(
            string configurationPath,
            string dbUser = null,
            string dbPassword = null,
            string botToken = null,
            string proxySocket = null,
            string proxyLogin = null,
            string proxyPassword = null)
        {
            _configuration = new ConfigurationBuilder().AddJsonFile(configurationPath, true, true).Build();

            ConnectionString = PrepareConnectionString(dbUser, dbPassword);
            WorkPath         = Path.GetFullPath(_configuration["WorkPath"]);
            BotToken         = botToken      ?? _configuration["BotToken"];
            ProxySocket      = proxySocket   ?? _configuration["ProxySocket"];
            ProxyLogin       = proxyLogin    ?? _configuration["ProxyLogin"];
            ProxyPassword    = proxyPassword ?? _configuration["ProxyPassword"];
            DefaultChatId    = int.TryParse(_configuration["DefaultChatId"], out int id) ? id : -1;
        }

        private string PrepareConnectionString(string dbUser, string dbPassword)
        {
            var cs = _configuration["ConnectionString"];
            var csArgs = Regex.Matches(cs, "(?<Key>[^=;]+)=(?<Val>[^;]+)")
                              .Where(m => m.Success)
                              .Select(m => m.Value.Trim()).ToList();

            if (!string.IsNullOrWhiteSpace(dbUser))
            {
                int index = csArgs.FindIndex(m => m.StartsWith("UID", StringComparison.OrdinalIgnoreCase));
                if (index >= 0)
                    csArgs[index] = $"Uid={dbUser}";
                else
                    csArgs.Add($"Uid={dbUser}");
            }
            if (!string.IsNullOrWhiteSpace(dbPassword))
            {
                int index = csArgs.FindIndex(m => m.StartsWith("PWD", StringComparison.OrdinalIgnoreCase));
                if (index >= 0) 
                    csArgs[index] = $"Pwd={dbPassword}";
                else
                    csArgs.Add($"Pwd={dbPassword}");
            }

            return string.Join(';', csArgs);
        }
    }
}
