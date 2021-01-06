using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zs.App.Home.Web.Areas.ApiVk.Models;

namespace Zs.App.Home.Web.Areas.ApiVk.Services
{
    /// <summary> API service </summary>
    public interface IVkActivityService
    {
        Task<VkActivityLogPageVM> GetLastActivity(ushort page, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<VkPeriodUserActivityVM>> GetUserStatistics(int userId, DateTime fromDate, DateTime toDate);
    }
}