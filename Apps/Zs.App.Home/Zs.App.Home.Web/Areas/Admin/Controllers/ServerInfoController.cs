using Microsoft.AspNetCore.Mvc;
using Zs.App.Home.Web.Areas.Admin.Models.ServerInfo;

namespace Zs.App.Home.Web.Areas.Admin.Controllers
{

    /// <summary>
    /// Shows general server status(errors, memory usage, etc.)
    /// </summary>
    [Area("admin")]
    [Route("admin/[controller]")] // Нужен для AJAX
    public class ServerInfoController : Controller
    {
        private readonly Server _serverInfo = new Server();

        //[Route("")]
        //[Route("index")]
        ////[Route("admin/ServerInfo/index")]
        //[Route("~/")]
        public IActionResult Index()
        {
            return View(_serverInfo);
        }

        //[HttpPost]
        [Route("AjaxUpdateCommonInfo")]
        public IActionResult AjaxUpdateCommonInfo()
        {
            return PartialView("_EnvironmentInfo", _serverInfo.EnvironmentInfo);
        }
    }
}
