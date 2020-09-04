using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zs.Common.Extensions;
using Zs.Service.Home.Model.Db;
using Zs.Service.Home.Web.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Zs.Service.Home.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityLogController : ControllerBase
    {
        private readonly DbContextOptions<HomeDbContext> _dbContextOptions;

        public ActivityLogController(IServiceProvider serviceProvider)
        {
            _dbContextOptions = serviceProvider.GetService<DbContextOptions<HomeDbContext>>();
        }

        // GET: api/<ActivityLogController>
        [HttpGet]
        public async Task<IEnumerable<DbVkActivityLog>> Get()
        {
            //Вывод 20-ти последних операций
            using var ctx = new HomeDbContext(_dbContextOptions);

            return await ctx.ActivityLog.Where(i => i.InsertDate < DateTime.Now - TimeSpan.FromSeconds(60))
                .OrderByDescending(i => i.InsertDate)
                .Take(10).ToListAsync();
        }

        // GET api/<ActivityLogController>/5
        [HttpGet("{id}", Name = "GetUserActivity")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var list = new List<UserStatistics>();

                if (id > 0)
                {

                    using var ctx = new HomeDbContext(_dbContextOptions);
                    var dbUserId = ctx.Users
                    .FromSqlRaw($"select * from vk.users where cast(raw_data ->> 'id' as int) = {id} limit 1")
                    .FirstOrDefault()?.UserId ?? -1;

                    for (int i = 1; i > 0; i--)
                    {
                        var fromDate = DateTime.Today - TimeSpan.FromDays(i);
                        var toDate = DateTime.Today - TimeSpan.FromDays(i - 1);

                        var log = await ctx.ActivityLog.Where(l => l.UserId == dbUserId
                         && l.LastSeen >= fromDate.ToUnixEpoch()
                         && l.LastSeen <= toDate.ToUnixEpoch())
                        .ToListAsync();
                        var user = await ctx.Users.FirstOrDefaultAsync(u =>u.UserId == dbUserId);

                        list.Add(UserStatistics.GetUserStatistics(dbUserId, $"{user.FirstName} {user.LastName}", log));
                    }
                    return new ObjectResult(list);
                }
                else
                {
                    var fromDate = DateTime.Today - TimeSpan.FromDays(1);
                    var toDate = DateTime.Today - TimeSpan.FromDays(0);

                    var bag = new ConcurrentBag<UserStatistics>();
                    var users = default(List<DbVkUser>);

                    using (var ctx = new HomeDbContext(_dbContextOptions))
                        users = await ctx.Users.ToListAsync();

                    var pOptions = new ParallelOptions()
                    {
                        MaxDegreeOfParallelism = Environment.ProcessorCount
                    };

                    Parallel.ForEach(users, pOptions, user =>
                    {
                        using (var ctx = new HomeDbContext(_dbContextOptions))
                        {
                            var log = ctx.ActivityLog.Where(l => l.UserId == user.UserId
                             && l.LastSeen.FromUnixEpoch() >= fromDate
                             && l.LastSeen.FromUnixEpoch() <= toDate)
                            .ToList();
                            bag.Add(UserStatistics.GetUserStatistics(user.UserId, $"{user.FirstName} {user.LastName}", log));
                        }
                    });
                    //list.Clear();
                    //foreach (var user in users)
                    //{
                    //    using (var ctx = new HomeDbContext(_dbContextOptions))
                    //    {
                    //        var log = await ctx.ActivityLog.Where(l => l.UserId == user.UserId
                    //             && l.LastSeenGmt >= fromDate && l.LastSeenGmt <= toDate))
                    //            .ToListAsync();
                    //        list.Add(UserStatistics.GetUserStatistics(user.UserId, $"{user.FirstName} {user.LastName}", log));
                    //    }
                    //}

                    return new ObjectResult(
                        bag.OrderByDescending(s => s.FullActivityTime).ToList());
                }
            }
            catch (Exception ex)
            {
                return new ObjectResult(ex);
            }
        }

        // POST api/<ActivityLogController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ActivityLogController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ActivityLogController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
