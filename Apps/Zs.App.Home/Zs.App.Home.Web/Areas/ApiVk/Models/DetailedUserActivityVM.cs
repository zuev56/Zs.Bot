using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Zs.App.Home.Web.Areas.ApiVk.Models
{

    public class DetailedUserActivityVM
    {
        [Display(Name = "Имя пользователя")]
        public string UserName { get; init; }
        public string Url { get; init; }

        [Display(Name = "Проанализировано дней")]
        public int AnalyzedDaysCount { get; init; }
        [Display(Name = "Активных дней")]
        public int ActivityDaysCount { get; init; }
        [Display(Name = "Заходы через сайт")]
        public int BrowserEntrance { get; init; }
        [Display(Name = "Заходы через приложение")]
        public int MobileEntrance { get; init; }
        [Display(Name = "Время на сайте")]
        [DisplayFormat(DataFormatString = "{0:dd} д {0:hh\\:mm\\:ss}")]
        public TimeSpan BrowserActivityTime { get; init; }
        [Display(Name = "Время в приложении")]
        [DisplayFormat(DataFormatString = "{0:dd} д {0:hh\\:mm\\:ss}")]
        public TimeSpan MobileActivityTime { get; init; }
        [Display(Name = "Полное время")]
        [DisplayFormat(DataFormatString = "{0:dd} д {0:hh\\:mm\\:ss}")]
        public TimeSpan FullActivityTime => BrowserActivityTime + MobileActivityTime;

        /// <summary> Activity for all time </summary>
        public Dictionary<DateTime, TimeSpan> ActivityCalendar { get; init; }

        [Display(Name = "Среднее время за день")]
        [DisplayFormat(DataFormatString = "{0:dd} д {0:hh\\:mm\\:ss}")]
        public TimeSpan AvgDailyActivityTime { get; init; }
        [DisplayFormat(DataFormatString = "{0:dd} д {0:hh\\:mm\\:ss}")]
        public TimeSpan MinDailyActivityTime { get; init; }
        [DisplayFormat(DataFormatString = "{0:dd} д {0:hh\\:mm\\:ss}")]
        public TimeSpan MaxDailyActivityTime { get; init; }
        public ReadOnlyDictionary<DayOfWeek, TimeSpan> AvgWeekDayActivity { get; init; }

        
    }
}
