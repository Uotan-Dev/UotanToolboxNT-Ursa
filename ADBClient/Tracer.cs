namespace ADBClient;
internal static class Tracer
{
    public static void Trace(string format, params object[] args)
    {
        var message = string.Format(format, args);
        Trace(message);
    }

    public static void Trace(string message)
    {
        System.Diagnostics.Trace.WriteLine(message);
    }
}
