namespace HikariNoShisai.Common.Helpers
{
    public static class ButtonFormatter
    {
        public static string AddButtons(string input, params string[] buttons)
        {
            if (buttons.Length == 0)
            {
                input += "\n<keyboard reply_remove>";
            }
            else            {
                input += "\n<keyboard>\n";
                foreach (var button in buttons)
                {
                    input += $"<button text=\"{button}\" \n>";
                }
            }

            return input + "</keyboard>";
        }
    }
}