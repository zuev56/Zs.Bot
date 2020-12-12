using Microsoft.AspNetCore.Mvc;
using Zs.App.Home.Web.Areas.Admin.Models;

namespace Zs.App.Home.Web.Areas.Admin.Controllers
{

    /// <summary>
    /// Shows general server status and information about services (errors, memory usage, etc.)
    /// </summary>
    [Area("admin")]
    [Route("admin/[controller]")]
    public class ServerInfoController : Controller
    {
        //[Route("")]
        //[Route("index")]
        ////[Route("admin/ServerInfo/index")]
        //[Route("~/")]
        public IActionResult Index()
        {
            return View(new ServerInfo());
        }

        //[HttpPost]
        [Route("UpdateCommonInfo")]
        public IActionResult UpdateCommonInfo()
        {
            return PartialView("_CommonInfo", new ServerInfo());
        }
    }
}
