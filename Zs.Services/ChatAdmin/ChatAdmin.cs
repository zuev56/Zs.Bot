using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Zs.Bot;
using Zs.Bot.Helpers;
using Zs.Bot.Modules.Messaging;
using Zs.Common.Enums;
using Zs.Common.Interfaces;
using Zs.Common.Modules.Connectors;
using Zs.Common.Modules.CycleWorker;
using Zs.Service.ChatAdmin.DbModel;

namespace Zs.Service.ChatAdmin
{
    internal class ChatAdmin : IHostedService
    {
        private readonly IZsConfiguration _configuration;
        private readonly IZsLogger _logger;
        private readonly ZsBot _bot;
        private readonly CycleWorker _cycleWorker;
        private readonly MessageProcessor _messageProcessor;
        private readonly ConnectionAnalyser _connectionAnalyser;


        public ChatAdmin(
            IZsConfiguration configuration,
            IMessenger messenger, 
            ConnectionAnalyser connectionAnalyser)
        {
            try
            {
                _logger = Logger.GetInstance();
                _configuration = configuration;

                _bot = new ZsBot(_configuration, messenger);

                _connectionAnalyser = connectionAnalyser;
                _connectionAnalyser.ConnectionStatusChanged += СonnectionAnalyser_StatusChanged;

                if (configuration is Configuration c)
                    _messageProcessor = new MessageProcessor(c.DefaultChatId);

                _cycleWorker = new CycleWorker(_logger);
                _cycleWorker.Jobs.AddRange(GetJobs());

                var optionsBuilder = new DbContextOptionsBuilder<ChatAdminDbContext>();
                optionsBuilder.UseNpgsql(_configuration.ConnectionString);
                ChatAdminDbContext.Initialize(optionsBuilder.Options);
            }
            catch (Exception e)
            {
                var te = new TypeInitializationException(typeof(ChatAdmin).FullName, e);
                _logger.LogError(te, nameof(ChatAdmin));
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _connectionAnalyser.Start(5000, 30000);
            _cycleWorker.Start(3000, 5000);
            _bot.Messenger.AddMessageToOutbox($"Бот запущен", "ADMIN");
            _logger.LogInfo("Бот запущен", nameof(ChatAdmin));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _connectionAnalyser.Stop();
            _cycleWorker.Stop();
            _logger.LogInfo("Бот остановлен", nameof(ChatAdmin));
            return Task.CompletedTask;
        }

        private void СonnectionAnalyser_StatusChanged(ConnectionStatus status)
        {
            if (status == ConnectionStatus.Ok)
                _messageProcessor?.SetInternetRepairDate(DateTime.Now);
            else
                _messageProcessor?.SetInternetRepairDate(null);
        }

        private IEnumerable<Job> GetJobs()
        {
            //        // Задачи на начало дня
            //            // 2. Сбрасываем дату учёта сообщений
            //            // 3. Задаём значения для ограничений из БД (важно, когда заданные админом значения были сдвинуты, чтобы не перетереть данные)
            //            // 4. Сбрасываем флаг отправки статистики за день
            //        // Проверка наличия интернета 
            //        //     Переопределение лимитов, если интернет был восстановлен после обрыва
            //        //     Запись в ЛОГ, если имеютя проблемы с доступом к серверу Telegram
            //        // Оповещение о сбоях Каждый час
            //        // После 23:55 высылаем статистику
            //                // Формирование сообщения
            //                // Рассылка админам
            //        // Оповещение о событиях календаря
            var job = new SqlJob(
                TimeSpan.FromMinutes(1),
                "SELECT Count(*) FROM bot.logs",
                _configuration.ConnectionString);

            yield return job;
        }
    }
}
