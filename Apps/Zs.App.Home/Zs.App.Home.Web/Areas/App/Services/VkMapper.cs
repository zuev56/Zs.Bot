using System.Collections.Generic;
using System.Linq;
using Zs.App.Home.Data.Models.Vk;
using Zs.App.Home.Web.Areas.ApiVk.Models;
using Zs.App.Home.Web.Areas.App.Models.Vk;
using Zs.Common.Abstractions;

namespace Zs.App.Home.Web.Areas.App.Services
{
    public class VkMapper
    {
        internal IEnumerable<UserVM> ToUsersVM(IServiceResult<List<User>> users)
        {
            // TODO: handle error
            return users.Result.Select(u => ToUserVM(u));
        }

        internal IEnumerable<UserVM> ToUsersVMWithActivity(IServiceResult<Dictionary<User, int>> usersWithActivityMap)
        {
            // TODO: handle error
            return usersWithActivityMap.Result.Select(u => ToUserVM(u.Key, u.Value));
        }

        internal DetailedUserActivityVM ToDetailedUserActivityVM(IServiceResult<DetailedUserActivity> activity)
        {
            // TODO: handle error
            return new DetailedUserActivityVM
            {
                UserName             = activity.Result.UserName,
                Url                  = activity.Result.Url,
                AnalyzedDaysCount    = activity.Result.AnalyzedDaysCount,
                ActivityDaysCount    = activity.Result.ActivityDaysCount,
                BrowserEntrance      = activity.Result.BrowserEntrance,
                MobileEntrance       = activity.Result.MobileEntrance,
                BrowserActivityTime  = activity.Result.BrowserActivityTime,
                MobileActivityTime   = activity.Result.MobileActivityTime,
                ActivityCalendar     = activity.Result.ActivityCalendar,
                AvgDailyActivityTime = activity.Result.AvgDailyActivityTime,
                MinDailyActivityTime = activity.Result.MinDailyActivityTime,
                MaxDailyActivityTime = activity.Result.MaxDailyActivityTime,
                AvgWeekDayActivity   = activity.Result.AvgWeekDayActivity
            };
        }

        internal PeriodUserActivityVM ToPeriodUserActivityVM(IServiceResult<PeriodUserActivity> activity)
        {
            // TODO: handle error
            return new PeriodUserActivityVM
            {
                UserId              = activity.Result.UserId,
                UserName            = activity.Result.UserName,
                Url                 = activity.Result.Url,
                FromDate            = activity.Result.FromDate,
                ToDate              = activity.Result.ToDate,
                BrowserActivityTime = activity.Result.BrowserActivityTime,
                MobileActivityTime  = activity.Result.MobileActivityTime,
                EntranceCounter     = activity.Result.EntranceCounter
            };
        }

        private static UserVM ToUserVM(User u, int activitySec = -1)
            => new UserVM
            {
                Id = u.Id,
                UserName = $"{u.FirstName} {u.LastName}",
                ActivitySec = activitySec
            };

    }
}
