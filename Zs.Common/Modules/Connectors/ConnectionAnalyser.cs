﻿using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using Zs.Common.Enums;
using Zs.Common.Interfaces;

namespace Zs.Common.Modules.Connectors
{
    /// <summary>
    /// Объект, анализирующий подключение к сети
    /// </summary>
    public class ConnectionAnalyser
    {
        // TODO: вести учёт 100 последних разрывов связи  

        private readonly object _locker = new object();
        private readonly IZsLogger _logger;      
        private readonly string[] _internetServers;
        private Timer _timer;

        public WebProxy WebProxy { get; private set; }

        public DateTime? InternetRepairDate { get; private set; }

        /// <summary> Событие на изменение состояния соединения с интернетом </summary>
        public event Action<ConnectionStatus> ConnectionStatusChanged;

        /// <summary> Текущий статус соединения </summary>
        public ConnectionStatus CurrentStatus = ConnectionStatus.Undefined;


        public ConnectionAnalyser(params string[] testHosts)
        {
            _internetServers = testHosts?.Length > 0 ? testHosts : throw new ArgumentException($"{nameof(testHosts)} must contains at least 1 element");
        }

        public ConnectionAnalyser(IZsLogger logger = null, params string[] testHosts)
            : this(testHosts)
        {
            _logger = logger;
        }
        
        public void Start(uint dueTime, uint period)
        {
            try
            {
                _timer = new Timer(new TimerCallback(AnalyzeConnection));
                _timer.Change(dueTime, period);

                _logger?.LogInfo($"{nameof(ConnectionAnalyser)} запущен", nameof(ConnectionAnalyser));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex);
            }
        }

        public void Stop()
        {
            try
            {
                _timer.Dispose();
                _logger?.LogInfo($"{nameof(ConnectionAnalyser)} остановлен", nameof(ConnectionAnalyser));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex);
            }
        }

        public void InitializeProxy(string socket, string userName = null, string password = null)
        {
            WebProxy = new WebProxy(socket, true);
            if (!string.IsNullOrWhiteSpace(userName)
                && !string.IsNullOrWhiteSpace(password))
            {
                var credentials = new NetworkCredential(userName, password);
                WebProxy.Credentials = credentials;
                //_logger?.LogInfo("Задействован прокси-сервер", $"{nameof(ConnectionAnalyser)}");
            }
        }

        public static bool PingHost(string hostAddress)
        {
            hostAddress = GetClearHostAddress(hostAddress ?? throw new ArgumentNullException(nameof(hostAddress)));

            using var pinger = new Ping();

            var pingReply = pinger.Send(hostAddress);
            return pingReply.Status == IPStatus.Success;
        }

        public static bool ValidateIPv4(string ipString)
        {
            if (string.IsNullOrWhiteSpace(ipString))
                return false;

            string[] splitValues = ipString.Split('.');
            if (splitValues.Length != 4)
                return false;

            byte tempForParsing;

            return splitValues.All(r => byte.TryParse(r, out tempForParsing));
        }

        private void AnalyzeConnection(object timerState)
        {
            try
            {
                if (_internetServers == null || _internetServers.Length == 0)
                    throw new ArgumentException("Должен быть задан адрес сервера для проверки соединения с интернетом!");

                var analyzeResult = ConnectionStatus.Undefined;

                lock (_locker)
                {
                    if (WebProxy?.Address?.Host is { } && !PingHost(WebProxy.Address.Host))
                        analyzeResult = ConnectionStatus.NoProxyConnection;

                    if (analyzeResult == ConnectionStatus.Undefined)
                        foreach (var server in _internetServers)
                        {
                            if (PingHost(server))
                            {
                                analyzeResult = ConnectionStatus.Ok;
                                break;
                            }
                            else
                                analyzeResult = ConnectionStatus.NoInternetConnection;
                        }
                    
                    if (InternetRepairDate == null && analyzeResult == ConnectionStatus.Ok)
                        InternetRepairDate = DateTime.Now;
                    else if (analyzeResult != ConnectionStatus.Ok)
                        InternetRepairDate = null;
                    
                    
                    if (analyzeResult != CurrentStatus)
                    {
                        CurrentStatus = analyzeResult;
                        ConnectionStatusChanged?.Invoke(CurrentStatus);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex);
            }
        }

        /// <summary> Получение адреса хоста без протокола и порта </summary>
        private static string GetClearHostAddress(string hostAddress)
        {
            if (hostAddress?.Length > 0)
            {
                hostAddress = hostAddress.Contains("://")
                              ? hostAddress.Substring(hostAddress.IndexOf("://") + 3)
                              : hostAddress;

                hostAddress = hostAddress.Contains(":") && !hostAddress.Contains("://")
                              ? hostAddress.Substring(0, hostAddress.IndexOf(":"))
                              : hostAddress;

                hostAddress = hostAddress.Trim('/');
            }

            return hostAddress;
        }

    }
}
