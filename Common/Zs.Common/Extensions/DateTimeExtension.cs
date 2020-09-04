using System;

namespace Zs.Common.Extensions
{
    public static class DateTimeExtentions
    {
        private static readonly DateTime _unixEpoch = new DateTime(1970,1,1,0,0,0);

        private static DateTime NextHour(this DateTime dt)
        {
            var nextHour = DateTime.Now.Hour < 23
                ? DateTime.Today + TimeSpan.FromHours(1)
                : DateTime.Today + TimeSpan.FromDays(1);

            while (DateTime.Now > nextHour)
                nextHour += TimeSpan.FromHours(1);

            return nextHour;
        }

        public static DateTime FromUnixEpoch(this int seconds)
        {
            return _unixEpoch + TimeSpan.FromSeconds(seconds);
        }

        public static int ToUnixEpoch(this DateTime dateTime)
        {
            return (int)(dateTime.Subtract(_unixEpoch)).TotalSeconds;
        }
    }
}
