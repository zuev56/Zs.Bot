using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Zs.App.Home.Data.Models.Vk;
using Zs.App.Home.Services.Vk;
using Zs.Bot.Data.Abstractions;
using Zs.Bot.Data.Models;
using Zs.Bot.Services.Messaging;
using Zs.Common.Abstractions;
using Zs.Common.Enums;
using Zs.Common.Extensions;
using Zs.Common.Services.Abstractions;
using Zs.Common.Services.Logging.Seq;
using Zs.Common.Services.Scheduler;

namespace Zs.App.Home.Bot
{
    // TODO: В хранимке vk.sf_cmd_get_not_active_users выводить точное количество времени отсутствия

    internal class UserWatcher : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly IActivityService _activityService;
        private readonly IMessenger _messenger;
        private readonly IScheduler _scheduler;
        private readonly IRepository<Data.Models.Vk.User, int> _vkUsersRepo;
        private readonly IRepository<ActivityLogItem, int> _vkActivityLogRepo;
        private readonly IItemsWithRawDataRepository<Message, int> _messagesRepo;
        private readonly ISeqService _seqService;
        private readonly ILogger<UserWatcher> _logger;
        //private readonly IConnectionAnalyser _connectionAnalyser;
        private readonly float _version;
        private readonly string _accessToken;
        private readonly int[] _userIds;
        private IJob _userActivityLogger;
        private readonly int _activityLogIntervalSec;
        private bool _isFirstStep = true;


        public UserWatcher(
            IConfiguration configuration, 
            IActivityService activityService,
            IMessenger messenger,
            IScheduler scheduler,
            IRepository<Data.Models.Vk.User, int> vkUsersRepo,
            IRepository<ActivityLogItem, int> vkActivityLogRepo,
            IItemsWithRawDataRepository<Message, int> messagesRepo,
            ISeqService seqService,
            ILogger<UserWatcher> logger)
        {
            try
            {
                _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
                _activityService = activityService ?? throw new ArgumentNullException(nameof(activityService));
                _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
                _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
                _vkUsersRepo = vkUsersRepo ?? throw new ArgumentNullException(nameof(vkUsersRepo));
                _vkActivityLogRepo = vkActivityLogRepo ?? throw new ArgumentNullException(nameof(vkActivityLogRepo));
                _messagesRepo = messagesRepo ?? throw new ArgumentNullException(nameof(messagesRepo));
                _seqService = seqService;
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));

                _activityLogIntervalSec = _configuration.GetSection("Home:Vk:ActivityLogIntervalSec").Get<int>();
                _version = float.Parse(_configuration["Home:Vk:Version"], CultureInfo.InvariantCulture);
                _accessToken = _configuration.GetSecretValue("Home:Vk:AccessToken");
                _userIds = _configuration.GetSection("Home:Vk:UserIds").Get<int[]>();

