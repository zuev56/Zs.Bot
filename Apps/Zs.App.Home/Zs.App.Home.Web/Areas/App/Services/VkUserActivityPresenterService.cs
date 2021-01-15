using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Zs.App.Home.Model;
using Zs.App.Home.Web.Areas.ApiVk.Models;
using Zs.App.Home.Web.Areas.App.Models.Vk;
using Zs.Bot.Data.Abstractions;
using Zs.Common.Extensions;

namespace Zs.App.Home.Web.Areas.App.Services
{
    public interface IVkUserActivityPresenterService
    {
        Task<VkPeriodUserActivityVM> GetUserActivity(int userId, DateTime fromDate, DateTime? toDate = null);
        Task<List<VkUserVM>> GetVkUserList(string filterText);
        Task<List<VkUserVM>> GetVkUserListWithActivity(string filterText, DateTime fromDate, DateTime toDate);
        Task<VkDetailedUserActivityVM> GetDetailedUserActivity(int userId);
    }

    /// <summary> Service for showing user activity </summary>
    public class VkUserActivityPresenterService : IVkUserActivityPresenterService
    {
        private readonly IRepository<VkActivityLogItem, int> _vkActivityLogRepo;
        private readonly IRepository<VkUser, int> _vkUsersRepo;

        public VkUserActivityPresenterService(
            IRepository<VkActivityLogItem, int> vkActivityLogRepo,
            IRepository<VkUser, int> vkUsersRepo)
        {
            _vkActivityLogRepo = vkActivityLogRepo ?? throw new ArgumentNullException(nameof(vkActivityLogRepo));
            _vkUsersRepo = vkUsersRepo ?? throw new ArgumentNullException(nameof(vkActivityLogRepo));
        }

        public async Task<VkPeriodUserActivityVM> GetUserActivity(int userId, DateTime fromDate, DateTime? toDate = null)
        {
            if (userId == default)
                throw new ArgumentOutOfRangeException(nameof(userId));

            int fromDateUnix = fromDate.ToUnixEpoch();
            int toDateUnix = (toDate ?? DateTime.Now).ToUnixEpoch();

            var user = await _vkUsersRepo.FindByKeyAsync(userId);

            var log = await GetOrderedLog(new[] { userId }, fromDate, toDate ?? DateTime.Now);

            return new VkPeriodUserActivityVM()
            {
                UserId              = user.Id,
                UserName            = $"{user.FirstName} {user.LastName}",
                Url                 = $"https://vk.com/id{JsonDocument.Parse(user.RawData).RootElement.GetProperty("id")}",
                BrowserActivityTime = TimeSpan.FromSeconds(GetBrowserActivitySec(log)),
                MobileActivityTime  = TimeSpan.FromSeconds(GetMobileActivitySec(log)),
                EntranceCounter     = log.Count(l => l.IsOnline == true),
                FromDate            = log.FirstOrDefault()?.LastSeen.FromUnixEpoch() ?? default,
                ToDate              = log.LastOrDefault()?.LastSeen.FromUnixEpoch() ?? default
            };
        }

        public async Task<VkDetailedUserActivityVM> GetDetailedUserActivity(int userId)
        {
            var user = await _vkUsersRepo.FindByKeyAsync(userId);
            var log = await GetOrderedLog(new[] { userId }, new DateTime(2020, 10, 01), DateTime.Now);

            if (!log.Any())
                return new VkDetailedUserActivityVM();

            var activityDetails = new VkDetailedUserActivityVM
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

            activityDetails.AvgDailyActivityTime = activityDetails.FullActivityTime / activityDetails.ActivityDaysCount;


            // Получаем активность по каждому дню с начала учёта
            //log

            // Получаем средние значения


            return activityDetails;
        }

        private Dictionary<DateTime, TimeSpan> GetActivityForEveryDay(List<VkActivityLogItem> log)
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

        public async Task<List<VkUserVM>> GetVkUserList(string filterText)
        {
            var users = !string.IsNullOrWhiteSpace(filterText)
                ? await _vkUsersRepo.FindAllAsync(u => EF.Functions.ILike(u.FirstName, $"%{filterText}%") || EF.Functions.ILike(u.LastName, $"%{filterText}%"))
                : await _vkUsersRepo.FindAllAsync();

            return users.Select(u => new VkUserVM
            {
                Id = u.Id,
                UserName = $"{u.FirstName} {u.LastName}",
                //Url = $"vk.com/id{JsonDocument.Parse(u.RawData).RootElement.GetProperty("id")}"                
            }).ToList();
        }

        public async Task<List<VkUserVM>> GetVkUserListWithActivity(string filterText, DateTime fromDate, DateTime toDate)
        {
            var userVMs = await GetVkUserList(filterText);
            var log = await GetOrderedLog(userVMs.Select(u => u.Id).ToArray(), fromDate, toDate);

            userVMs.AsParallel().ForAll(u => u.ActivitySec = GetActivitySec(log.Where(l => l.UserId == u.Id).OrderBy(l => l.LastSeen).ToList()));

            return userVMs;

            int GetActivitySec(List<VkActivityLogItem> log)
                => GetBrowserActivitySec(log) + GetMobileActivitySec(log);
        }


        private async Task<List<VkActivityLogItem>> GetOrderedLog(int[] userIds, DateTime fromDate, DateTime toDate)
        {
            int fromDateUnix = fromDate.ToUnixEpoch();
            int toDateUnix = toDate.ToUnixEpoch();

            var log = await _vkActivityLogRepo
                .FindAllAsync(l => userIds.Contains(l.UserId) && l.LastSeen >= fromDateUnix && l.LastSeen <= toDateUnix);
            log = log.OrderBy(l => l.InsertDate).SkipWhile(l => l.IsOnline != true).ToList();
            return log;
        }

        private int GetMobileActivitySec(List<VkActivityLogItem> log)
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

        private int GetBrowserActivitySec(List<VkActivityLogItem> log)
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
    }
}
