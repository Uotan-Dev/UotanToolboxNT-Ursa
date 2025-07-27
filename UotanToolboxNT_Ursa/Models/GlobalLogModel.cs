using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using UotanToolboxNT_Ursa.ViewModels;

namespace UotanToolboxNT_Ursa.Models;

public class GlobalLogModel
{
    private static readonly Lock LockObject = new();

    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Debug
    }

    static GlobalLogModel()
    {
        if (!Global.LogDirectory.Exists)
        {
            Directory.CreateDirectory(Global.LogDirectory.FullName);
        }
        if (Global.LatestLogFile.Exists)
        {
            var firstLine = File.ReadLines(Global.LatestLogFile.FullName).FirstOrDefault();
            var timestamp = ExtractTimestampFromLog(firstLine) ?? DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var archivedLogFile = Path.Combine(Global.LogDirectory.FullName, $"log_{timestamp}.log");
            File.Move(Global.LatestLogFile.FullName, archivedLogFile);
        }
    }

    public static void AddLog(string message, LogLevel level = LogLevel.Info)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var logEntry = $"[{timestamp}] [{level}] {message}";

        lock (LockObject)
        {
            File.AppendAllText(Global.LatestLogFile.FullName, logEntry + Environment.NewLine);
        }
        UpdateLogContent(logEntry);
    }

    private static void UpdateLogContent(string logEntry) => GlobalLogViewModel._logContent += logEntry + Environment.NewLine;

    private static string? ExtractTimestampFromLog(string? logLine)
    {
        if (string.IsNullOrWhiteSpace(logLine))
        {
            return null;
        }
        var match = Regex.Match(logLine, @"\[(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})\]");
        if (match.Success)
        {
            if (DateTime.TryParse(match.Groups[1].Value, out var dateTime))
            {
                return dateTime.ToString("yyyyMMdd_HHmmss");
            }
        }
        return null;
    }
}