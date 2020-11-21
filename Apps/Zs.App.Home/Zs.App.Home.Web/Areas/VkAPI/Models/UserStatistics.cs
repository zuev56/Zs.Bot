using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using Zs.Common.Extensions;
using Zs.App.Home.Model.Abstractions;
using Zs.App.Home.Model;

namespace Zs.App.Home.Web.Areas.VkAPI.Models
{
    public class UserStatistics
    {
        [JsonIgnore]
        public int UserId { get; set; }
        [JsonPropertyName("User")]
        public string UserName { get; set; }
        //[JsonIgnore]
        public DateTime FromDate { get; set; }
        //[JsonIgnore]
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

        public static UserStatistics GetUserStatistics(
            int dbUserId, string userName, IEnumerable<IVkActivityLogItem> log)
        {
            if (log == null)
                throw new ArgumentNullException(nameof(log));

            var userLog = log.Where(l => l.UserId == dbUserId)
                .OrderBy(l => l.LastSeen)
                .SkipWhile(l => l.IsOnline != true).ToList();

            int browserActivitySec = 0;
            int mobileActivitySec = 0;
            if (userLog.Any())
            {
                // Проверка:
                //  - Первый элемент списка должен быть IsOnline == true
                //  - Каждый последующий элемент обрабатывается опираясь на предыдущий
                // Обработка ситуаций:
                //  - Предыдущий IsOnline + Mobile  -> Текущий IsOnline + !Mobile
                //  - Предыдущий IsOnline + Mobile  -> Текущий !IsOnline
                //  - Предыдущий IsOnline + !Mobile -> Текущий IsOnline + Mobile
                //  - Предыдущий IsOnline + !Mobile -> Текущий !IsOnline
                //  - Предыдущий !IsOnline          -> Текущий IsOnline + Mobile
                //  - Предыдущий !IsOnline          -> Текущий IsOnline + !Mobile

                for (int i = 1; i < userLog.Count; i++)
                {
                    var prev = userLog[i - 1];
                    var cur = userLog[i];

                    if (prev.IsOnline == true && prev.IsOnlineMobile == true
                        && (cur.IsOnline == true && cur.IsOnlineMobile == false || cur.IsOnline == false))
                    {
                        mobileActivitySec += cur.LastSeen - prev.LastSeen;
                    }
                    else if (prev.IsOnline == true && prev.IsOnlineMobile == false
                        && (cur.IsOnline == true && cur.IsOnlineMobile == true || cur.IsOnline == false))
                    {
                        browserActivitySec += cur.LastSeen - prev.LastSeen;
                    }
                }
            }

            var mobileEntrance = log.Count(l => l.IsOnline == true && l.IsOnlineMobile);
            var browserEntrance = log.Count(l => l.IsOnline == true && !l.IsOnlineMobile);

            return new UserStatistics()
            {
                UserId = dbUserId,
                UserName = userName,
#warning 8790237
                BrowserActivityTime = TimeSpan.FromSeconds(browserActivitySec),
                MobileActivityTime = TimeSpan.FromSeconds(mobileActivitySec),
                //ActivityTime = TimeSpan.FromSeconds(activitySec - appEntrance*60 - siteEntrance*60*5),
                EntranceCounter = log.Count(l => l.IsOnline == true),
                FromDate = userLog.FirstOrDefault()?.LastSeen.FromUnixEpoch() ?? default,
                ToDate = userLog.LastOrDefault()?.LastSeen.FromUnixEpoch() ?? default
            };
        }
    }
}
