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
            ILogger<UserWatcher> logger = null)
        {
            try
            {
                _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
                _activityService = activityService;
                _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
                _scheduler = scheduler;
                _vkUsersRepo = vkUsersRepo ?? throw new ArgumentNullException(nameof(vkUsersRepo));
                _vkActivityLogRepo = vkActivityLogRepo ?? throw new ArgumentNullException(nameof(vkActivityLogRepo));
                _messagesRepo = messagesRepo;
                _logger = logger;

                _activityLogIntervalSec = _configuration.GetSection("Home:Vk:ActivityLogIntervalSec").Get<int>();
                _version = float.Parse(_configuration["Home:Vk:Version"], CultureInfo.InvariantCulture);
                _accessToken = _configuration["Home:Vk:AccessToken"];
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

            var notActiveUsers12hInformer = new SqlJob(
                TimeSpan.FromHours(1),
                QueryResultType.String,
                $"select vk.sf_cmd_get_not_active_users('{string.Join(',', _configuration.GetSection("Home:Vk:TrackedUserIds").Get<int[]>())}', {_configuration.GetSection("Vk:AlarmAfterInactiveHours").Get<int>()})",
                _configuration.GetConnectionString("Default"),
                startDate: DateTime.Now + TimeSpan.FromSeconds(5),
                description: "notActiveUsers12hInformer"
                );
        // Больше нет bot.logs
        //    var dayErrorsAndWarningsInformer = new SqlJob(
        //        TimeSpan.FromHours(1),
        //        QueryResultType.String,
        //         @"select string_agg('**' || log_type || '**  ' || to_char(insert_date, 'HH24:MI:SS') || E'\n' || log_initiator || ':  ' || log_message, E'\n\n' order by insert_date desc)
        //                     from bot.logs
        //                    where log_type in ('Warning', 'Error')
        //                      and insert_date > now() - interval '1 hour'",
        //        _configuration.GetConnectionString("Default"),
        //        startDate: Job.NextHour(),
        //        description: "dayErrorsAndWarningsInformer"
        //        );
        //
        //    var nightErrorsAndWarningsInformer = new SqlJob(
        //        TimeSpan.FromDays(1),
        //        QueryResultType.String,
        //         @" select string_agg('**' || log_type || '**  ' || to_char(insert_date, 'HH24:MI:SS') || E'\n' || log_initiator || ':  ' || log_message, E'\n\n' order by insert_date desc)
        //                      from bot.logs
        //                     where log_type in ('Warning', 'Error')
        //                       and insert_date > now() - interval '12 hours'",
        //        _configuration.GetConnectionString("Default"),
        //        startDate: DateTime.Today + TimeSpan.FromHours(24+10),
        //        description: "nightErrorsAndWarningsInformer"
        //        );

            notActiveUsers12hInformer.ExecutionCompleted += Job_ExecutionCompleted;
        //    dayErrorsAndWarningsInformer.ExecutionCompleted += Job_ExecutionCompleted;
        //    nightErrorsAndWarningsInformer.ExecutionCompleted += Job_ExecutionCompleted;

            _scheduler.Jobs.Add(_userActivityLogger);
        //    _scheduler.Jobs.Add(notActiveUsers12hInformer);
        //    _scheduler.Jobs.Add(dayErrorsAndWarningsInformer);
        //    _scheduler.Jobs.Add(nightErrorsAndWarningsInformer);
        }

        private async void Job_ExecutionCompleted(IJob job, IServiceResult<string> result)
        {
            try
            {
                if (result.IsSuccess && result.Result != null
                    && DateTime.Now.Hour > _configuration.GetSection("Notifier:Time:FromHour").Get<int>() 
                    && DateTime.Now.Hour < _configuration.GetSection("Notifier:Time:ToHour").Get<int>())
                {
                    var todaysAlerts = await _messagesRepo.FindAllAsync(m => m.InsertDate > DateTime.Today && m.Text.Contains("is not active for"));

                    if (!todaysAlerts.Any(m => m.Text.WithoutDigits() == result.Result.WithoutDigits()))
                        await _messenger.AddMessageToOutboxAsync(result.Result, "ADMIN");
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
