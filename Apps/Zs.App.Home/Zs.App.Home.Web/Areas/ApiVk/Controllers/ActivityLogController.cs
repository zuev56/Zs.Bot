using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Zs.App.Home.Services.Vk;

namespace Zs.App.Home.Web.Areas.ApiVk.Controllers
{
    [Area("apivk")]
    [Route("apivk/[controller]")] // необходим для формирования полного маршрута при использовании короткого маршрута в HttpGet
    public class ActivityLogController : Controller
    {
        private readonly IActivityService _vkActivityService;
        //private readonly IZsLogger _logger;

        public ActivityLogController(IActivityService vkActivityService)
        {
            _vkActivityService = vkActivityService ?? throw new ArgumentNullException(nameof(vkActivityService));
        }

        [HttpGet("test", Name = "Test1")]
        public async Task<IActionResult> Test(ushort page = 1)
        {
            try
            {
                return Ok("Test");
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    InnerExceptionMessage = ex.InnerException?.Message,
                    InnerExceptionType = ex.InnerException?.GetType().FullName,
                    InnerExceptionStackTrace = ex.InnerException?.StackTrace,
                    Message = ex.Message,
                    Type = ex.GetType().FullName,
                    StackTrace = ex.StackTrace
                });
                //return StatusCode(500);
            }
        }

        [HttpGet("all/{page?}", Name = "GetLastActivityPage")]
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
                return Ok(new {
                    InnerExceptionMessage = ex.InnerException?.Message,
                    InnerExceptionType = ex.InnerException?.GetType().FullName,
                    InnerExceptionStackTrace = ex.InnerException?.StackTrace,
                    Message = ex.Message,
                    Type = ex.GetType().FullName,
                    StackTrace = ex.StackTrace
                });
                //return StatusCode(500);
            }
        }


        [HttpGet("User/{id?}", Name = "GetUserActivity")]
        public async Task<IActionResult> Get(int id = 0)
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
                return Ok(new
                {
                    InnerExceptionMessage = ex.InnerException?.Message,
                    InnerExceptionType = ex.InnerException?.GetType().FullName,
                    InnerExceptionStackTrace = ex.InnerException?.StackTrace,
                    Message = ex.Message,
                    Type = ex.GetType().FullName,
                    StackTrace = ex.StackTrace
                });
                //return StatusCode(500);
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
