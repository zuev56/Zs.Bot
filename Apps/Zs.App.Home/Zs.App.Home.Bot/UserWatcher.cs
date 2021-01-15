using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Zs.App.Home.Model;
using Zs.App.Home.Model.VkAPI;
using Zs.Bot.Data.Abstractions;
using Zs.Bot.Data.Models;
using Zs.Bot.Services.Messaging;
using Zs.Common.Abstractions;
using Zs.Common.Enums;
using Zs.Common.Services.Scheduler;
using Zs.Common.Services.WebAPI;

namespace Zs.App.Home.Bot
{
    // TODO: В хранимке vk.sf_cmd_get_not_active_users выводить точное количество времени отсутствия

    internal class UserWatcher : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly IMessenger _messenger;
        private readonly IScheduler _scheduler;
        private readonly IRepository<VkUser, int> _vkUsersRepo;
        private readonly IRepository<VkActivityLogItem, int> _vkActivityLogRepo;
        private readonly IItemsWithRawDataRepository<Message, int> _messagesRepo;
        private readonly IZsLogger _logger;
        //private readonly IConnectionAnalyser _connectionAnalyser;
        private readonly bool _detailedLogging;
        private readonly float _version;
        private readonly string _accessToken;
        private readonly int[] _userIds;
        private IJob _userActivityLogger;
        private readonly int _activityLogIntervalSec = 1000; // TODO: брать из конфига


