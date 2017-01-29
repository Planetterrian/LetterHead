using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class DateTimeHelper
{
    private const int SECOND = 1;
    private const int MINUTE = 60 * SECOND;
    private const int HOUR = 60 * MINUTE;
    private const int DAY = 24 * HOUR;
    private const int MONTH = 30 * DAY;

    /// <summary>
    /// Returns a friendly version of the provided DateTime, relative to now. E.g.: "2 days ago", or "in 6 months".
    /// </summary>
    /// <param name="dateTime">The DateTime to compare to Now</param>
    /// <returns>A friendly string</returns>
    public static string GetFriendlyRelativeTime(DateTime dateTime)
    {
        if (DateTime.UtcNow.Ticks == dateTime.Ticks)
        {
            return "Now!";
        }

        bool isFuture = (DateTime.UtcNow.Ticks < dateTime.Ticks);
        var ts = DateTime.UtcNow.Ticks < dateTime.Ticks ? new TimeSpan(dateTime.Ticks - DateTime.UtcNow.Ticks) : new TimeSpan(DateTime.UtcNow.Ticks - dateTime.Ticks);

        double delta = ts.TotalSeconds;

        if (delta < 10)
        {
            return "just now";
        }

        if (delta < 1 * MINUTE)
        {
            return isFuture ? (ts.Seconds == 1 ? "one second" : ts.Seconds + " seconds") : ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";
        }
        if (delta < 2 * MINUTE)
        {
            return isFuture ? "a minute" : "a minute ago";
        }
        if (delta < 45 * MINUTE)
        {
            return isFuture ? ts.Minutes + " minutes" : ts.Minutes + " minutes ago";
        }
        if (delta < 90 * MINUTE)
        {
            return isFuture ? "an hour" : "an hour ago";
        }
        if (delta < 24 * HOUR)
        {
            return isFuture ? ts.Hours + " hours" : ts.Hours + " hours ago";
        }
        if (delta < 48 * HOUR)
        {
            return isFuture ? "tomorrow" : "yesterday";
        }
        if (delta < 30 * DAY)
        {
            return isFuture ? ts.Days + " days" : ts.Days + " days ago";
        }
        if (delta < 12 * MONTH)
        {
            int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
            return isFuture ? (months <= 1 ? "one month" : months + " months") : months <= 1 ? "one month ago" : months + " months ago";
        }
        else
        {
            int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
            return isFuture ? (years <= 1 ? "one year" : years + " years") : years <= 1 ? "one year ago" : years + " years ago";
        }
    }

    public static string GetFriendlyRelativeTimeShort(DateTime dateTime)
    {
        if (DateTime.UtcNow.Ticks == dateTime.Ticks)
        {
            return "Now!";
        }

        bool isFuture = (DateTime.UtcNow.Ticks < dateTime.Ticks);
        var ts = DateTime.UtcNow.Ticks < dateTime.Ticks ? new TimeSpan(dateTime.Ticks - DateTime.UtcNow.Ticks) : new TimeSpan(DateTime.UtcNow.Ticks - dateTime.Ticks);

        double delta = ts.TotalSeconds;

        if (delta < 10)
        {
            return "just now";
        }

        if (delta < 1 * MINUTE)
        {
            return isFuture ? (ts.Seconds == 1 ? "1s" : ts.Seconds + "s") : ts.Seconds == 1 ? "s ago" : ts.Seconds + "s ago";
        }
        if (delta < 2 * MINUTE)
        {
            return isFuture ? "1m" : "1m ago";
        }
        if (delta < 45 * MINUTE)
        {
            return isFuture ? ts.Minutes + "m" : ts.Minutes + "m ago";
        }
        if (delta < 90 * MINUTE)
        {
            return isFuture ? "1h" : "1h ago";
        }
        if (delta < 24 * HOUR)
        {
            return isFuture ? ts.Hours + "h" : ts.Hours + "h ago";
        }
        if (delta < 48 * HOUR)
        {
            return isFuture ? "1d" : "1d ago";
        }
        if (delta < 30 * DAY)
        {
            return isFuture ? ts.Days + "d" : ts.Days + "d ago";
        }
        if (delta < 12 * MONTH)
        {
            int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
            return isFuture ? (months <= 1 ? "1m" : months + "m") : months <= 1 ? "m ago" : months + "m ago";
        }
        else
        {
            int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
            return isFuture ? (years <= 1 ? "1y" : years + "y") : years <= 1 ? "y ago" : years + "y ago";
        }
    }
}