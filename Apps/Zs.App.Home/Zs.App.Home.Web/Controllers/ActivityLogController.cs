using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zs.Common.Extensions;
using Zs.App.Home.Model;
using Zs.App.Home.Model.Data;
using Zs.App.Home.Web.Models;
using Zs.Common.Abstractions;
using Zs.App.Home.Model.Abstractions;
using Zs.App.Home.Web.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Zs.App.Home.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityLogController : ControllerBase
    {
        private readonly IVkActivityService _vkActivityService;
        private readonly IZsLogger _logger;

        public ActivityLogController(IVkActivityService vkActivityService)
        {
            _vkActivityService = vkActivityService ?? throw new ArgumentNullException(nameof(vkActivityService));
            //_logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: api/<ActivityLogController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var result = await _vkActivityService.GetLastActivity(100);
                return Ok(result);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, nameof(ActivityLogController));
                return StatusCode(500);
            }
        }

        // GET api/<ActivityLogController>/5
        [HttpGet("{id}", Name = "GetUserActivity")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var fromDate = DateTime.Today - TimeSpan.FromDays(10);
                var toDate = DateTime.Today - TimeSpan.FromDays(0);

                var result = await _vkActivityService.GetUserStatistics(id, fromDate, toDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, nameof(ActivityLogController));
                return StatusCode(500);
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
