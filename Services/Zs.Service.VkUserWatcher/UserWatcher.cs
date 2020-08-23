﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zs.Bot;
using Zs.Bot.Helpers;
using Zs.Bot.Model.Db;
using Zs.Bot.Modules.Messaging;
using Zs.Common.Abstractions;
using Zs.Common.Enums;
using Zs.Common.Modules.CycleWorker;
using Zs.Common.Modules.WebAPI;
using Zs.Service.VkUserWatcher.Model;

namespace Zs.Service.VkUserWatcher
{
    internal class UserWatcher : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly IZsLogger _logger = Logger.GetInstance();
        private readonly ZsBot _bot;
        private readonly CycleWorker _cycleWorker;
        private readonly IConnectionAnalyser _connectionAnalyser;
        private readonly bool _detailedLogging;

        private readonly float _version;
        private readonly string _accessToken;
        private readonly int[] _userIds;


        public UserWatcher(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IMessenger messenger,
            IConnectionAnalyser connectionAnalyser)
        {
            try
            {
                _serviceProvider = serviceProvider;
                _configuration = configuration;

                ZsBotDbContext.Initialize(_serviceProvider.GetService<DbContextOptions<ZsBotDbContext>>());
                VkUserWatcherDbContext.Initialize(_serviceProvider.GetService<DbContextOptions<VkUserWatcherDbContext>>());

                _version = float.Parse(_configuration["Vk:Version"]);
                _accessToken = _configuration["Vk:AccessToken"];
                _userIds = _configuration.GetSection("Vk:UserIds").Get<int[]>();

                bool.TryParse(_configuration["DetailedLogging"]?.ToString(), out _detailedLogging);

                _bot = new ZsBot(_configuration, messenger);
                //_bot.Messenger.MessageReceived += Messenger_MessageReceived;

                _connectionAnalyser = connectionAnalyser;
                //_connectionAnalyser.ConnectionStatusChanged += СonnectionAnalyser_StatusChanged;

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
            _connectionAnalyser.Start(5000, 10000);
            _cycleWorker.Start(3000, 1000);
            _bot.Messenger.AddMessageToOutbox($"Bot started", "ADMIN");
            _logger.LogInfo($"{nameof(UserWatcher)} started", nameof(UserWatcher));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _connectionAnalyser.Stop();
            _cycleWorker.Stop();
            _logger.LogInfo($"{nameof(UserWatcher)} stopped", nameof(UserWatcher));

            return Task.CompletedTask;
        }

        private void CreateJobs()
        {
            // 1. Каждые 5 секунд делается запрос на определение статуса пользователей (IsOnline)
            // 2. Отсылка статистики по заданным пользователям за предыдущий день

            var getUserStatus = new ProgramJob(
                TimeSpan.FromSeconds(10),
                GetUsersStatus,
                description: "getUserStatus");

            var informAboutNotActiveUsers = new SqlJob(
                TimeSpan.FromHours(1),
                QueryResultType.String,
                $"select vk.sf_cmd_get_not_active_users('{string.Join(',', _configuration.GetSection("Vk:TrackedUserIds").Get<int[]>())}')",
                _configuration.GetConnectionString("VkUserWatcher"),
                startDate: DateTime.Now + TimeSpan.FromSeconds(5),
                description: "informAboutNotActiveUsers"
                );

            //var sendYesterdaysStatistics = new SqlJob(
            //    TimeSpan.FromDays(1),
            //    QueryResultType.String,
            //    $"select vk.sf_cmd_get_full_statistics(10, now()::date - interval '1 day', now()::date - interval '1 millisecond')",
            //    _configuration["VkUserWatcher"].ToString(),
            //    startDate: DateTime.Now.Date + TimeSpan.FromHours(24+9.5)
            //    )
            //{ Description = "sendYesterdaysStatistics" };


            informAboutNotActiveUsers.ExecutionCompleted += Job_ExecutionCompleted;

            _cycleWorker.Jobs.Add(getUserStatus);
            _cycleWorker.Jobs.Add(informAboutNotActiveUsers);
        }

        private void Job_ExecutionCompleted(Job job, IJobExecutionResult result)
        {
            if (result != null && DateTime.Now.Hour > 8 && DateTime.Now.Hour < 22)
                _bot.Messenger.AddMessageToOutbox(result?.TextValue, "ADMIN");
        }

        private async void GetUsersStatus()
        {
            try
            {
                //https://api.vk.com/method/users.get?user_ids=8790237,1234567&fields=online&access_token=cf11cfd68111cfe68111cf89bf1a6bb54b1a3fad6d&v=5.122
                //var url = $"https://api.vk.com/method/users.get?user_ids={string.Join(',',_userIds)}&fields=photo_id,verified,sex,bdate,city,country,home_town,photo_max_orig,online,domain,has_mobile,contacts,site,education,universities,schools,status,last_seen,followers_count,occupation,nickname,relatives,relation,personal,connections,exports,activities,interests,music,movies,tv,books,games,about,quotes,can_post,can_see_all_posts,can_see_audio,can_write_private_message,can_send_friend_request,is_favorite,is_hidden_from_feed,timezone,screen_name,maiden_name,is_friend,friend_status,career,military,blacklisted,blacklisted_by_me,can_be_invited_group&access_token={_accessToken}&v={_version}";
                var url = $"https://api.vk.com/method/users.get?user_ids={string.Join(',',_userIds)}&fields=online&access_token={_accessToken}&v={_version}";

                //var str = await ApiHelper.GetResponce(url);
                var response = await ApiHelper.GetResponce<ApiVkResponse>(url, throwOnError: true);

                using var ctx = new VkUserWatcherDbContext();

                if (response is null)
                {
                    // Для всех пользователей создаём записи в журнале
                    // с неопределённым статусом, если они ещё не созданы
                    var users = ctx.Users.ToList();
                    users.ForEach(u => ctx.StatusLog.Add(
                        new DbVkStatusLog()
                        {
                            UserId = u.UserId,
                            IsOnline = null,
                            InsertDate = DateTime.Now
                        }
                    ));

                    return;
                }

                foreach (var user in response.Users)
                {
                    // Для каждого пользователя проверяем текущее состояние в status_log
                    // и, если оно отличается, то делаем новую запись

                    // Находим пользователя в БД. Если нет, то добавляем
                    var dbUser = ctx.Users.FromSqlRaw($"select * from vk.users where cast(raw_data ->> 'id' as integer) = {user.Id}").FirstOrDefault();
                    if (dbUser is null)
                    {
                        ctx.Users.Add((DbVkUser)user);
                        ctx.SaveChanges();
                        dbUser = ctx.Users.FromSqlRaw($"select * from vk.users where cast(raw_data ->> 'id' as integer) = {user.Id}").FirstOrDefault();
                    }

                    // Находим для него последнюю запись в status_log
                    var lastStatusDbItem = ctx.StatusLog.OrderByDescending(l => l.StatusLogId)
                        .FirstOrDefault(l => l.UserId == dbUser.UserId);

                    var currentStatus = user.Online switch
                    {
                        0 => false,
                        1 => true,
                        _ => default(bool?)
                    };

                    if (lastStatusDbItem == null || lastStatusDbItem.IsOnline != currentStatus)
                    {
                        ctx.StatusLog.Add(new DbVkStatusLog()
                        {
                            UserId = dbUser.UserId,
                            IsOnline = currentStatus,
                            InsertDate = DateTime.Now
                        });
                    }
                }

                if (ctx.ChangeTracker.HasChanges())
                    ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }

    public enum UserStatus
    {
        Undefined = -1,
        Offline = 0,
        Online = 1
    }
}
