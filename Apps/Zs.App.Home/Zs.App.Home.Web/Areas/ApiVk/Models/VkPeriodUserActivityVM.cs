using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Zs.App.Home.Web.Areas.ApiVk.Models
{
    public class VkPeriodUserActivityVM
    {
        [JsonIgnore]
        public int UserId { get; set; }
        [JsonPropertyName("User")]
        [Display(Name = "Имя пользователя")]
        public string UserName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        [JsonIgnore]
        [Display(Name = "Время на сайте")]
        public TimeSpan BrowserActivityTime { get; set; }
        [Display(Name = "Время в приложении")]
        [JsonIgnore]
        public TimeSpan MobileActivityTime { get; set; }
        [JsonIgnore]
        [Display(Name = "Полное время")]
        public TimeSpan FullActivityTime => BrowserActivityTime + MobileActivityTime;

        [Display(Name = "Количество посещений")]
        public int EntranceCounter { get; set; }
        [Display(Name = "Полное время")]
        public string FullActivity => $"{FullActivityTime.Days} д {FormatValue(FullActivityTime.Hours)}:{FormatValue(FullActivityTime.Minutes)}:{FormatValue(FullActivityTime.Seconds)}";

        [Display(Name = "Время в приложении")] 
        public string MobileActivity => $"{MobileActivityTime.Days} д {FormatValue(MobileActivityTime.Hours)}:{FormatValue(MobileActivityTime.Minutes)}:{FormatValue(MobileActivityTime.Seconds)}";

        [Display(Name = "Время на сайте")] public string BrowserActivity => $"{BrowserActivityTime.Days} д {FormatValue(BrowserActivityTime.Hours)}:{FormatValue(BrowserActivityTime.Minutes)}:{FormatValue(BrowserActivityTime.Seconds)}";

        private string FormatValue(int value) 
            => value < 10 ? $"0{value}" : value.ToString();
    }
}
