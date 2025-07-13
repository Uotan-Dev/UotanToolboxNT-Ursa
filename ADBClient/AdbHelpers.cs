namespace ADBClient;
public static class AdbHelpers
{
    public static string CombinePath(string path1, string path2)
    {
        return Path.Combine(path1, path2).Replace('\\', '/');
    }

    private static DateTime GetUnixEpoch()
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    public static DateTime FromUnixTime(int unixTime)
    {
        return GetUnixEpoch().AddSeconds(unixTime).ToLocalTime();
    }

    public static int ToUnixTime(DateTime dateTime)
    {
        var timeSpan = dateTime.ToUniversalTime() - GetUnixEpoch();
        return Convert.ToInt32(timeSpan.TotalSeconds);
    }
}
