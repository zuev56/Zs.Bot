using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Zs.App.Home.Web.Areas.ApiVk.Models
{
    public class PeriodUserActivityVM
    {
        [JsonIgnore]
        public int UserId { get; init; }
        [JsonPropertyName("User")]
        [Display(Name = "Имя пользователя")]
        public string UserName { get; init; }
        public string Url { get; init; }
        public DateTime FromDate { get; init; }
        public DateTime ToDate { get; init; }
        [JsonIgnore]
        [Display(Name = "Время на сайте")]
        public TimeSpan BrowserActivityTime { get; init; }
        [Display(Name = "Время в приложении")]
        [JsonIgnore]
        public TimeSpan MobileActivityTime { get; init; }
        [JsonIgnore]
        [Display(Name = "Полное время")]
        public TimeSpan FullActivityTime => BrowserActivityTime + MobileActivityTime;

        [Display(Name = "Количество посещений")]
        public int EntranceCounter { get; init; }
        [Display(Name = "Полное время")]
        public string FullActivity => $"{FullActivityTime.Days} д {FormatValue(FullActivityTime.Hours)}:{FormatValue(FullActivityTime.Minutes)}:{FormatValue(FullActivityTime.Seconds)}";

        [Display(Name = "Время в приложении")] 
        public string MobileActivity => $"{MobileActivityTime.Days} д {FormatValue(MobileActivityTime.Hours)}:{FormatValue(MobileActivityTime.Minutes)}:{FormatValue(MobileActivityTime.Seconds)}";

        [Display(Name = "Время на сайте")] 
        public string BrowserActivity => $"{BrowserActivityTime.Days} д {FormatValue(BrowserActivityTime.Hours)}:{FormatValue(BrowserActivityTime.Minutes)}:{FormatValue(BrowserActivityTime.Seconds)}";

        private string FormatValue(int value) 
            => value < 10 ? $"0{value}" : value.ToString();
    }
}
