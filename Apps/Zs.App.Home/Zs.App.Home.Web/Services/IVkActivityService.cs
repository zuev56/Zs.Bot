using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zs.App.Home.Model;
using Zs.App.Home.Web.Models;

namespace Zs.App.Home.Web.Services
{
    public interface IVkActivityService
    {
        Task<List<VkActivityLog>> GetLastActivity(int take, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<UserStatistics>> GetUserStatistics(int userId, DateTime fromDate, DateTime toDate);
    }
}