using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zs.App.Home.Model;
using Zs.App.Home.Web.Areas.VkAPI.Models;
using Zs.App.Home.Web.Models;

namespace Zs.App.Home.Web.Areas.VkAPI.Services
{
    public interface IVkActivityService
    {
        Task<VkActivityLogPage> GetLastActivity(ushort page, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<UserStatistics>> GetUserStatistics(int userId, DateTime fromDate, DateTime toDate);
    }
}