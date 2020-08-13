using System;
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
    // - Подробное логгирование
    // - Проверить устойчивасть к перебоям со связью
    // - Исправить двойную отправку сообщений от джобов в релизной версии
    // - После бана удалять старое предупреждение от бота, чтобы не захламлять чат
    

    internal class ChatAdmin : IHostedService
    {
        private readonly IZsConfiguration _configuration;
        private readonly IZsLogger _logger;
        private readonly ZsBot _bot;
        private readonly CycleWorker _cycleWorker;
        private readonly MessageProcessor _messageProcessor;
        private readonly ConnectionAnalyser _connectionAnalyser;
        private readonly bool _detailedLogging;


        public ChatAdmin(
            IZsConfiguration configuration,
            IMessenger messenger, 
            ConnectionAnalyser connectionAnalyser)
        {
            try
            {
                _logger = Logger.GetInstance();
                _configuration = configuration;

                if (_configuration.Contains("DetailedLogging"))
                    bool.TryParse(_configuration["DetailedLogging"].ToString(), out _detailedLogging);

                _bot = new ZsBot(_configuration, messenger);
                _bot.Messenger.MessageReceived += Messenger_MessageReceived;

                _connectionAnalyser = connectionAnalyser;
                _connectionAnalyser.ConnectionStatusChanged += СonnectionAnalyser_StatusChanged;

                _messageProcessor = new MessageProcessor(_configuration, messenger);
                _messageProcessor.LimitsDefined += MessageProcessor_LimitsDefined;

                _cycleWorker = new CycleWorker(_logger, _detailedLogging);
                CreateJobs();

#warning Переделать инициализацию из Program.cs
                var optionsBuilder = new DbContextOptionsBuilder<ChatAdminDbContext>();
                optionsBuilder.UseNpgsql(_configuration["ConnectionString"].ToString());
                ChatAdminDbContext.Initialize(optionsBuilder.Options);
            }
            catch (Exception ex)
            {
                var tiex = new TypeInitializationException(typeof(ChatAdmin).FullName, ex);
                _logger.LogError(tiex, nameof(ChatAdmin));
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _connectionAnalyser.Start(5000, 30000);
            _cycleWorker.Start(3000, 1000);
            _bot.Messenger.AddMessageToOutbox($"Bot started", "ADMIN");
            _logger.LogInfo("Bot started", nameof(ChatAdmin));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _bot.Messenger.AddMessageToOutbox($"Bot started", "ADMIN");
            Task.Delay(2000);
            _connectionAnalyser.Stop();
            _cycleWorker.Stop();
            _logger.LogInfo("Bot stopped", nameof(ChatAdmin));
            return Task.CompletedTask;
        }

        private void MessageProcessor_LimitsDefined(string messageText)
        {
            if (DateTime.Now.Hour > 9)
                _bot.Messenger.AddMessageToOutbox(messageText, "ADMIN");
        }

        private void СonnectionAnalyser_StatusChanged(ConnectionStatus status)
        {
            if (status == ConnectionStatus.Ok)
                _messageProcessor?.SetInternetRepairDate(DateTime.Now);
            else
            {
                _messageProcessor?.SetInternetRepairDate(null);

                var logMessage = status == ConnectionStatus.NoProxyConnection
                    ? "No proxy connection"
                    : "No internet connection";

                _logger.LogWarning(logMessage, nameof(ConnectionAnalyser));
            }
        }

        private void Messenger_MessageReceived(MessageActionEventArgs e)
        {
            _messageProcessor.ProcessGroupMessage(e.Message);
        }

        private void Job_ExecutionCompleted(Job job, IJobExecutionResult result)
        {
            _logger.LogInfo(
                $"Job execution completed{(job?.Description != null ? $" [{job.Description}]" : "")}", 
                result?.Show() ?? "<null>", 
                nameof(ChatAdmin));

            if (result != null && DateTime.Now.Hour > 9 && DateTime.Now.Hour < 23)
                _bot.Messenger.AddMessageToOutbox(result?.Show(), "ADMIN");
        }

        /// <summary> Creating a <see cref="Job"/> list for a <see cref="CycleWorker"/> instance </summary>
        private void CreateJobs()
        {
            // Задачи на начало дня
            // 2. Сбрасываем дату учёта сообщений
            // 3. Задаём значения для ограничений из БД/конфигурации (важно, когда заданные админом значения были сдвинуты ботом, чтобы не перетереть данные)
            // Проверка наличия интернета 
            // Оповещение о сбоях Каждый час
            // После 23:55 высылаем статистику
            // Оповещение о событиях календаря

            var sendYesterdaysStatistics = new SqlJob(
                TimeSpan.FromDays(1),
                QueryResultType.String,
                $"select zl.sf_cmd_get_full_statistics(10, '{DateTime.Today - TimeSpan.FromDays(1)}', '{DateTime.Today - TimeSpan.FromSeconds(1)}')",
                _configuration["ConnectionString"].ToString(),
                startDate: DateTime.Now.Date + TimeSpan.FromHours(24+10)
                ) { Description = "sendYesterdaysStatistics" };

            var resetLimits = new ProgramJob(
                TimeSpan.FromDays(1),
                _messageProcessor.ResetLimits,
                startDate: DateTime.Now.Date + TimeSpan.FromDays(1)
                ) { Description = "resetLimits" };

            var sendErrorsAndWarnings = new SqlJob(
                TimeSpan.FromHours(1),
                QueryResultType.String,
                 @" select string_agg('**' || log_type || '**  ' || to_char(insert_date, 'HH24:MI:SS') || E'\n' || log_initiator || ':  ' || log_message, E'\n\n' order by insert_date desc)"
                + "\n from bot.logs"
                + "\nwhere log_type in ('Warning', 'Error')"
                + "\n  and insert_date > now() - interval '1 hour'",
                _configuration["ConnectionString"].ToString(),
                startDate: Job.NextHour()
                ) { Description = "sendErrorsAndWarnings" };

            //var testException = new ProgramJob(
            //    TimeSpan.FromSeconds(25),
            //    () => throw new Exception("Test exception"));
            //var testNOException = new ProgramJob(
            //    TimeSpan.FromSeconds(25),
            //    () => { });
            //var testSqlJob = new SqlJob(
            //    TimeSpan.FromSeconds(30),
            //    QueryResultType.Double,
            //    "select count(*) from bot.messages",
            //    _configuration["ConnectionString"].ToString(),
            //    DateTime.Now + TimeSpan.FromSeconds(25)
            //    ) {Description = "testSqlJob"};
            //testSqlJob.ExecutionCompleted += Job_ExecutionCompleted;

            sendYesterdaysStatistics.ExecutionCompleted += Job_ExecutionCompleted;
            sendErrorsAndWarnings.ExecutionCompleted += Job_ExecutionCompleted;

            _cycleWorker.Jobs.Add(sendYesterdaysStatistics);
            _cycleWorker.Jobs.Add(resetLimits);
            _cycleWorker.Jobs.Add(sendErrorsAndWarnings);
            //_cycleWorker.Jobs.Add(testSqlJob);
            //_cycleWorker.Jobs.Add(testException);
            //_cycleWorker.Jobs.Add(testNOException);
        }
    }
}
