using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zs.App.Home.Model;
using Zs.App.Home.Web.Areas.ApiVk.Models;
using Zs.App.Home.Web.Models;

namespace Zs.App.Home.Web.Areas.ApiVk.Services
{
    public interface IVkActivityService
    {
        Task<VkActivityLogPageVM> GetLastActivity(ushort page, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<UserStatisticsVM>> GetUserStatistics(int userId, DateTime fromDate, DateTime toDate);
    }
}