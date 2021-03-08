using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Zs.App.ChatAdmin.Abstractions;
using Zs.Bot.Services.Messaging;
using Zs.Common.Abstractions;
using Zs.Common.Enums;
using Zs.Common.Extensions;
using Zs.Common.Services.Abstractions;
using Zs.Common.Services.Connection;
using Zs.Common.Services.Scheduler;

namespace Zs.App.ChatAdmin
{
    internal class ChatAdmin : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChatAdmin> _logger;
        private readonly IMessenger _messenger;
        private readonly IScheduler _scheduler;
        private readonly IMessageProcessor _messageProcessor;
        private readonly IConnectionAnalyser _connectionAnalyser;


        // TODO: После бана удалять старое предупреждение от бота, чтобы не захламлять чат

        public ChatAdmin(
            IConfiguration configuration,
            IConnectionAnalyser connectionAnalyser,
            IMessenger messenger,
            IMessageProcessor messageProcessor,
            IScheduler scheduler,
            ILogger<ChatAdmin> logger)
        {
            try
            {
                _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));

                _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
                _messenger.MessageReceived += Messenger_MessageReceived;

                _connectionAnalyser = connectionAnalyser ?? throw new ArgumentNullException(nameof(connectionAnalyser));
                _connectionAnalyser.ConnectionStatusChanged += СonnectionAnalyser_StatusChanged;

                _messageProcessor = messageProcessor ?? throw new ArgumentNullException(nameof(messageProcessor));
                _messageProcessor.LimitsDefined += MessageProcessor_LimitsDefined;

                _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
                CreateJobs();
            }
            catch (Exception ex)
            {
                var tiex = new TypeInitializationException(typeof(ChatAdmin).FullName, ex);
                _logger.LogError(tiex, $"{nameof(ChatAdmin)} initialization error");
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _connectionAnalyser.Start(5000, 30000);
            _scheduler.Start(3000, 1000);
            await _messenger.AddMessageToOutboxAsync($"Bot started", "ADMIN");
            _logger.LogInformation($"{nameof(ChatAdmin)} started");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _connectionAnalyser.Stop();
            _scheduler.Stop();
            _logger.LogInformation($"{nameof(ChatAdmin)} stopped");

            return Task.CompletedTask;
        }

        private async void MessageProcessor_LimitsDefined(string messageText)
        {
            if (DateTime.Now.Hour > 9)
                await _messenger.AddMessageToOutboxAsync(messageText, "ADMIN");
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

            _logger.LogWarning(logMessage);
        }

        private async void Messenger_MessageReceived(object sender, MessageActionEventArgs e)
        {
            await _messageProcessor.ProcessGroupMessage(e.Message);
        }

        private async void Job_ExecutionCompleted(IJob job, IServiceResult<string> result)
        {
            if (result.IsSuccess && result.Result != null
                    && DateTime.Now.Hour > _configuration.GetSection("Notifier:Time:FromHour").Get<int>()
                    && DateTime.Now.Hour < _configuration.GetSection("Notifier:Time:ToHour").Get<int>())
            {
                await _messenger.AddMessageToOutboxAsync(result.Result, "ADMIN");
            }
        }

        /// <summary> Creating a <see cref="Job"/> list for a <see cref="Scheduler"/> instance </summary>
        private void CreateJobs()
        {
            var sendYesterdaysStatistics = new SqlJob(
                TimeSpan.FromDays(1),
                QueryResultType.String,
                $"select zl.sf_cmd_get_full_statistics(10, now()::date - interval '1 day', now()::date - interval '1 millisecond')",
                _configuration.GetSecretValue("ConnectionStrings:Default"),
                startDate: DateTime.Now.Date + TimeSpan.FromHours(24+10),
                description: "sendYesterdaysStatistics"
            );

            var resetLimits = new ProgramJob(
                TimeSpan.FromDays(1),
                _messageProcessor.ResetLimits,
                startDate: DateTime.Now.Date + TimeSpan.FromDays(1),
                description: "resetLimits"
            );

        // Больше нет bot.logs
        //    var sendDayErrorsAndWarnings = new SqlJob(
        //        TimeSpan.FromHours(1),
        //        QueryResultType.String,
        //         @" select string_agg('**' || log_type || '**  ' || to_char(insert_date, 'HH24:MI:SS') || E'\n' || log_initiator || ':  ' || log_message, E'\n\n' order by insert_date desc)"
        //        + "\n from bot.logs"
        //        + "\nwhere log_type in ('Warning', 'Error')"
        //        + "\n  and insert_date > now() - interval '1 hour'",
        //        _configuration.GetSecretValue("ConnectionStrings:Default"),
        //        startDate: Job.NextHour(),
        //        description: "sendDayErrorsAndWarnings"
        //    );
        //
        //    var sendNightErrorsAndWarnings = new SqlJob(
        //        TimeSpan.FromDays(1),
        //        QueryResultType.String,
        //         @" select string_agg('**' || log_type || '**  ' || to_char(insert_date, 'HH24:MI:SS') || E'\n' || log_initiator || ':  ' || log_message, E'\n\n' order by insert_date desc)"
        //        + "\n from bot.logs"
        //        + "\nwhere log_type in ('Warning', 'Error')"
        //        + "\n  and insert_date > now() - interval '12 hours'",
        //        _configuration.GetSecretValue("ConnectionStrings:Default"),
        //        startDate: DateTime.Today + TimeSpan.FromHours(24+10),
        //        description: "sendNightErrorsAndWarnings"
        //    );

            sendYesterdaysStatistics.ExecutionCompleted += Job_ExecutionCompleted;
        //    sendDayErrorsAndWarnings.ExecutionCompleted += Job_ExecutionCompleted;
        //    sendNightErrorsAndWarnings.ExecutionCompleted += Job_ExecutionCompleted;

            _scheduler.Jobs.Add(sendYesterdaysStatistics);
            _scheduler.Jobs.Add(resetLimits);
        //    _scheduler.Jobs.Add(sendDayErrorsAndWarnings);
        //    _scheduler.Jobs.Add(sendNightErrorsAndWarnings);
        }
    }
}
