using System;

namespace Zs.App.Home.Web.Areas.App.Models.Vk
{
    /// <summary> User activity over the period </summary>
    public class OLD_VkUserPeriodActivityVM
    {
        private DateTime _dateFrom;
        private DateTime _dateTo;
        private TimeSpan _mobileTime;
        private TimeSpan _pcTime;
        public int VkUserId { get; init; }
        public int UserName { get; init; }
        public string Period => $"{_dateFrom::dd MMM} - {_dateTo::dd MMM}";
        public string MobileTime => _mobileTime.ToString();
        public string PCTime => _pcTime.ToString();
    }
}
