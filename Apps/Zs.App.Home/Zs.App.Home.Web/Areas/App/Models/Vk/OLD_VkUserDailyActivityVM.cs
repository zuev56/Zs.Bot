using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zs.App.Home.Web.Areas.App.Models.Vk
{
    /// <summary> User activity per day </summary>
    public class OLD_VkUserDailyActivityVM
    {
        private DateTime _date;
        private TimeSpan _mobileTime;
        private TimeSpan _pcTime;
        public int VkUserId { get; init; }
        public int UserName { get; init; }
        public string DayInfo => _date.ToString();
        public string MobileTime => _mobileTime.ToString();
        public string PCTime => _pcTime.ToString();

    }
}
