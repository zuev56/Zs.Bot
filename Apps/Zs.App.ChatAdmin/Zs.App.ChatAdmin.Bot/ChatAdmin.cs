using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Zs.Bot;
using Zs.Bot.Model.Data;
using Zs.Bot.Services.Messaging;
using Zs.Common.Abstractions;
using Zs.Common.Enums;
using Zs.Common.Modules.Connectors;
using Zs.Common.Modules.Scheduler;
using Zs.App.ChatAdmin.Abstractions;

namespace Zs.App.ChatAdmin
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
        private readonly IZsLogger _logger;
        private readonly IZsBot _bot;
        private readonly IScheduler _scheduler;
        private readonly IMessageProcessor _messageProcessor;
        private readonly IConnectionAnalyser _connectionAnalyser;
        private readonly IContextFactory _contextFactory;
        private readonly bool _detailedLogging;


        public ChatAdmin(
            IConfiguration configuration,
            IConnectionAnalyser connectionAnalyser,
            IZsBot zsBot,
            IContextFactory contextFactory,
            IMessageProcessor messageProcessor,
            IScheduler scheduler,
            IZsLogger logger)
        {
            try
            {
                _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
                _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));

                _bot = zsBot ?? throw new ArgumentNullException(nameof(zsBot));
                _bot.Messenger.MessageReceived += Messenger_MessageReceived;

                _connectionAnalyser = connectionAnalyser ?? throw new ArgumentNullException(nameof(connectionAnalyser));
                _connectionAnalyser.ConnectionStatusChanged += СonnectionAnalyser_StatusChanged;

                _messageProcessor = messageProcessor ?? throw new ArgumentNullException(nameof(messageProcessor));
                _messageProcessor.LimitsDefined += MessageProcessor_LimitsDefined;

                _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
                CreateJobs();

                if (bool.TryParse(_configuration?["DetailedLogging"], out bool detailedLogging))
                {
                    _detailedLogging = detailedLogging;
                }
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
            _scheduler.Start(3000, 1000);
            _bot.Messenger.AddMessageToOutbox($"Bot started", "ADMIN");
            _logger.LogInfo($"{nameof(ChatAdmin)} started", nameof(ChatAdmin));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _connectionAnalyser.Stop();
            _scheduler.Stop();
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

        private async void Messenger_MessageReceived(MessageActionEventArgs e)
        {
            await _messageProcessor.ProcessGroupMessage(e.Message);
        }

        private void Job_ExecutionCompleted(IJob job, IJobExecutionResult result)
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

        /// <summary> Creating a <see cref="Job"/> list for a <see cref="Scheduler"/> instance </summary>
        private void CreateJobs()
        {
            var sendYesterdaysStatistics = new SqlJob(
                TimeSpan.FromDays(1),
                QueryResultType.String,
                $"select zl.sf_cmd_get_full_statistics(10, now()::date - interval '1 day', now()::date - interval '1 millisecond')",
                _configuration.GetConnectionString("Default"),
                startDate: DateTime.Now.Date + TimeSpan.FromHours(24+10),
                description: "sendYesterdaysStatistics"
            );

            var resetLimits = new ProgramJob(
                TimeSpan.FromDays(1),
                _messageProcessor.ResetLimits,
                startDate: DateTime.Now.Date + TimeSpan.FromDays(1),
                description: "resetLimits"
            );

            var sendDayErrorsAndWarnings = new SqlJob(
                TimeSpan.FromHours(1),
                QueryResultType.String,
                 @" select string_agg('**' || log_type || '**  ' || to_char(insert_date, 'HH24:MI:SS') || E'\n' || log_initiator || ':  ' || log_message, E'\n\n' order by insert_date desc)"
                + "\n from bot.logs"
                + "\nwhere log_type in ('Warning', 'Error')"
                + "\n  and insert_date > now() - interval '1 hour'",
                _configuration.GetConnectionString("Default"),
                startDate: Job.NextHour(),
                description: "sendDayErrorsAndWarnings"
            );

            var sendNightErrorsAndWarnings = new SqlJob(
                TimeSpan.FromDays(1),
                QueryResultType.String,
                 @" select string_agg('**' || log_type || '**  ' || to_char(insert_date, 'HH24:MI:SS') || E'\n' || log_initiator || ':  ' || log_message, E'\n\n' order by insert_date desc)"
                + "\n from bot.logs"
                + "\nwhere log_type in ('Warning', 'Error')"
                + "\n  and insert_date > now() - interval '12 hours'",
                _configuration.GetConnectionString("Default"),
                startDate: DateTime.Today + TimeSpan.FromHours(24+10),
                description: "sendNightErrorsAndWarnings"
            );

            sendYesterdaysStatistics.ExecutionCompleted += Job_ExecutionCompleted;
            sendDayErrorsAndWarnings.ExecutionCompleted += Job_ExecutionCompleted;
            sendNightErrorsAndWarnings.ExecutionCompleted += Job_ExecutionCompleted;

            _scheduler.Jobs.Add(sendYesterdaysStatistics);
            _scheduler.Jobs.Add(resetLimits);
            _scheduler.Jobs.Add(sendDayErrorsAndWarnings);
            _scheduler.Jobs.Add(sendNightErrorsAndWarnings);
        }
    }
}
