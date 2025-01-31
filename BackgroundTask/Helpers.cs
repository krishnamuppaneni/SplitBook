﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace BackgroundTasks
{
    class Helpers
    {
        public static string AccessToken
        {
            get { return (string)ApplicationData.Current.LocalSettings.Values["access_token"]; }
            set { ApplicationData.Current.LocalSettings.Values["access_token"] = value; }
        }

        public static string AccessTokenSecret
        {
            get { return (string)ApplicationData.Current.LocalSettings.Values["access_token_secret"]; }
            set { ApplicationData.Current.LocalSettings.Values["access_token_secret"] = value; }
        }

        public static string NotificationsLastUpdated
        {
            get { return (string)ApplicationData.Current.LocalSettings.Values["notifications_last_updated"]; }
            set { ApplicationData.Current.LocalSettings.Values["notifications_last_updated"] = value; }
        }

        //public static string DateTimeHelper(string dateTime)
        //{
        //    const int SECOND = 1;
        //    const int MINUTE = 60 * SECOND;
        //    const int HOUR = 60 * MINUTE;
        //    const int DAY = 24 * HOUR;
        //    const int MONTH = 30 * DAY;

        //    var ts = new TimeSpan(DateTime.UtcNow.Ticks - DateTimeOffset.Parse(dateTime).UtcDateTime.Ticks);
        //    double delta = Math.Abs(ts.TotalSeconds);

        //    if (delta < 1 * MINUTE)
        //        return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";

        //    if (delta < 2 * MINUTE)
        //        return "a minute ago";

        //    if (delta < 45 * MINUTE)
        //        return ts.Minutes + " minutes ago";

        //    if (delta < 90 * MINUTE)
        //        return "an hour ago";

        //    if (delta < 24 * HOUR)
        //        return ts.Hours + " hours ago";

        //    if (delta < 48 * HOUR)
        //        return "yesterday";

        //    if (delta < 30 * DAY)
        //        return ts.Days + " days ago";

        //    if (delta < 12 * MONTH)
        //    {
        //        int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
        //        return months <= 1 ? "one month ago" : months + " months ago";
        //    }
        //    else
        //    {
        //        int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
        //        return years <= 1 ? "one year ago" : years + " years ago";
        //    }
        //}
    }
}
