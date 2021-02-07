using System;
using System.Text.Json.Serialization;

namespace Zs.App.Home.Data.Models.Vk
{
    public class PeriodUserActivity
    {
        [JsonIgnore]
        public int UserId { get; set; }
        [JsonPropertyName("User")]
        public string UserName { get; set; }
        public string Url { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        [JsonIgnore]
        public TimeSpan BrowserActivityTime { get; set; }
        [JsonIgnore]
        public TimeSpan MobileActivityTime { get; set; }
        public int EntranceCounter { get; set; }
    }
}
