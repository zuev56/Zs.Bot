using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public IActionResult Index()
        {
            return View(new ServerInfo());
        }
    }
}
