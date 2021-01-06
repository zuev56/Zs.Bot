using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Zs.App.Home.Web.Areas.ApiVk.Models
{

    public class VkDetailedUserActivityVM
    {
        [Display(Name = "Имя пользователя")]
        public string UserName { get; set; }

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

        [Display(Name = "Проанализировано дней")]
        public int AnalyzedDaysCount { get; set; }
        [Display(Name = "Активных дней")]
        public int ActivityDaysCount { get; set; }
        [Display(Name = "Заходы через сайт")]
        public int BrowserEntrance { get; set; }
        [Display(Name = "Заходы через приложение")]
        public int MobileEntrance { get; set; }
        [Display(Name = "Время на сайте")]
        [DisplayFormat(DataFormatString = "{0:dd} д {0:hh\\:mm\\:ss}")]
        public TimeSpan BrowserActivityTime { get; set; }
        [Display(Name = "Время в приложении")]
        [DisplayFormat(DataFormatString = "{0:dd} д {0:hh\\:mm\\:ss}")]
        public TimeSpan MobileActivityTime { get; set; }
        [Display(Name = "Полное время")]
        [DisplayFormat(DataFormatString = "{0:dd} д {0:hh\\:mm\\:ss}")]
        public TimeSpan FullActivityTime => BrowserActivityTime + MobileActivityTime;

        /// <summary> Activity for all time </summary>
        public Dictionary<DateTime, TimeSpan> ActivityCalendar { get; set; }

        [Display(Name = "Среднее время за день")]
        [DisplayFormat(DataFormatString = "{0:dd} д {0:hh\\:mm\\:ss}")]
        public TimeSpan AvgDailyActivityTime { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd} д {0:hh\\:mm\\:ss}")]
        public TimeSpan MinDailyActivityTime { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd} д {0:hh\\:mm\\:ss}")]
        public TimeSpan MaxDailyActivityTime { get; set; }
        public ReadOnlyDictionary<DayOfWeek, TimeSpan> AvgWeekDayActivity { get; } = new ReadOnlyDictionary<DayOfWeek, TimeSpan>(_avgWeekDayActivity);

        
    }
}
