using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zs.Service.Home.Model.Db;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Zs.Service.HomeAPI.Controllers
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
                .OrderByDescending(i => i.InsertDate).ToListAsync();
        }

        // GET api/<ActivityLogController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
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
