using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zs.App.Home.Web.Areas.VkAPI.Services;

namespace Zs.App.Home.Web.Areas.VkAPI.Controllers
{
    [Area("vkapi")]
    [Route("vkapi/[controller]")]
    public class ActivityLogController : Controller
    {
        private readonly IVkActivityService _vkActivityService;
        //private readonly IZsLogger _logger;

        public ActivityLogController(IVkActivityService vkActivityService)
        {
            _vkActivityService = vkActivityService ?? throw new ArgumentNullException(nameof(vkActivityService));
        }

        //// GET: api/<ActivityLogController>
        //[HttpGet]
        //public async Task<IActionResult> GetAll()
        //{
        //    try
        //    {
        //        var result = await _vkActivityService.GetLastActivity(1);
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        //_logger.LogError(ex, nameof(ActivityLogController));
        //        return StatusCode(500);
        //    }
        //}

        // GET: api/<ActivityLogController>
        [HttpGet]
        [Route("all/{page}")]
        public async Task<IActionResult> Get(ushort page = 1)
        {
            try
            {
                var result = await _vkActivityService.GetLastActivity(page);
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
        [Route("User/{id?}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var fromDate = DateTime.Today - TimeSpan.FromDays(2);
                var toDate = DateTime.Today - TimeSpan.FromDays(1);

                var result = await _vkActivityService.GetUserStatistics(id, fromDate, toDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, nameof(ActivityLogController));
                return StatusCode(500);
            }
        }

        //// POST api/<ActivityLogController>
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}
        //
        //// PUT api/<ActivityLogController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}
        //
        //// DELETE api/<ActivityLogController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
