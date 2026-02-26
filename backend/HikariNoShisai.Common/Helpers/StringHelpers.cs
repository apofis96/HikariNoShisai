namespace HikariNoShisai.Common.Helpers
{
    public static class StringHelpers
    {
        public static string ReplacePlaceholder(string input, params string[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                string currentPlaceholder = "{" + i + "}";
                input = input.Replace(currentPlaceholder, values[i]);
            }

            return input;
        }

        public static string FormatDuration(TimeSpan duration)
        {
            return (int)duration.TotalHours + ":" + duration.ToString("mm");
        }
    }
}

