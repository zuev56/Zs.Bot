using System;

namespace Zs.Common.Extensions
{
    public static class DateTimeExtentions
    {
        private static DateTime NextHour(this DateTime dt)
        { // TODO: Make extension method
            var nextHour = DateTime.Now.Hour < 23
                ? DateTime.Today + TimeSpan.FromHours(1)
                : DateTime.Today + TimeSpan.FromDays(1);

            while (DateTime.Now > nextHour)
                nextHour += TimeSpan.FromHours(1);

            return nextHour;
        }
    }
}
