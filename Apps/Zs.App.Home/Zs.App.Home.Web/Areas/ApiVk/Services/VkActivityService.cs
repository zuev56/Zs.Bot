using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zs.App.Home.Data.Models;
using Zs.App.Home.Web.Areas.ApiVk.Models;
using Zs.Bot.Data.Abstractions;
using Zs.Common.Extensions;

namespace Zs.App.Home.Web.Areas.ApiVk.Services
{
    /// <summary> API service </summary>
    public class VkActivityService : IVkActivityService
    {
        private readonly IRepository<VkActivityLogItem, int> _vkActivityLogRepo;
        private readonly IRepository<VkUser, int> _vkUsersRepo;
        private readonly uint _pageSize = 50;

        public VkActivityService(
            IRepository<VkActivityLogItem, int> vkActivityLogRepo,
            IRepository<VkUser, int> vkUsersRepo)
        {
            _vkActivityLogRepo = vkActivityLogRepo ?? throw new ArgumentNullException(nameof(vkActivityLogRepo));
            _vkUsersRepo = vkUsersRepo ?? throw new ArgumentNullException(nameof(vkUsersRepo));
        }

        public async Task<VkActivityLogPageVM> GetLastActivity(ushort page, DateTime? fromDate = null, DateTime? toDate = null)
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

            var pageItemVMs = new List<VkActivityLogItemVM>();
            pageItems.ForEach(i => pageItemVMs.Add(
                new VkActivityLogItemVM 
                { 
                    Id             = i.Id,
                    UserId         = i.UserId,
                    IsOnline       = i.IsOnline,
                    IsOnlineMobile = i.IsOnlineMobile,
                    OnlineApp      = i.OnlineApp,
                    LastSeen       = i.LastSeen,
                    InsertDate     = i.InsertDate
                }));

            return new VkActivityLogPageVM()
            {
                Page = page,
                Items = pageItemVMs
            };
        }

        public async Task<List<VkPeriodUserActivityVM>> GetUserStatistics(int userId, DateTime fromDate, DateTime toDate)
        {
            var list = new List<VkPeriodUserActivityVM>();
            
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
                        
                    var user = await _vkUsersRepo.FindAsync(u =>u.Id == dbUserId);
                    
                    if (user != null)
                        list.Add(GetUserStatistics(dbUserId, $"{user.FirstName} {user.LastName}", log));
                //}
                return list;
            }
            else
            {
                var bag = new ConcurrentBag<VkPeriodUserActivityVM>();
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
                    
                    bag.Add(GetUserStatistics(user.Id, $"{user.FirstName} {user.LastName}", log));
                });
            
                return bag.OrderByDescending(s => s.FullActivityTime).ToList();
            }
        }
    
    
        public static VkPeriodUserActivityVM GetUserStatistics(
            int dbUserId, string userName, IEnumerable<VkActivityLogItem> log)
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

            return new VkPeriodUserActivityVM()
            {
                UserId = dbUserId,
                UserName = userName,
#warning 8790237
                BrowserActivityTime = TimeSpan.FromSeconds(browserActivitySec),
                MobileActivityTime = TimeSpan.FromSeconds(mobileActivitySec),
                //ActivityTime = TimeSpan.FromSeconds(activitySec - appEntrance*60 - siteEntrance*60*5),
                EntranceCounter = log.Count(l => l.IsOnline == true),
                FromDate = userLog.FirstOrDefault()?.LastSeen.FromUnixEpoch() ?? default,
                ToDate = userLog.LastOrDefault()?.LastSeen.FromUnixEpoch() ?? default
            };
        }
    }
}
