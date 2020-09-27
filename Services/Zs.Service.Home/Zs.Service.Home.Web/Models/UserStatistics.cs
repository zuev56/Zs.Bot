using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using Zs.Common.Extensions;
using Zs.Service.Home.Model.Abstractions;
using Zs.Service.Home.Model;

namespace Zs.Service.Home.Web.Models
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
            int dbUserId, string userName, IEnumerable<IVkActivityLog> log)
        {
            if (log == null)
                throw new ArgumentNullException(nameof(log));

            var userLog = log.Where(l => l.UserId == dbUserId)
                .OrderBy(l => l.LastSeen)
                .SkipWhile(l => l.IsOnline != true).ToList();

            var activitySecBrowser = 0d;
            var activitySecMobile = 0d;
            if (userLog.Any())
            {
                // Проверка:
                //  список начинается с IsOnline == true
                //  все чётные элементы (c [0]) IsOnline == true
                //  все нечётные элементы IsOnline == false
                if (userLog.Any() && userLog[0].IsOnline != true)
                    throw new InvalidDataException($"{nameof(userLog)} must starts with 'IsOnline == true'!");

                if (userLog.Where((l, i) => i % 2 != 0).Any(l => l.IsOnline != false))
                    throw new InvalidDataException($"{nameof(userLog)} must contains only 'IsOnline == true' odd elements");

                if (userLog.Where((l, i) => i % 2 == 0).Any(l => l.IsOnline != true))
                    throw new InvalidDataException($"{nameof(userLog)} must contains only 'IsOnline == false' even elements");

                var lastOnlineFromMobile = default(bool?);
                var intervalBegBrowser = default(DateTime);
                var intervalBegMobile = default(DateTime);
                foreach (var l in userLog)
                {
                    if (l.IsOnline == true)
                    {
                        if (l.IsOnlineMobile)
                        {
                            if (lastOnlineFromMobile == false)
                            {
                                activitySecBrowser += (l.LastSeen.FromUnixEpoch() - intervalBegBrowser).TotalSeconds;
                                intervalBegBrowser = default;
                            }

                            intervalBegMobile = l.LastSeen.FromUnixEpoch();
                            lastOnlineFromMobile = true;
                        }
                        else
                        {
                            if (lastOnlineFromMobile == true)
                            {
                                activitySecMobile += (l.LastSeen.FromUnixEpoch() - intervalBegMobile).TotalSeconds;
                                intervalBegMobile = default;
                            }

                            intervalBegBrowser = l.LastSeen.FromUnixEpoch();
                            lastOnlineFromMobile = false;
                        }
                    }
                    else
                    {
                        if (lastOnlineFromMobile == true)
                            activitySecMobile += (l.LastSeen.FromUnixEpoch() - intervalBegMobile).TotalSeconds;
                        else if (lastOnlineFromMobile == false)
                            activitySecBrowser += (l.LastSeen.FromUnixEpoch() - intervalBegBrowser).TotalSeconds;

                        lastOnlineFromMobile = null;
                        intervalBegMobile = default;
                        intervalBegBrowser = default;
                    }
                }

                if (userLog.Last().IsOnline == true)
                {
                    if (lastOnlineFromMobile == true)
                        activitySecMobile += (DateTime.Now - intervalBegMobile).TotalSeconds;
                    else if (lastOnlineFromMobile == false)
                        activitySecBrowser += (DateTime.Now - intervalBegBrowser).TotalSeconds;
                }
            }

            var mobileEntrance = log.Count(l => l.IsOnline == true && l.IsOnlineMobile);
            var browserEntrance = log.Count(l => l.IsOnline == true && !l.IsOnlineMobile);

            return new UserStatistics()
            {
                UserId = dbUserId,
                UserName = userName,
#warning 8790237
                BrowserActivityTime = TimeSpan.FromSeconds(activitySecBrowser),
                MobileActivityTime = TimeSpan.FromSeconds(activitySecMobile),
                //ActivityTime = TimeSpan.FromSeconds(activitySec - appEntrance*60 - siteEntrance*60*5),
                EntranceCounter = log.Count(l => l.IsOnline == true),
                FromDate = userLog.FirstOrDefault()?.LastSeen.FromUnixEpoch() ?? default,
                ToDate = userLog.LastOrDefault()?.LastSeen.FromUnixEpoch() ?? default
            };
        }
    }
}