        public UserWatcher(
            IConfiguration configuration,
            IMessenger messenger,
            IScheduler scheduler,
            IRepository<VkUser, int> vkUsersRepo,
            IRepository<VkActivityLogItem, int> vkActivityLogRepo,
            IItemsWithRawDataRepository<Message, int> messagesRepo,
            IZsLogger logger = null)
        {
            try
            {
                _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
                _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
                _scheduler = scheduler;
                _vkUsersRepo = vkUsersRepo ?? throw new ArgumentNullException(nameof(vkUsersRepo));
                _vkActivityLogRepo = vkActivityLogRepo ?? throw new ArgumentNullException(nameof(vkActivityLogRepo));
                _messagesRepo = messagesRepo;
                _logger = logger;

                _version = float.Parse(_configuration["Vk:Version"], CultureInfo.InvariantCulture);
                _accessToken = _configuration["Vk:AccessToken"];
                _userIds = _configuration.GetSection("Vk:UserIds").Get<int[]>();
                bool.TryParse(_configuration["DetailedLogging"]?.ToString(), out _detailedLogging);

                CreateJobs();
            }
            catch (Exception ex)
            {
                var tiex = new TypeInitializationException(typeof(UserWatcher).FullName, ex);
               _logger?.LogErrorAsync(tiex, nameof(UserWatcher));
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _scheduler.Start(3000, 1000);
                await _messenger.AddMessageToOutboxAsync($"Bot started", "ADMIN");
                await _logger?.LogInfoAsync($"{nameof(UserWatcher)} started", nameof(UserWatcher));
            }
            catch (Exception ex)
            {
                await _logger?.LogErrorAsync(ex, nameof(UserWatcher));
                throw;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _scheduler.Stop();
            await _logger?.LogInfoAsync($"{nameof(UserWatcher)} stopped", nameof(UserWatcher));
        }

        private void CreateJobs()
        {
            _userActivityLogger = new ProgramJob(
                TimeSpan.FromSeconds(_activityLogIntervalSec),
                async () => await SaveVkUsersActivity(),
                description: "logUserStatus");

            var notActiveUsers12hInformer = new SqlJob(
                TimeSpan.FromHours(1),
                QueryResultType.String,
                $"select vk.sf_cmd_get_not_active_users('{string.Join(',', _configuration.GetSection("Vk:TrackedUserIds").Get<int[]>())}', 12)",
                _configuration.GetConnectionString("Default"),
                startDate: DateTime.Now + TimeSpan.FromSeconds(5),
                description: "notActiveUsers12hInformer"
                );

            var dayErrorsAndWarningsInformer = new SqlJob(
                TimeSpan.FromHours(1),
                QueryResultType.String,
                 @"select string_agg('**' || log_type || '**  ' || to_char(insert_date, 'HH24:MI:SS') || E'\n' || log_initiator || ':  ' || log_message, E'\n\n' order by insert_date desc)
                             from bot.logs
                            where log_type in ('Warning', 'Error')
                              and insert_date > now() - interval '1 hour'",
                _configuration.GetConnectionString("Default"),
                startDate: Job.NextHour(),
                description: "dayErrorsAndWarningsInformer"
                );

            var nightErrorsAndWarningsInformer = new SqlJob(
                TimeSpan.FromDays(1),
                QueryResultType.String,
                 @" select string_agg('**' || log_type || '**  ' || to_char(insert_date, 'HH24:MI:SS') || E'\n' || log_initiator || ':  ' || log_message, E'\n\n' order by insert_date desc)
                              from bot.logs
                             where log_type in ('Warning', 'Error')
                               and insert_date > now() - interval '12 hours'",
                _configuration.GetConnectionString("Default"),
                startDate: DateTime.Today + TimeSpan.FromHours(24+10),
                description: "nightErrorsAndWarningsInformer"
                );

            notActiveUsers12hInformer.ExecutionCompleted += Job_ExecutionCompleted;
            dayErrorsAndWarningsInformer.ExecutionCompleted += Job_ExecutionCompleted;
            nightErrorsAndWarningsInformer.ExecutionCompleted += Job_ExecutionCompleted;

            _scheduler.Jobs.Add(_userActivityLogger);
            _scheduler.Jobs.Add(notActiveUsers12hInformer);
            _scheduler.Jobs.Add(dayErrorsAndWarningsInformer);
            _scheduler.Jobs.Add(nightErrorsAndWarningsInformer);
        }

        private async void Job_ExecutionCompleted(IJob job, IJobExecutionResult result)
        {
            try
            {
                if (result != null && DateTime.Now.Hour > 8 && DateTime.Now.Hour < 22)
                {
                    var todaysMessage = await _messagesRepo.FindAsync(m => m.Text == result.TextValue && m.InsertDate > DateTime.Today);

                    if (todaysMessage == null)
                        await _messenger.AddMessageToOutboxAsync(result?.TextValue, "ADMIN");
                }                   
            }
            catch (Exception ex)
            {
               _logger?.LogErrorAsync(ex, nameof(UserWatcher));
            }
        }

        /// <summary> Activity data collection </summary>
        private async Task SaveVkUsersActivity()
        {
            try
            {
                string url = null;
                if (await _vkUsersRepo.FindAsync() != null)
                {
                    url = $"https://api.vk.com/method/users.get?user_ids={string.Join(',', _userIds)}"
                        + $"&fields=online,online_mobile,online_app,last_seen&access_token={_accessToken}&v={_version.ToString(CultureInfo.InvariantCulture)}";
                }
                else
                {
                    url = $"https://api.vk.com/method/users.get?user_ids={string.Join(',', _userIds)}"
                        + $"&fields=photo_id,verified,sex,bdate,city,country,home_town,photo_max_orig,online,domain,has_mobile,"
                        + $"contacts,site,education,universities,schools,status,last_seen,followers_count,occupation,nickname,"
                        + $"relatives,relation,personal,connections,exports,activities,interests,music,movies,tv,books,games,"
                        + $"about,quotes,can_post,can_see_all_posts,can_see_audio,can_write_private_message,can_send_friend_request,"
                        + $"is_favorite,is_hidden_from_feed,timezone,screen_name,maiden_name,is_friend,friend_status,career,military,"
                        + $"blacklisted,blacklisted_by_me,can_be_invited_group&access_token={_accessToken}&v={_version.ToString(CultureInfo.InvariantCulture)}";
                    
                    // Delay to add all users in the database on the first execution
                    _userActivityLogger.IdleStepsCount = 10000;
                }

                var response = await ApiHelper.GetResponce<ApiResponse>(url, throwOnError: true);

                if (response is null)
                {
                    await SetUndefinedActivityToAllVkUsers();
                    return;
                }

                foreach (var apiUser in response.Users)
                {
                    var dbUser = await GetVkUserFromDatabase(apiUser);
                    await LogVkUserActivity(apiUser, dbUser);
                }

                // Now the delay is not needed
                if (_userActivityLogger.IdleStepsCount > 0)
                    _userActivityLogger.IdleStepsCount = 0;
            }
            catch (Exception ex)
            {
               _logger?.LogErrorAsync(ex, nameof(UserWatcher));
            }
        }

        private async Task SetUndefinedActivityToAllVkUsers()
        {
            // Для всех пользователей создаём записи в журнале
            // с неопределённым статусом, если они ещё не созданы
            var users = await _vkUsersRepo.FindAllAsync();
            users.ForEach(async u => await _vkActivityLogRepo.SaveAsync(
                new VkActivityLogItem()
                {
                    UserId = u.Id,
                    IsOnlineMobile = false,
                    LastSeen = -1,
                    IsOnline = null,
                    InsertDate = DateTime.Now
                }
            ));
        }

        private async Task<VkUser> GetVkUserFromDatabase(ApiUser apiUser)
        {
            var query = $"select * from vk.users where cast(raw_data ->> 'id' as integer) = {apiUser.Id}";

            var dbUser = await _vkUsersRepo.FindBySqlAsync(query);
            if (dbUser is null)
            {
                await _vkUsersRepo.SaveAsync((VkUser)apiUser);
                dbUser = await _vkUsersRepo.FindBySqlAsync(query);
            }

            return dbUser;
        }

        private async Task LogVkUserActivity(ApiUser apiUser, VkUser dbUser)
        {
            var lastActivityDbItem = await _vkActivityLogRepo.FindAsync(
                                    l => l.UserId == dbUser.Id,
                                    orderBy: query => query.OrderByDescending(l => l.Id));

            var currentOnlineStatus = apiUser.Online == 1 ? true : false;
            var currentMobileStatus = apiUser.OnlineMobile == 1 ? true : false;
            var currentApp = apiUser.OnlineApp;

            if (lastActivityDbItem == null
                || lastActivityDbItem.IsOnline != currentOnlineStatus
                || lastActivityDbItem.IsOnlineMobile != currentMobileStatus
                || lastActivityDbItem.OnlineApp != currentApp)
            {
                await _vkActivityLogRepo.SaveAsync(
                    new VkActivityLogItem()
                    {
                        UserId = dbUser.Id,
                        IsOnline = currentOnlineStatus,
                        IsOnlineMobile = currentMobileStatus,
                        OnlineApp = apiUser.OnlineApp,
                        LastSeen = apiUser.LastSeenUnix?.Time ?? 0,
                        InsertDate = DateTime.Now
                    });
            }
        }


    }
}
