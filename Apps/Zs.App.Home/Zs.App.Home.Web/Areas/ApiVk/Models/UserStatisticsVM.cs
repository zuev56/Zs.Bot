using System;
using System.Text.Json.Serialization;

namespace Zs.App.Home.Web.Areas.ApiVk.Models
{
    public class UserStatisticsVM
    {
        [JsonIgnore]
        public int UserId { get; set; }
        [JsonPropertyName("User")]
        public string UserName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        [JsonIgnore]
        public TimeSpan BrowserActivityTime { get; set; }
        [JsonIgnore]
        public TimeSpan MobileActivityTime { get; set; }
        [JsonIgnore]
        public TimeSpan FullActivityTime => BrowserActivityTime + MobileActivityTime;

        public int EntranceCounter { get; set; }
        public string FullActivity => $"{FullActivityTime.Days} {FullActivityTime.Hours}:{FullActivityTime.Minutes}:{FullActivityTime.Seconds}";
        public string MobileActivity => $"{MobileActivityTime.Days} {MobileActivityTime.Hours}:{MobileActivityTime.Minutes}:{MobileActivityTime.Seconds}";
        public string BrowserActivity => $"{BrowserActivityTime.Days} {BrowserActivityTime.Hours}:{BrowserActivityTime.Minutes}:{BrowserActivityTime.Seconds}";

    }
}
