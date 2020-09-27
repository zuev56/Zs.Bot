﻿using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Zs.Bot;
using Zs.Bot.Model.Data;
using Zs.Bot.Modules.Messaging;
using Zs.Common.Abstractions;
using Zs.Common.Enums;
using Zs.Common.Modules.CycleWorker;
using Zs.Common.Modules.WebAPI;
using Zs.Service.Home.Model.Db;
using Zs.Service.Home.Model.Vk;

namespace Zs.Service.Home.Bot
{
    internal class UserWatcher : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly IZsLogger _logger;
        private readonly IContextFactory<BotContext> _botContextFactory;
        private readonly IContextFactory<HomeDbContext> _homeContextFactory;
        private readonly ZsBot _bot;
        private readonly CycleWorker _cycleWorker;
        //private readonly IConnectionAnalyser _connectionAnalyser;
        private readonly bool _detailedLogging;

        private readonly float _version;
        private readonly string _accessToken;
        private readonly int[] _userIds;


        public UserWatcher(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IMessenger messenger,
            IContextFactory<BotContext> botContextFactory,
            IContextFactory<HomeDbContext> homeContextFactory,
            IZsLogger logger)
        {
            try
            {
                if (serviceProvider == null)
                    throw new ArgumentNullException(nameof(serviceProvider));

                if (configuration == null)
                    throw new ArgumentNullException(nameof(configuration));

                if (messenger is null)
                    throw new ArgumentNullException(nameof(messenger));

                if (botContextFactory is null)
                    throw new ArgumentNullException(nameof(botContextFactory));

                if (homeContextFactory is null)
                    throw new ArgumentNullException(nameof(homeContextFactory));

                if (logger is null)
                    throw new ArgumentNullException(nameof(logger));

                _configuration = configuration;
                _botContextFactory = botContextFactory;
                _homeContextFactory = homeContextFactory;
                _logger = logger;
                _serviceProvider = serviceProvider;

                _version = float.Parse(_configuration["Vk:Version"], CultureInfo.InvariantCulture);
                _accessToken = _configuration["Vk:AccessToken"];
                _userIds = _configuration.GetSection("Vk:UserIds").Get<int[]>();

                bool.TryParse(_configuration["DetailedLogging"]?.ToString(), out _detailedLogging);

                _bot = new ZsBot(_configuration, messenger, _botContextFactory, _logger);

                _cycleWorker = new CycleWorker(_logger, _detailedLogging);
                CreateJobs();
            }
            catch (Exception ex)
            {
                var tiex = new TypeInitializationException(typeof(UserWatcher).FullName, ex);
                _logger.LogError(tiex, nameof(UserWatcher));
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _cycleWorker.Start(3000, 1000);
                _bot.Messenger.AddMessageToOutbox($"Bot started", "ADMIN");
                _logger.LogInfo($"{nameof(UserWatcher)} started", nameof(UserWatcher));

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(UserWatcher));
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cycleWorker.Stop();
            _logger.LogInfo($"{nameof(UserWatcher)} stopped", nameof(UserWatcher));

            return Task.CompletedTask;
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
                _configuration.GetConnectionString("Home"),
                startDate: DateTime.Now + TimeSpan.FromSeconds(5),
                description: "informAboutNotActiveUsers24h"
                );

            var informAboutNotActiveUsers12h = new SqlJob(
                TimeSpan.FromHours(1),
                QueryResultType.String,
                $"select vk.sf_cmd_get_not_active_users('{string.Join(',', _configuration.GetSection("Vk:TrackedUserIds").Get<int[]>())}', 12)",
                _configuration.GetConnectionString("Home"),
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
                _configuration.GetConnectionString("Home"),
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
                _configuration.GetConnectionString("Home"),
                startDate: DateTime.Today + TimeSpan.FromHours(24+10),
                description: "sendNightErrorsAndWarnings"
                );



            informAboutNotActiveUsers24h.ExecutionCompleted += Job_ExecutionCompleted;
            informAboutNotActiveUsers12h.ExecutionCompleted += Job_ExecutionCompleted;
            sendDayErrorsAndWarnings.ExecutionCompleted += Job_ExecutionCompleted;
            sendNightErrorsAndWarnings.ExecutionCompleted += Job_ExecutionCompleted;

