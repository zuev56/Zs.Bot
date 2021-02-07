using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zs.App.Home.Data.Models.Vk;
using Zs.Common.Abstractions;

namespace Zs.App.Home.Services.Vk
{
    public interface IActivityService
    {
        Task<IServiceResult> AddNewVkUser(int vkIserId);
        Task<IServiceResult<ActivityLogPage>> GetLastActivity(ushort page, DateTime? fromDate = null, DateTime? toDate = null);
        Task<IServiceResult<List<PeriodUserActivity>>> GetUserStatistics(int userId, DateTime fromDate, DateTime toDate);
        Task<IServiceResult<PeriodUserActivity>> GetUserActivity(int userId, DateTime fromDate, DateTime? toDate = null);
        Task<IServiceResult<List<User>>> GetVkUsers(string filterText);
        Task<IServiceResult<Dictionary<User, int>>> GetVkUsersWithActivity(string filterText, DateTime fromDate, DateTime toDate);
        Task<IServiceResult<DetailedUserActivity>> GetDetailedUserActivity(int dbUserId);
        /// <summary> Activity data collection </summary>
        Task<IServiceResult> SaveVkUsersActivityAsync();
    }
}