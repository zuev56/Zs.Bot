﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zs.App.Home.Model;
using Zs.App.Home.Model.Abstractions;
using Zs.App.Home.Model.Data;
using Zs.App.Home.Web.Areas.VkAPI.Models;
using Zs.App.Home.Web.Models;
using Zs.Bot.Data;
using Zs.Common.Abstractions;
using Zs.Common.Extensions;

namespace Zs.App.Home.Web.Areas.VkAPI.Services
{
    public class VkActivityService : IVkActivityService
    {
        private readonly IContextFactory _contextFactory;
        private readonly int _pageSize = 50;

        public VkActivityService(IContextFactory contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        public async Task<VkActivityLogPage> GetLastActivity(ushort page, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (page < 1)
                throw new ArgumentOutOfRangeException(nameof(page), page, $"Argument '{nameof(page)}' must be greater then sero");

            using var context = _contextFactory.GetHomeContext();
            
            var query = context.VkActivityLog.AsQueryable();

            query = fromDate != null
                ? query.Where(i => i.InsertDate >= fromDate)
                : query;
            
            query = toDate != null
                ? query.Where(i => i.InsertDate <= toDate)
                : query;

            int count = query.Count();
            int skip = _pageSize * (page - 1);

            var pageItems = await query.OrderByDescending(i => i.InsertDate)
                .Skip(skip).Take(_pageSize)
                .ToListAsync();

            return new VkActivityLogPage()
            {
                Page = page,
                Items = pageItems
            };
        }

        public async Task<List<UserStatistics>> GetUserStatistics(int userId, DateTime fromDate, DateTime toDate)
        {
            var list = new List<UserStatistics>();
            
            if (userId > 0)
            {
                using var context = _contextFactory.GetHomeContext();
            
                var dbUserId = context.VkUsers
                    .FromSqlRaw($"select * from vk.users where cast(raw_data ->> 'id' as int) = {userId} limit 1")
                    .FirstOrDefault()?.Id ?? -1;
            
                //for (int i = 1; i > 0; i--)
                //{
                    //var fromDate = DateTime.Today - TimeSpan.FromDays(i);
                    //var toDate = DateTime.Today - TimeSpan.FromDays(i - 1);
                    var log = await context.VkActivityLog.Where(l => l.UserId == dbUserId
                         && l.LastSeen >= fromDate.ToUnixEpoch()
                         && l.LastSeen <= toDate.ToUnixEpoch())
                        .ToListAsync();
                    var user = await context.VkUsers.FirstOrDefaultAsync(u =>u.Id == dbUserId);
                    
                    if (user != null)
                        list.Add(UserStatistics.GetUserStatistics(dbUserId, $"{user.FirstName} {user.LastName}", log));
                //}
                return list;
            }
            else
            {
                var bag = new ConcurrentBag<UserStatistics>();
                var users = default(List<VkUser>);
            
                using (var context = _contextFactory.GetHomeContext())
                {
                    users = await context.VkUsers.ToListAsync();
                }
            
                var pOptions = new ParallelOptions()
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount // default
                };
            
                Parallel.ForEach(users, pOptions, user =>
                {
                    using (var context = _contextFactory.GetHomeContext())
                    {
                        var log = context.VkActivityLog.Where(l => l.UserId == user.Id
                             && l.LastSeen.FromUnixEpoch() >= fromDate
                             && l.LastSeen.FromUnixEpoch() <= toDate)
                            .ToList();
                        bag.Add(UserStatistics.GetUserStatistics(user.Id, $"{user.FirstName} {user.LastName}", log));
                    }
                });
            
                return bag.OrderByDescending(s => s.FullActivityTime).ToList();
            }
        }
    }
}