            _cycleWorker.Jobs.Add(logUserStatus);
            _cycleWorker.Jobs.Add(informAboutNotActiveUsers24h);
            _cycleWorker.Jobs.Add(informAboutNotActiveUsers12h);
            _cycleWorker.Jobs.Add(sendDayErrorsAndWarnings);
            _cycleWorker.Jobs.Add(sendNightErrorsAndWarnings);
        }

        private void Job_ExecutionCompleted(Job job, IJobExecutionResult result)
        {
            try
            {
                if (result != null && DateTime.Now.Hour > 8 && DateTime.Now.Hour < 22)
                    _bot.Messenger.AddMessageToOutbox(result?.TextValue, "ADMIN");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(UserWatcher));
            }
        }

        private async void GetUsersStatus()
        {
            try
            {
                //https://api.vk.com/method/users.get?user_ids=8790237,1234567&fields=online&access_token=cf11cfd68111cfe68111cf89bf1a6bb54b1a3fad6d&v=5.122
                //var url = $"https://api.vk.com/method/users.get?user_ids={string.Join(',',_userIds)}&fields=photo_id,verified,sex,bdate,city,country,home_town,photo_max_orig,online,domain,has_mobile,contacts,site,education,universities,schools,status,last_seen,followers_count,occupation,nickname,relatives,relation,personal,connections,exports,activities,interests,music,movies,tv,books,games,about,quotes,can_post,can_see_all_posts,can_see_audio,can_write_private_message,can_send_friend_request,is_favorite,is_hidden_from_feed,timezone,screen_name,maiden_name,is_friend,friend_status,career,military,blacklisted,blacklisted_by_me,can_be_invited_group&access_token={_accessToken}&v={_version}";
                var url = $"https://api.vk.com/method/users.get?user_ids={string.Join(',',_userIds)}&fields=online,online_mobile,online_app,last_seen&access_token={_accessToken}&v={_version.ToString(CultureInfo.InvariantCulture)}";

                //var str = await ApiHelper.GetResponce(url);
                var response = await ApiHelper.GetResponce<ApiResponse>(url, throwOnError: true);

                using var ctx = _homeContextFactory.GetContext();

                if (response is null)
                {
                    // Для всех пользователей создаём записи в журнале
                    // с неопределённым статусом, если они ещё не созданы
                    var users = ctx.Users.ToList();
                    users.ForEach(u => ctx.ActivityLog.Add(
                        new DbVkActivityLog()
                        {
                            UserId = u.UserId,
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
                    var dbUser = ctx.Users.FromSqlRaw($"select * from vk.users where cast(raw_data ->> 'id' as integer) = {user.Id}").FirstOrDefault();
                    if (dbUser is null)
                    {
                        ctx.Users.Add((DbVkUser)user);
                        ctx.SaveChanges();
                        dbUser = ctx.Users.FromSqlRaw($"select * from vk.users where cast(raw_data ->> 'id' as integer) = {user.Id}").FirstOrDefault();
                    }

                    var lastActivityDbItem = ctx.ActivityLog.OrderByDescending(l => l.ActivityLogId)
                        .FirstOrDefault(l => l.UserId == dbUser.UserId);

                    var currentOnlineStatus = user.Online == 1 ? true : false;
                    var currentMobileStatus = user.OnlineMobile == 1 ? true : false;
                    var currentApp = user.OnlineApp;

                    if (lastActivityDbItem == null
                        || lastActivityDbItem.IsOnline != currentOnlineStatus
                        || lastActivityDbItem.IsOnlineMobile != currentMobileStatus
                        || lastActivityDbItem.OnlineApp != currentApp)
                    {
                        ctx.ActivityLog.Add(new DbVkActivityLog()
                        {
                            UserId = dbUser.UserId,
                            IsOnline = currentOnlineStatus,
                            IsOnlineMobile = currentMobileStatus,
                            OnlineApp = user.OnlineApp,
                            LastSeen = user.LastSeenUnix?.Time ?? 0,
                            InsertDate = DateTime.Now
                        });
                    }
                }

                if (ctx.ChangeTracker.HasChanges())
                    ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(UserWatcher));
            }
        }
    }
}