using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Zs.App.Home.Data.Models.Vk;
using Zs.App.Home.Data.Models.VkAPI;
using Zs.Bot.Data.Abstractions;
using Zs.Common.Abstractions;
using Zs.Common.Extensions;
using Zs.Common.Models;
using Zs.Common.Services.WebAPI;

namespace Zs.App.Home.Services.Vk
{
    public class ActivityService : IActivityService
    {
        private readonly IConfiguration _configuration;
        private readonly IRepository<ActivityLogItem, int> _vkActivityLogRepo;
        private readonly IRepository<User, int> _vkUsersRepo;
        private readonly uint _pageSize = 50;
        private readonly float _version;
        private readonly string _accessToken;
        private readonly int[] _userIds;
        private readonly ILogger<ActivityService> _logger;

        public ActivityService(
            IConfiguration configuration,
            IRepository<ActivityLogItem, int> vkActivityLogRepo,
            IRepository<User, int> vkUsersRepo,
            ILogger<ActivityService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _vkActivityLogRepo = vkActivityLogRepo ?? throw new ArgumentNullException(nameof(vkActivityLogRepo));
            _vkUsersRepo = vkUsersRepo ?? throw new ArgumentNullException(nameof(vkUsersRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _version = float.Parse(_configuration["Home:Vk:Version"], CultureInfo.InvariantCulture);
            _accessToken = _configuration["Home:Vk:AccessToken"];
            _userIds = _configuration.GetSection("Home:Vk:UserIds").Get<int[]>();
        }

        public async Task<IServiceResult<ActivityLogPage>> GetLastActivity(ushort page, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                if (page < 1)
                    throw new ArgumentOutOfRangeException(nameof(page), page, $"Argument '{nameof(page)}' must be greater then sero");

                //var data = await _vkActivityLogRepo.FindAllAsync(
                //    l => l.InsertDate > (fromDate ?? DateTime.MinValue) && l.InsertDate < (toDate ?? DateTime.MaxValue));

                uint skip = (uint)(_pageSize * (page - 1));

                var pageItems = await _vkActivityLogRepo.FindAllAsync(
                    predicate: l => l.InsertDate > (fromDate ?? DateTime.MinValue) && l.InsertDate < (toDate ?? DateTime.MaxValue),
                    orderBy: query => query.OrderByDescending(i => i.InsertDate),
                    skip: skip,
                    take: _pageSize);

                var pageItemVMs = new List<ActivityLogItem>();
                pageItems.ForEach(i => pageItemVMs.Add(
                    new ActivityLogItem
                    {
                        Id = i.Id,
                        UserId = i.UserId,
                        IsOnline = i.IsOnline,
                        IsOnlineMobile = i.IsOnlineMobile,
                        OnlineApp = i.OnlineApp,
                        LastSeen = i.LastSeen,
                        InsertDate = i.InsertDate
                    }));

                return ServiceResult<ActivityLogPage>.Success(
                    new ActivityLogPage()
                    {
                        Page = page,
                        Items = pageItemVMs
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetLastActivity error");
                return ServiceResult<ActivityLogPage>.Error();
            }
        }

        public async Task<IServiceResult<List<PeriodUserActivity>>> GetUserStatistics(int userId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var list = new List<PeriodUserActivity>();

                if (userId > 0)
                {
                    var dbUser = await _vkUsersRepo.FindBySqlAsync($"select * from vk.users where cast(raw_data ->> 'id' as int) = {userId} limit 1");
                    var dbUserId = dbUser?.Id ?? -1;

                    //for (int i = 1; i > 0; i--)
                    //{
                    //var fromDate = DateTime.Today - TimeSpan.FromDays(i);
                    //var toDate = DateTime.Today - TimeSpan.FromDays(i - 1);
                    var log = await _vkActivityLogRepo
                        .FindAllAsync(l => l.UserId == dbUserId
                                        && l.LastSeen >= fromDate.ToUnixEpoch()
                                        && l.LastSeen <= toDate.ToUnixEpoch());

                    var user = await _vkUsersRepo.FindAsync(u => u.Id == dbUserId);

                    if (user != null)
                    {
                        var userStat = GetUserStatistics(dbUserId, $"{user.FirstName} {user.LastName}", log);
                        // TODO: handle error
                        //if (!userStat.IsSuccess)
                        //    return ServiceResult<List<PeriodUserActivity>>.Error(userStat);

                        list.Add(userStat.Result);
                    }
                    //}
                    return ServiceResult<List<PeriodUserActivity>>.Success(list);
                }
                else
                {
                    var bag = new ConcurrentBag<PeriodUserActivity>();
                    var users = await _vkUsersRepo.FindAllAsync();

                    var pOptions = new ParallelOptions()
                    {
                        MaxDegreeOfParallelism = Environment.ProcessorCount // default
                    };

                    Parallel.ForEach(users, pOptions, user =>
                    {
                        var log = _vkActivityLogRepo.FindAllAsync(l => l.UserId == user.Id
                             && l.LastSeen.FromUnixEpoch() >= fromDate
                             && l.LastSeen.FromUnixEpoch() <= toDate).Result;

                        var userStat = GetUserStatistics(user.Id, $"{user.FirstName} {user.LastName}", log);
                        // TODO: handle error
                        //if (!userStat.IsSuccess)
                        //    return ServiceResult<List<PeriodUserActivity>>.Error(userStat);

                        bag.Add(userStat.Result);
                    });

                    return ServiceResult<List<PeriodUserActivity>>.Success(bag.OrderByDescending(s => s.BrowserActivityTime + s.MobileActivityTime).ToList());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUserStatistics error");
                return ServiceResult<List<PeriodUserActivity>>.Error();
            }
        }

        public async Task<IServiceResult> AddNewVkUser(int vkIserId)
        {
            try
            {
                var url = $"https://api.vk.com/method/users.get?user_ids={vkIserId}"
                        + $"&fields=photo_id,verified,sex,bdate,city,country,home_town,photo_max_orig,online,domain,has_mobile,"
                        + $"contacts,site,education,universities,schools,status,last_seen,followers_count,occupation,nickname,"
                        + $"relatives,relation,personal,connections,exports,activities,interests,music,movies,tv,books,games,"
                        + $"about,quotes,can_post,can_see_all_posts,can_see_audio,can_write_private_message,can_send_friend_request,"
                        + $"is_favorite,is_hidden_from_feed,timezone,screen_name,maiden_name,is_friend,friend_status,career,military,"
                        + $"blacklisted,blacklisted_by_me,can_be_invited_group&access_token={_accessToken}&v={_version.ToString(CultureInfo.InvariantCulture)}";

                var response = await ApiHelper.GetAsync<ApiResponse>(url, throwOnError: true);

                if (response is null)
                    return ServiceResult.Error("Response is null");

                var apiUser = response.Users.Single();
                var dbUser = await _vkUsersRepo.FindBySqlAsync($"select * from vk.users where cast(raw_data ->> 'id' as integer) = {apiUser.Id}");

                if (dbUser is null)
                {
                    return await _vkUsersRepo.SaveAsync((User)apiUser)
                        ? ServiceResult.Success("New user is successfully added")
                        : ServiceResult.Error("User adding failed");
                }
                else
                    return ServiceResult.Warning($"The user with ID {vkIserId} already exists");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User adding failed");
                return ServiceResult.Error("User adding failed");
            }
        }

        public async Task<IServiceResult<PeriodUserActivity>> GetUserActivity(int userId, DateTime fromDate, DateTime? toDate = null)
        {
            try
            {
                if (userId == default)
                    throw new ArgumentOutOfRangeException(nameof(userId));

                int fromDateUnix = fromDate.ToUnixEpoch();
                int toDateUnix = (toDate ?? DateTime.Now).ToUnixEpoch();

                var user = await _vkUsersRepo.FindByKeyAsync(userId);

                var log = await GetOrderedLog(new[] { userId }, fromDate, toDate ?? DateTime.Now);

                var periodUserActivity = new PeriodUserActivity()
                {
                    UserId = user.Id,
                    UserName = $"{user.FirstName} {user.LastName}",
                    Url = $"https://vk.com/id{JsonDocument.Parse(user.RawData).RootElement.GetProperty("id")}",
                    BrowserActivityTime = TimeSpan.FromSeconds(GetBrowserActivitySec(log)),
                    MobileActivityTime = TimeSpan.FromSeconds(GetMobileActivitySec(log)),
                    EntranceCounter = log.Count(l => l.IsOnline == true),
                    FromDate = log.FirstOrDefault()?.LastSeen.FromUnixEpoch() ?? default,
                    ToDate = log.LastOrDefault()?.LastSeen.FromUnixEpoch() ?? default
                };

                return ServiceResult<PeriodUserActivity>.Success(periodUserActivity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUserActivity error");
                return ServiceResult<PeriodUserActivity>.Error();
            }
        }
        
        public async Task<IServiceResult<DetailedUserActivity>> GetDetailedUserActivity(int userId)
        {
            try
            {
                if (userId == default)
                    throw new ArgumentOutOfRangeException(nameof(userId));

                var user = await _vkUsersRepo.FindByKeyAsync(userId);
                var log = await GetOrderedLog(new[] { userId }, new DateTime(2020, 10, 01), DateTime.Now);

                if (!log.Any())
                    return ServiceResult<DetailedUserActivity>.Warning(new DetailedUserActivity(), "No data");

                var activityDetails = new DetailedUserActivity
                {
                    UserName = $"{user.FirstName} {user.LastName}",
                    AnalyzedDaysCount = (int)(log.Max(l => l.InsertDate.Date) - log.Min(l => l.InsertDate.Date)).TotalDays,
                    ActivityDaysCount = log.Select(l => l.InsertDate.Date).Distinct().Count(),
                    BrowserEntrance = log.Count(l => l.IsOnline == true && !l.IsOnlineMobile),
                    MobileEntrance = log.Count(l => l.IsOnline == true && l.IsOnlineMobile),
                    ActivityCalendar = GetActivityForEveryDay(log),
                    //MaxDailyActivityTime = log.
                    Url = $"https://vk.com/id{JsonDocument.Parse(user.RawData).RootElement.GetProperty("id")}",
                    BrowserActivityTime = TimeSpan.FromSeconds(GetBrowserActivitySec(log.OrderBy(l => l.Id).SkipWhile(l => l.IsOnline != true).ToList())),
                    MobileActivityTime = TimeSpan.FromSeconds(GetMobileActivitySec(log.OrderBy(l => l.Id).SkipWhile(l => l.IsOnline != true).ToList())),
                };

                // TODO: Получаем активность по каждому дню с начала учёта

                // TODO: Получаем средние значения

                return ServiceResult<DetailedUserActivity>.Success(activityDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetDetailedUserActivity error");
                return ServiceResult<DetailedUserActivity>.Error();
            }
        }

        public async Task<IServiceResult<List<User>>> GetVkUsers(string filterText)
        {
            try
            {
                var users = !string.IsNullOrWhiteSpace(filterText)
                        ? await _vkUsersRepo.FindAllAsync(u => EF.Functions.ILike(u.FirstName, $"%{filterText}%") || EF.Functions.ILike(u.LastName, $"%{filterText}%"))
                        : await _vkUsersRepo.FindAllAsync();

                return ServiceResult<List<User>>.Success(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetVkUsers error");
                return ServiceResult<List<User>>.Error();
            }
        }

        public async Task<IServiceResult<Dictionary<User, int>>> GetVkUsersWithActivity(string filterText, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var userVMs = await GetVkUsers(filterText);
                // TODO: handle error
                //if (!userVMs.IsSuccess)
                //    return ServiceResult<List<User>>.Error(userVMs);

                var log = await GetOrderedLog(userVMs.Result.Select(u => u.Id).ToArray(), fromDate, toDate);

                var userActivityMap = new Dictionary<User, int>();
                userVMs.Result.AsParallel().ForAll(u =>
                {
                    var activitySec = GetActivitySec(log.Where(l => l.UserId == u.Id).OrderBy(l => l.LastSeen).ToList());
                    userActivityMap.Add(u, activitySec);
                });

                return ServiceResult<Dictionary<User, int>>.Success(userActivityMap);

                int GetActivitySec(List<ActivityLogItem> log)
                    => GetBrowserActivitySec(log) + GetMobileActivitySec(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetVkUsersWithActivity error");
                return ServiceResult<Dictionary<User, int>>.Error();
            }
        }

        public async Task<IServiceResult> SaveVkUsersActivityAsync()
        {
            ServiceResult result = ServiceResult.Success();
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

                    result.MessageToAdd = InfoMessage.Success("Filling VkUsers repository");
                }

                var response = await ApiHelper.GetAsync<ApiResponse>(url, throwOnError: true);

                if (response is null)
                {
                    await SetUndefinedActivityToAllVkUsers();

                    result.MessageToAdd = InfoMessage.Warning("Response is null. Setting undefined activity to all VkUsers");
                    return result;
                }

                foreach (var apiUser in response.Users)
                {
                    var dbUser = await GetVkUserFromDatabase(apiUser);
                    await LogVkUserActivityAsync(apiUser, dbUser);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SaveVkUsersActivityAsync error");
                return ServiceResult.Error("Users activity logging error");
            }
        }


        private IServiceResult<PeriodUserActivity> GetUserStatistics(int dbUserId, string userName, IEnumerable<ActivityLogItem> log)
        {
            try
            {
                if (log == null)
                    throw new ArgumentNullException(nameof(log));

                var userLog = log.Where(l => l.UserId == dbUserId)
                    .OrderBy(l => l.LastSeen)
                    .SkipWhile(l => l.IsOnline != true).ToList();

                int browserActivitySec = 0;
                int mobileActivitySec = 0;
                if (userLog.Any())
                {
                    // Проверка:
                    //  - Первый элемент списка должен быть IsOnline == true
                    //  - Каждый последующий элемент обрабатывается опираясь на предыдущий
                    // Обработка ситуаций:
                    //  - Предыдущий IsOnline + Mobile  -> Текущий IsOnline + !Mobile
                    //  - Предыдущий IsOnline + Mobile  -> Текущий !IsOnline
                    //  - Предыдущий IsOnline + !Mobile -> Текущий IsOnline + Mobile
                    //  - Предыдущий IsOnline + !Mobile -> Текущий !IsOnline
                    //  - Предыдущий !IsOnline          -> Текущий IsOnline + Mobile
                    //  - Предыдущий !IsOnline          -> Текущий IsOnline + !Mobile

                    for (int i = 1; i < userLog.Count; i++)
                    {
                        var prev = userLog[i - 1];
                        var cur = userLog[i];

                        if (prev.IsOnline == true && prev.IsOnlineMobile == true
                            && (cur.IsOnline == true && cur.IsOnlineMobile == false || cur.IsOnline == false))
                        {
                            mobileActivitySec += cur.LastSeen - prev.LastSeen;
                        }
                        else if (prev.IsOnline == true && prev.IsOnlineMobile == false
                            && (cur.IsOnline == true && cur.IsOnlineMobile == true || cur.IsOnline == false))
                        {
                            browserActivitySec += cur.LastSeen - prev.LastSeen;
                        }
                    }
                }

                var mobileEntrance = log.Count(l => l.IsOnline == true && l.IsOnlineMobile);
                var browserEntrance = log.Count(l => l.IsOnline == true && !l.IsOnlineMobile);

                var periodUserActivity = new PeriodUserActivity()
                {
                    UserId = dbUserId,
                    UserName = userName,
                    BrowserActivityTime = TimeSpan.FromSeconds(browserActivitySec),
                    MobileActivityTime = TimeSpan.FromSeconds(mobileActivitySec),
                    EntranceCounter = log.Count(l => l.IsOnline == true),
                    FromDate = userLog.FirstOrDefault()?.LastSeen.FromUnixEpoch() ?? default,
                    ToDate = userLog.LastOrDefault()?.LastSeen.FromUnixEpoch() ?? default
                };

                return ServiceResult<PeriodUserActivity>.Success(periodUserActivity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUserStatistics error");
                return ServiceResult<PeriodUserActivity>.Error();
            }
        }
        
        private Dictionary<DateTime, TimeSpan> GetActivityForEveryDay(List<ActivityLogItem> log)
        {
            // Вычисление активности за каждый день должно начинаться с начала суток, если предыдущие сутки закончились онлайн
            if (log == null)
                throw new ArgumentOutOfRangeException(nameof(log));

            var result = new Dictionary<DateTime, TimeSpan>();
            log = log.OrderBy(l => l.Id).SkipWhile(l => l.IsOnline != true).ToList();


            bool prevDayEndedOnlineFromBrowser = false;
            bool prevDayEndedOnlineFromMobile = false;
            log.Select(l => l.InsertDate.Date).Distinct().ToList().ForEach(day =>
            {
                int result = 0;
                var dailyLog = log.Where(l => l.InsertDate.Date == day).OrderBy(l => l.InsertDate).ToList();

                // TODO: разделить подсчёт времени с браузера и мобильного
                if (prevDayEndedOnlineFromBrowser || prevDayEndedOnlineFromMobile)
                    result += dailyLog[0].LastSeen - day.ToUnixEpoch();

                for (int i = 1; i < dailyLog.Count; i++)
                {
                    var prev = dailyLog[i - 1];
                    var cur = dailyLog[i];

                    // TODO: разделить подсчёт времени с браузера и мобильного
                    if (prev.IsOnline == true && prev.IsOnlineMobile == true
                        && (cur.IsOnline == true && cur.IsOnlineMobile == false || cur.IsOnline == false))
                    {
                        result += cur.LastSeen - prev.LastSeen;
                    }
                    else if (prev.IsOnline == true && prev.IsOnlineMobile == false
                    && (cur.IsOnline == true && cur.IsOnlineMobile == true || cur.IsOnline == false))
                    {
                        result += cur.LastSeen - prev.LastSeen;
                    }
                }

                // Фиксируем, как закончился предыдущий день
                prevDayEndedOnlineFromBrowser = false;
                prevDayEndedOnlineFromMobile = false;
                var last = dailyLog.Last();
                if (last.IsOnline == true)
                {// TODO: разделить подсчёт времени с браузера и мобильного
                    prevDayEndedOnlineFromBrowser = true;
                    result += (day + TimeSpan.FromDays(1)).ToUnixEpoch() - last.LastSeen;
                }
            });

            return result;
        }

        private async Task<List<ActivityLogItem>> GetOrderedLog(int[] dbUserIds, DateTime fromDate, DateTime toDate)
        {
            int fromDateUnix = fromDate.ToUnixEpoch();
            int toDateUnix = toDate.ToUnixEpoch();

            var log = await _vkActivityLogRepo
                .FindAllAsync(l => dbUserIds.Contains(l.UserId) && l.LastSeen >= fromDateUnix && l.LastSeen <= toDateUnix);
            log = log.OrderBy(l => l.InsertDate).SkipWhile(l => l.IsOnline != true).ToList();
            return log;
        }

        private int GetMobileActivitySec(List<ActivityLogItem> log)
        {
            // Проверка:
            //  - Первый элемент списка должен быть IsOnline == true
            //  - Каждый последующий элемент обрабатывается опираясь на предыдущий
            // Обработка ситуаций:
            //  - Предыдущий IsOnline + Mobile  -> Текущий IsOnline + !Mobile
            //  - Предыдущий IsOnline + Mobile  -> Текущий !IsOnline
            //  - Предыдущий IsOnline + !Mobile -> Текущий IsOnline + Mobile
            //  - Предыдущий IsOnline + !Mobile -> Текущий !IsOnline
            //  - Предыдущий !IsOnline          -> Текущий IsOnline + Mobile
            //  - Предыдущий !IsOnline          -> Текущий IsOnline + !Mobile

            int result = 0;
            for (int i = 1; i < log.Count; i++)
            {
                var prev = log[i - 1];
                var cur = log[i];

                if (prev.IsOnline == true && prev.IsOnlineMobile == true
                    && (cur.IsOnline == true && cur.IsOnlineMobile == false || cur.IsOnline == false))
                {
                    result += cur.LastSeen - prev.LastSeen;
                }
            }
            return result;
        }
        
        private int GetBrowserActivitySec(List<ActivityLogItem> log)
        {
            int result = 0;
            for (int i = 1; i < log.Count; i++)
            {
                var prev = log[i - 1];
                var cur = log[i];

                if (prev.IsOnline == true && prev.IsOnlineMobile == false
                    && (cur.IsOnline == true && cur.IsOnlineMobile == true || cur.IsOnline == false))
                {
                    result += cur.LastSeen - prev.LastSeen;
                }
            }
            return result;
        }

        private async Task SetUndefinedActivityToAllVkUsers()
        {
            // TODO: Check if last user activity is set as undefined
            var users = await _vkUsersRepo.FindAllAsync();
            users.ForEach(async u => await _vkActivityLogRepo.SaveAsync(
                new ActivityLogItem()
                {
                    UserId = u.Id,
                    IsOnlineMobile = false,
                    LastSeen = -1,
                    IsOnline = null,
                    InsertDate = DateTime.Now
                }
            ));
        }

        private async Task<Data.Models.Vk.User> GetVkUserFromDatabase(ApiUser apiUser)
        {
            var query = $"select * from vk.users where cast(raw_data ->> 'id' as integer) = {apiUser.Id}";

            var dbUser = await _vkUsersRepo.FindBySqlAsync(query);
            if (dbUser is null)
            {
                await _vkUsersRepo.SaveAsync((Data.Models.Vk.User)apiUser);
                dbUser = await _vkUsersRepo.FindBySqlAsync(query);
            }

            return dbUser;
        }

        private async Task LogVkUserActivityAsync(ApiUser apiUser, Data.Models.Vk.User dbUser)
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
                    new ActivityLogItem()
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
