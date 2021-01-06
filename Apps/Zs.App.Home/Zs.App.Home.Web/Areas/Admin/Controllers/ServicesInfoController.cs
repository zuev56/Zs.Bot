using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Zs.App.Home.Web.Areas.Admin.Models;
using Zs.App.Home.Web.Areas.Admin.Services;

namespace Zs.App.Home.Web.Areas.Admin.Controllers
{
    /// <summary>
    /// Shows information about sprcific services
    /// </summary>
    [Area("admin")]
    public class ServicesInfoController : Controller
    {
        private readonly ServicesInfoService _servicesInfoService;

        public ServicesInfoController(IConfiguration configuration)
        {
            _servicesInfoService = new ServicesInfoService(configuration);
        }

        public IActionResult Index()
        {
            return View(_servicesInfoService.GetServicesInfoList());
        }
    }
}
