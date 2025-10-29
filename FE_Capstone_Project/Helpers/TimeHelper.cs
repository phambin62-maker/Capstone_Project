namespace FE_Capstone_Project.Helpers
{
    public static class TimeHelper
    {
        public static string GetTimeAgo(DateTime date)
        {
            var ts = DateTime.Now - date;

            if (ts.TotalSeconds < 60)
                return $"{(int)ts.TotalSeconds} second{(ts.Seconds == 1 ? "" : "s")} ago";

            if (ts.TotalMinutes < 60)
            {
                int minutes = (int)Math.Round(ts.TotalMinutes);
                return $"{minutes} minute{(minutes == 1 ? "" : "s")} ago";
            }

            if (ts.TotalHours < 24)
            {
                int hours = (int)Math.Round(ts.TotalHours);
                return $"{hours} hour{(hours == 1 ? "" : "s")} ago";
            }

            if (ts.TotalDays < 7)
            {
                int days = (int)Math.Round(ts.TotalDays);
                return $"{days} day{(days == 1 ? "" : "s")} ago";
            }

            if (ts.TotalDays < 30)
            {
                int weeks = (int)Math.Round(ts.TotalDays / 7);
                return $"{weeks} week{(weeks == 1 ? "" : "s")} ago";
            }

            if (ts.TotalDays < 365)
            {
                int months = (int)Math.Round(ts.TotalDays / 30);
                return $"{months} month{(months == 1 ? "" : "s")} ago";
            }

            int years = (int)Math.Round(ts.TotalDays / 365);
            return $"{years} year{(years == 1 ? "" : "s")} ago";
        }

    }
}
