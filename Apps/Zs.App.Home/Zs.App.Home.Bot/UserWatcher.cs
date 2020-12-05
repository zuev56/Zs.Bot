﻿using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Zs.Bot;
using Zs.Bot.Data;
using Zs.Bot.Services.Messaging;
using Zs.Common.Abstractions;
using Zs.Common.Enums;
using Zs.Common.Services.Scheduler;
using Zs.Common.Services.WebAPI;
using Zs.App.Home.Model;
using Zs.App.Home.Model.Abstractions;
using Zs.App.Home.Model.Data;
using Zs.App.Home.Model.VkAPI;

namespace Zs.App.Home.Bot
{
    internal class UserWatcher : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly IZsLogger _logger;
        private readonly IContextFactory _contextFactory;
        private readonly IMessenger _messenger;
        private readonly IScheduler _scheduler;
        //private readonly IConnectionAnalyser _connectionAnalyser;
        private readonly bool _detailedLogging;
        private readonly float _version;
        private readonly string _accessToken;
        private readonly int[] _userIds;


        public UserWatcher(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IMessenger messenger,
            IContextFactory contextFactory,
            IZsLogger logger)
        {
            try
            {
                _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
                _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
                _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
                _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
                _logger = logger;

                _version = float.Parse(_configuration["Vk:Version"], CultureInfo.InvariantCulture);
                _accessToken = _configuration["Vk:AccessToken"];
                _userIds = _configuration.GetSection("Vk:UserIds").Get<int[]>();
                bool.TryParse(_configuration["DetailedLogging"]?.ToString(), out _detailedLogging);

                _scheduler = new Scheduler(configuration, _logger);
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
            var logUserStatus = new ProgramJob(
                TimeSpan.FromSeconds(10),
                GetUsersStatus,
                description: "logUserStatus");

            var informAboutNotActiveUsers24h = new SqlJob(
                TimeSpan.FromHours(1),
                QueryResultType.String,
                $"select vk.sf_cmd_get_not_active_users('{string.Join(',', _configuration.GetSection("Vk:TrackedUserIds").Get<int[]>())}', 24)",
                _configuration.GetConnectionString("Default"),
                startDate: DateTime.Now + TimeSpan.FromSeconds(5),
                description: "informAboutNotActiveUsers24h"
                );

            var informAboutNotActiveUsers12h = new SqlJob(
                TimeSpan.FromHours(1),
                QueryResultType.String,
                $"select vk.sf_cmd_get_not_active_users('{string.Join(',', _configuration.GetSection("Vk:TrackedUserIds").Get<int[]>())}', 12)",
                _configuration.GetConnectionString("Default"),
                startDate: DateTime.Now + TimeSpan.FromSeconds(5),
                description: "informAboutNotActiveUsers12h"
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

            informAboutNotActiveUsers24h.ExecutionCompleted += Job_ExecutionCompleted;
            informAboutNotActiveUsers12h.ExecutionCompleted += Job_ExecutionCompleted;
            sendDayErrorsAndWarnings.ExecutionCompleted += Job_ExecutionCompleted;
            sendNightErrorsAndWarnings.ExecutionCompleted += Job_ExecutionCompleted;

            _scheduler.Jobs.Add(logUserStatus);
            _scheduler.Jobs.Add(informAboutNotActiveUsers24h);
            _scheduler.Jobs.Add(informAboutNotActiveUsers12h);
            _scheduler.Jobs.Add(sendDayErrorsAndWarnings);
            _scheduler.Jobs.Add(sendNightErrorsAndWarnings);
        }

        private async void Job_ExecutionCompleted(IJob job, IJobExecutionResult result)
        {
            try
            {
                if (result != null && DateTime.Now.Hour > 8 && DateTime.Now.Hour < 22)
                   await _messenger.AddMessageToOutboxAsync(result?.TextValue, "ADMIN");
            }
            catch (Exception ex)
            {
               _logger?.LogErrorAsync(ex, nameof(UserWatcher));
            }
        }

        private async void GetUsersStatus()
        {
            try
            {
                using var homeContext = _contextFactory.GetHomeContext();

                //https://api.vk.com/method/users.get?user_ids=8790237,1234567&fields=online&access_token=cf11cfd68111cfe68111cf89bf1a6bb54b1a3fad6d&v=5.122
                var url = homeContext.VkUsers.Any()
                    ? $"https://api.vk.com/method/users.get?user_ids={string.Join(',',_userIds)}&fields=online,online_mobile,online_app,last_seen&access_token={_accessToken}&v={_version.ToString(CultureInfo.InvariantCulture)}"
                    : $"https://api.vk.com/method/users.get?user_ids={string.Join(',', _userIds)}&fields=photo_id,verified,sex,bdate,city,country,home_town,photo_max_orig,online,domain,has_mobile,contacts,site,education,universities,schools,status,last_seen,followers_count,occupation,nickname,relatives,relation,personal,connections,exports,activities,interests,music,movies,tv,books,games,about,quotes,can_post,can_see_all_posts,can_see_audio,can_write_private_message,can_send_friend_request,is_favorite,is_hidden_from_feed,timezone,screen_name,maiden_name,is_friend,friend_status,career,military,blacklisted,blacklisted_by_me,can_be_invited_group&access_token={_accessToken}&v={_version}";

                //var str = await ApiHelper.GetResponce(url);
                var response = await ApiHelper.GetResponce<ApiResponse>(url, throwOnError: true);

                if (response is null)
                {
                    // Для всех пользователей создаём записи в журнале
                    // с неопределённым статусом, если они ещё не созданы
                    var users = homeContext.VkUsers.ToList();
                    users.ForEach(u => homeContext.VkActivityLog.Add(
                        new VkActivityLogItem()
                        {
                            UserId = u.Id,
                            IsOnlineMobile = false,
                            LastSeen = -1,
                            IsOnline = null,
                            InsertDate = DateTime.Now
                        }
                    ));

                    return;
                }

                foreach (var user in response.Users)
                {
                    var dbUser = homeContext.VkUsers.FromSqlRaw($"select * from vk.users where cast(raw_data ->> 'id' as integer) = {user.Id}").FirstOrDefault();
                    if (dbUser is null)
                    {
                        homeContext.VkUsers.Add((VkUser)user);
                        homeContext.SaveChanges();
                        dbUser = homeContext.VkUsers.FromSqlRaw($"select * from vk.users where cast(raw_data ->> 'id' as integer) = {user.Id}").FirstOrDefault();
                    }

                    var lastActivityDbItem = homeContext.VkActivityLog.OrderByDescending(l => l.Id)
                        .FirstOrDefault(l => l.UserId == dbUser.Id);

                    var currentOnlineStatus = user.Online == 1 ? true : false;
                    var currentMobileStatus = user.OnlineMobile == 1 ? true : false;
                    var currentApp = user.OnlineApp;

                    if (lastActivityDbItem == null
                        || lastActivityDbItem.IsOnline != currentOnlineStatus
                        || lastActivityDbItem.IsOnlineMobile != currentMobileStatus
                        || lastActivityDbItem.OnlineApp != currentApp)
                    {
                        homeContext.VkActivityLog.Add(new VkActivityLogItem()
                        {
                            UserId = dbUser.Id,
                            IsOnline = currentOnlineStatus,
                            IsOnlineMobile = currentMobileStatus,
                            OnlineApp = user.OnlineApp,
                            LastSeen = user.LastSeenUnix?.Time ?? 0,
                            InsertDate = DateTime.Now
                        });
                    }
                }

                if (homeContext.ChangeTracker.HasChanges())
                    homeContext.SaveChanges();
            }
            catch (Exception ex)
            {
               _logger?.LogErrorAsync(ex, nameof(UserWatcher));
            }
        }
    }
}
