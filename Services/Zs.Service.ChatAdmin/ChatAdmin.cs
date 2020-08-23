using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Zs.Bot;
using Zs.Bot.Helpers;
using Zs.Bot.Model.Db;
using Zs.Bot.Modules.Messaging;
using Zs.Common.Abstractions;
using Zs.Common.Enums;
using Zs.Common.Modules.Connectors;
using Zs.Common.Modules.CycleWorker;
using Zs.Service.ChatAdmin.Model;

namespace Zs.Service.ChatAdmin
{
    // + Подробное логгирование
    // + Проверить устойчивасть к перебоям со связью
    // + Исправить двойную отправку сообщений от джобов в релизной версии
    // - После бана удалять старое предупреждение от бота, чтобы не захламлять чат
    // + Проверить пересылку больших сообщений от Бота
    // + Проверить, будут ли удаляться сообщения забаненного пользователя после восстановления соединения с интернетом
    // + Сделать джоб, который утром присылает все ошибки за предыдущую ночь
    // + Если к моменту восстановления интернета бан уже закончился, пользователь может отправить больше сообщений, чем MessageLimitAfterBan, то есть он может заново пройти через все лимиты

    internal class ChatAdmin : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly IZsLogger _logger = Logger.GetInstance();
        private readonly ZsBot _bot;
        private readonly CycleWorker _cycleWorker;
        private readonly MessageProcessor _messageProcessor;
        private readonly IConnectionAnalyser _connectionAnalyser;
        private readonly bool _detailedLogging;


        public ChatAdmin(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IMessenger messenger, 
            IConnectionAnalyser connectionAnalyser)
        {
            try
            {
                _configuration = configuration;

                ZsBotDbContext.Initialize(serviceProvider.GetService<DbContextOptions<ZsBotDbContext>>());
                ChatAdminDbContext.Initialize(serviceProvider.GetService<DbContextOptions<ChatAdminDbContext>>());

                bool.TryParse(_configuration["DetailedLogging"]?.ToString(), out _detailedLogging);

                _bot = new ZsBot(_configuration, messenger);
                _bot.Messenger.MessageReceived += Messenger_MessageReceived;

                _connectionAnalyser = connectionAnalyser;
                _connectionAnalyser.ConnectionStatusChanged += СonnectionAnalyser_StatusChanged;

                _messageProcessor = new MessageProcessor(_configuration, messenger);
                _messageProcessor.LimitsDefined += MessageProcessor_LimitsDefined;

                _cycleWorker = new CycleWorker(_logger, _detailedLogging);
                CreateJobs();
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
            _logger.LogInfo($"{nameof(ChatAdmin)} started", nameof(ChatAdmin));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _connectionAnalyser.Stop();
            _cycleWorker.Stop();
            _logger.LogInfo($"{nameof(ChatAdmin)} stopped", nameof(ChatAdmin));

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

            _messageProcessor?.SetInternetRepairDate(null);

            var logMessage = status switch
            {
                ConnectionStatus.NoProxyConnection => "No proxy connection",
                ConnectionStatus.NoInternetConnection => "No internet connection",
                ConnectionStatus.Ok => "Connection restored",
                _ => "Connection status is undefined"
            };

            _logger.LogWarning(logMessage, nameof(ConnectionAnalyser));
        }

        private void Messenger_MessageReceived(MessageActionEventArgs e)
        {
            _messageProcessor.ProcessGroupMessage(e.Message);
        }

        private void Job_ExecutionCompleted(Job job, IJobExecutionResult result)
        {
            if (_detailedLogging || result?.TextValue != null)
            {
                _logger.LogInfo(
                    $"Job execution completed{(job?.Description != null ? $" [{job.Description}]" : "")}",
                    result?.TextValue ?? "<null>",
                    nameof(ChatAdmin));
            }

            if (result != null && DateTime.Now.Hour > 9 && DateTime.Now.Hour < 23)
                _bot.Messenger.AddMessageToOutbox(result?.TextValue, "ADMIN");
        }

        /// <summary> Creating a <see cref="Job"/> list for a <see cref="CycleWorker"/> instance </summary>
        private void CreateJobs()
        {
            var sendYesterdaysStatistics = new SqlJob(
                TimeSpan.FromDays(1),
                QueryResultType.String,
                $"select zl.sf_cmd_get_full_statistics(10, now()::date - interval '1 day', now()::date - interval '1 millisecond')",
                _configuration["ConnectionString"].ToString(),
                startDate: DateTime.Now.Date + TimeSpan.FromHours(24+10)
                ) { Description = "sendYesterdaysStatistics" };

            var resetLimits = new ProgramJob(
                TimeSpan.FromDays(1),
                _messageProcessor.ResetLimits,
                startDate: DateTime.Now.Date + TimeSpan.FromDays(1)
                ) { Description = "resetLimits" };

            var sendDayErrorsAndWarnings = new SqlJob(
                TimeSpan.FromHours(1),
                QueryResultType.String,
                 @" select string_agg('**' || log_type || '**  ' || to_char(insert_date, 'HH24:MI:SS') || E'\n' || log_initiator || ':  ' || log_message, E'\n\n' order by insert_date desc)"
                + "\n from bot.logs"
                + "\nwhere log_type in ('Warning', 'Error')"
                + "\n  and insert_date > now() - interval '1 hour'",
                _configuration["ConnectionString"].ToString(),
                startDate: Job.NextHour()
                ) { Description = "sendDayErrorsAndWarnings" };

            var sendNightErrorsAndWarnings = new SqlJob(
                TimeSpan.FromDays(1),
                QueryResultType.String,
                 @" select string_agg('**' || log_type || '**  ' || to_char(insert_date, 'HH24:MI:SS') || E'\n' || log_initiator || ':  ' || log_message, E'\n\n' order by insert_date desc)"
                + "\n from bot.logs"
                + "\nwhere log_type in ('Warning', 'Error')"
                + "\n  and insert_date > now() - interval '12 hours'",
                _configuration["ConnectionString"].ToString(),
                startDate: DateTime.Today + TimeSpan.FromHours(24+10)
                ) { Description = "sendNightErrorsAndWarnings" };

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
            sendDayErrorsAndWarnings.ExecutionCompleted += Job_ExecutionCompleted;
            sendNightErrorsAndWarnings.ExecutionCompleted += Job_ExecutionCompleted;

            _cycleWorker.Jobs.Add(sendYesterdaysStatistics);
            _cycleWorker.Jobs.Add(resetLimits);
            _cycleWorker.Jobs.Add(sendDayErrorsAndWarnings);
            _cycleWorker.Jobs.Add(sendNightErrorsAndWarnings);
            //_cycleWorker.Jobs.Add(testSqlJob);
            //_cycleWorker.Jobs.Add(testException);
            //_cycleWorker.Jobs.Add(testNOException);
        }
    }
}
