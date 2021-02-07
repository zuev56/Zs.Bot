using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Zs.App.Home.Data.Models.Vk
{

    public class DetailedUserActivity
    {
        public string UserName { get; set; }
        public string Url { get; set; }

        private static Dictionary<DayOfWeek, TimeSpan> _avgWeekDayActivity = new Dictionary<DayOfWeek, TimeSpan>
        {
            { DayOfWeek.Monday,    TimeSpan.FromSeconds(-1) },
            { DayOfWeek.Tuesday,   TimeSpan.FromSeconds(-1) },
            { DayOfWeek.Wednesday, TimeSpan.FromSeconds(-1) },
            { DayOfWeek.Thursday,  TimeSpan.FromSeconds(-1) },
            { DayOfWeek.Friday,    TimeSpan.FromSeconds(-1) },
            { DayOfWeek.Saturday,  TimeSpan.FromSeconds(-1) },
            { DayOfWeek.Sunday,    TimeSpan.FromSeconds(-1) }
        };

        public int AnalyzedDaysCount { get; set; }
        public int ActivityDaysCount { get; set; }
        public int BrowserEntrance { get; set; }
        public int MobileEntrance { get; set; }
        public TimeSpan BrowserActivityTime { get; set; }
        public TimeSpan MobileActivityTime { get; set; }

        /// <summary> Activity for all time </summary>
        public Dictionary<DateTime, TimeSpan> ActivityCalendar { get; set; }

        public TimeSpan AvgDailyActivityTime => ActivityDaysCount > 0 ? (BrowserActivityTime + MobileActivityTime) / ActivityDaysCount : default;
        public TimeSpan MinDailyActivityTime { get; set; }
        public TimeSpan MaxDailyActivityTime { get; set; }
        public ReadOnlyDictionary<DayOfWeek, TimeSpan> AvgWeekDayActivity { get; } = new ReadOnlyDictionary<DayOfWeek, TimeSpan>(_avgWeekDayActivity);

    }
}