                CreateJobs();
            }
            catch (Exception ex)
            {
                var tiex = new TypeInitializationException(typeof(UserWatcher).FullName, ex);
               _logger?.LogError(tiex, $"{nameof(UserWatcher)} initialization error");
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _scheduler.Start(3000, 1000);
                await _messenger.AddMessageToOutboxAsync($"Bot started", "ADMIN");
                _logger?.LogInformation($"{nameof(UserWatcher)} started");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"{nameof(UserWatcher)} starting error");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _scheduler.Stop();
            _logger?.LogInformation($"{nameof(UserWatcher)} stopped");
            return Task.CompletedTask;
        }

        private void CreateJobs()
        {
            _userActivityLogger = new ProgramJob(
                TimeSpan.FromSeconds(_activityLogIntervalSec),
                () => SaveVkUsersActivityAsync().Wait(),
                description: "logUserStatus", 
                logger: _logger);
            _scheduler.Jobs.Add(_userActivityLogger);

            var notActiveUsers12hInformer = new SqlJob(
                TimeSpan.FromHours(1),
                QueryResultType.String,
                $"select vk.sf_cmd_get_not_active_users('{string.Join(',', _configuration.GetSection("Home:Vk:TrackedUserIds").Get<int[]>())}', {_configuration.GetSection("Vk:AlarmAfterInactiveHours").Get<int>()})",
                _configuration.GetSecretValue("ConnectionStrings:Default"),
                startDate: DateTime.Now + TimeSpan.FromSeconds(5),
                description: "notActiveUsers12hInformer"
                );
            notActiveUsers12hInformer.ExecutionCompleted += Job_ExecutionCompleted;
            _scheduler.Jobs.Add(notActiveUsers12hInformer);


            if (_seqService != null)
            {
                var dayErrorsAndWarningsInformer = new ProgramJob<string>(
                TimeSpan.FromHours(1),
                () =>
                {
                    var events = _seqService.GetLastEvents(DateTime.Now - TimeSpan.FromHours(1), 10, _configuration.GetSection("Seq:ObservedSignals").Get<int[]>());
                    events.Wait();
                    return events.Result?.Count > 0 ? string.Join(Environment.NewLine + Environment.NewLine, events.Result) : null;
                },
                startDate: Job.NextHour(),
                description: "dayErrorsAndWarningsInformer"
                );
                dayErrorsAndWarningsInformer.ExecutionCompleted += Job_ExecutionCompleted;
                _scheduler.Jobs.Add(dayErrorsAndWarningsInformer);

                var nightErrorsAndWarningsInformer = new ProgramJob<string>(
                    TimeSpan.FromDays(1),
                    () =>
                    {
                        var events = _seqService.GetLastEvents(DateTime.Now - TimeSpan.FromHours(12), 10, _configuration.GetSection("Seq:ObservedSignals").Get<int[]>());
                        events.Wait();
                        return events.Result?.Count > 0 ? string.Join(Environment.NewLine + Environment.NewLine, events.Result) : null;
                    },
                    startDate: DateTime.Today + TimeSpan.FromHours(24 + 10),
                    description: "nightErrorsAndWarningsInformer"
                    );
                nightErrorsAndWarningsInformer.ExecutionCompleted += Job_ExecutionCompleted;
                _scheduler.Jobs.Add(nightErrorsAndWarningsInformer);
            }
        }

        private async void Job_ExecutionCompleted(IJob<string> job, IServiceResult<string> result)
        {
            try
            {
                if (result.IsSuccess && result.Result != null
                    && DateTime.Now.Hour > _configuration.GetSection("Notifier:Time:FromHour").Get<int>()
                    && DateTime.Now.Hour < _configuration.GetSection("Notifier:Time:ToHour").Get<int>())
                {
                    if (job.Description == "notActiveUsers12hInformer")
                    {
                        var todaysAlerts = await _messagesRepo.FindAllAsync(m => m.InsertDate > DateTime.Today && m.Text.Contains("is not active for"));

                        if (!todaysAlerts.Any(m => m.Text.WithoutDigits() == result.Result.WithoutDigits()))
                            await _messenger.AddMessageToOutboxAsync(result.Result, "ADMIN");
                    }
                    else
                    {
                        await _messenger.AddMessageToOutboxAsync(result.Result, "ADMIN");
                    }
                }               
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Job's ExecutionCompleted handler error", result);
            }
        }

        /// <summary> Activity data collection </summary>
        private async Task SaveVkUsersActivityAsync()
        {
            if (_isFirstStep)
            {
                _isFirstStep = false;
                _userActivityLogger.IdleStepsCount = 10;
            }

            var result = await _activityService.SaveVkUsersActivityAsync();
            
            if (_userActivityLogger.IdleStepsCount > 0)
                _userActivityLogger.IdleStepsCount = 0;

            if (!result.IsSuccess)
                _logger?.LogWarning(result.Messages.LastOrDefault()?.Text);
        }
    }
}
