namespace HikariNoShisai.Common.Constants
{
    [Flags]
    public enum UserSettings
    {
        None = 0,
        NotificationsEnabled = 1 << 0,  // 1
        VerboseNotifications = 1 << 1,  // 2
    }
}
