using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zs.App.Home.Web.Areas.App.Models.Vk;
using Zs.App.Home.Web.Areas.App.Services;

namespace Zs.App.Home.Web.Areas.App.Controllers
{
    [Area("app")]
    [Route("app/[controller]")] // Нужен для AJAX
    public class VkController : Controller
    {
        private readonly IVkUserActivityPresenterService _service;
        

        public VkController(IVkUserActivityPresenterService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new VkVM());
        }



        [HttpGet]
        [Route("AjaxGetUsers")]
        public async Task<IActionResult> AjaxGetUsers(string filterText)
        {
            return PartialView("_Users", await _service.GetVkUserList(filterText));
        }

        [HttpGet]
        [Route("AjaxGetUsersWithActivity")]
        public async Task<IActionResult> AjaxGetUsersWithActivity(string filterText, DateTime fromDate, DateTime toDate)
        {
            return PartialView("_Users", await _service.GetVkUserListWithActivity(filterText, fromDate, toDate));
        }

        [HttpGet]
        [Route("AjaxGetUserActivity")]
        public async Task<IActionResult> AjaxGetUserActivity(int userId, DateTime fromDate, DateTime toDate)
        {
            return PartialView("_UserActivity", await _service.GetUserActivity(userId, fromDate, toDate));
        }

        [HttpGet]
        [Route("AjaxGetDetailedUserActivity")]
        public async Task<IActionResult> AjaxGetDetailedUserActivity(int userId)
        {
            return PartialView("_DetailedUserActivity", await _service.GetDetailedUserActivity(userId));
        }
    }
}
