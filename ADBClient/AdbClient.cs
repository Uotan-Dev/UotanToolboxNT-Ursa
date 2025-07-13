
using Microsoft.Win32;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ADBClient;
public class AdbClient
{
    public string ServerHost { get; private set; }
    public int ServerPort { get; private set; }

    public string DeviceSerialNumber { get; private set; }
    private static readonly char[] Separator = ['\r', '\n'];
    private static readonly char[] SeparatorArray = [' '];

    public AdbClient(int adbServerPort = -1, string adbServerHost = null)
    {
        if (adbServerPort < 0)
        {
            var serverPortString = Environment.GetEnvironmentVariable("ANDROID_ADB_SERVER_PORT");
            ServerPort = !string.IsNullOrEmpty(serverPortString) && int.TryParse(serverPortString, out var serverPort) ? serverPort : 5037;
        }
        else
        {
            ServerPort = adbServerPort;
        }

        ServerHost = string.IsNullOrEmpty(adbServerHost) ? "127.0.0.1" : adbServerHost;
    }

    public void StartServer()
    {
        var path = Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Android SDK Tools", "Path", null) as string;
        if (string.IsNullOrEmpty(path))
        {
            path = Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Wow6432Node\Android SDK Tools", "Path", null) as string;
        }
        if (string.IsNullOrEmpty(path))
        {
            throw new AdbException("Cannot find Android SDK tools");
        }

        path = Path.Combine(path, @"platform-tools\adb.exe");

        var processStartInfo = new ProcessStartInfo(path, "start-server")
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = Process.Start(processStartInfo);
        process.WaitForExit();
    }

    public void StopServer()
    {
        using var adbSocket = GetSocket();
        adbSocket.SendCommand("host:kill");
    }

    public int GetServerVersion()
    {
        using var adbSocket = GetSocket();
        adbSocket.SendCommand("host:version");
        return adbSocket.ReadInt32Hex();
    }

    public AdbDevice[] GetDevices()
    {
        var response = "";
        using (var adbSocket = GetSocket())
        {
            adbSocket.SendCommand("host:devices-l");
            response = adbSocket.ReadHexString();
        }

        var lines = response.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

        var devices = new List<AdbDevice>(lines.Length);

        // "b3819a41               device product:a3ultexx model:SM_A300FU device:a3ulte"
        foreach (var line in lines)
        {
            var parts = line.Split(SeparatorArray, StringSplitOptions.RemoveEmptyEntries);

            var product = "";
            var model = "";
            var device = "";

            for (var i = 2; i < parts.Length; i++)
            {
                var halves = parts[i].Split([':'], StringSplitOptions.RemoveEmptyEntries);
                if (2 == halves.Length)
                {
                    switch (halves[0])
                    {
                        case "product":
                            product = halves[1];
                            break;
                        case "model":
                            model = halves[1];
                            break;
                        case "device":
                            device = halves[1];
                            break;
                        default:
                            break;
                    }
                }
            }

            devices.Add(new AdbDevice(parts[0], product, model, device));
        }

        return [.. devices];
    }

    public void SetDevice(string serialNumber)
    {
        DeviceSerialNumber = serialNumber;
    }

    public Dictionary<string, string> GetDeviceProperties()
    {
        var props = new Dictionary<string, string>();

        var lines = ExecuteRemoteCommand("/system/bin/getprop");

        foreach (var line in lines)
        {
            // "[ro.product.model]: [SM-A300FU]"
            var match = Regex.Match(line, @"\[(.*)]: \[(.*)]");
            if (!match.Success || (match.Groups.Count != 3))
            {
                Tracer.Trace($"Invalid prop line: '{line}'");
            }
            else
            {
                props.Add(match.Groups[1].Value, match.Groups[2].Value);
            }
        }

        return props;
    }

    #region shell:

    public string[] ExecuteRemoteCommand(string command)
    {
        using var adbSocket = GetSocket();
        SetDevice(adbSocket);

        Tracer.Trace($"Executing '{command}' remote command");
        adbSocket.SendCommand($"shell:{command}");

        var response = adbSocket.ReadAllLines();
        Tracer.Trace($"{response.Length} response lines read");
        return response;
    }

    #endregion

    #region sync:

    public AdbFileInfo GetFileInfo(string fileName)
    {
        using var adbSocket = GetSocket();
        var response = SendSyncCommand(adbSocket, "STAT", fileName);

        return !response.Equals("STAT") ? throw new AdbInvalidResponseException(response) : GetFileInfo(adbSocket, fileName, null);
    }

    public AdbFileInfo[] GetDirectoryListing(string directoryName)
    {
        using var adbSocket = GetSocket();
        var response = SendSyncCommand(adbSocket, "LIST", directoryName);

        var fileInfos = new List<AdbFileInfo>();

        var realDirectory = false;
        while (true)
        {
            if (response.Equals("DONE"))
            {
                if (!realDirectory)
                {
                    throw new DirectoryNotFoundException();
                }
                else
                {
                    break;
                }
            }
            else if (!response.Equals("DENT"))
            {
                throw new AdbInvalidResponseException(response);
            }

            var adbFileInfo = GetFileInfo(adbSocket, null, directoryName);
            if (null == adbFileInfo)
            {
                realDirectory = true; // has "." and ".."
            }
            else
            {
                fileInfos.Add(adbFileInfo);
            }

            response = adbSocket.ReadString(4);
        }

        return [.. fileInfos];
    }

    private AdbFileInfo GetFileInfo(AdbSocket adbSocket, string fullName, string directoryName)
    {
        var mode = adbSocket.ReadInt32();
        var size = adbSocket.ReadInt32();
        var time = AdbHelpers.FromUnixTime(adbSocket.ReadInt32());

        string? name;
        if (string.IsNullOrEmpty(fullName))
        {
            name = adbSocket.ReadSyncString();
            if (name.Equals(".") || name.Equals(".."))
            {
                return null;
            }
            fullName = AdbHelpers.CombinePath(directoryName, name);
        }
        else
        {
            name = Path.GetFileName(fullName);
        }

        return new AdbFileInfo(fullName, name, size, mode, time);
    }

    public void DownloadFile(string remoteFileName, string localFileName)
    {
        using var adbSocket = GetSocket();
        var response = SendSyncCommand(adbSocket, "RECV", remoteFileName);

        var total = 0;
        using var stream = File.Open(localFileName, FileMode.Create);
        var bytes = new byte[65536];
        while (true)
        {
            if (response.Equals("DONE"))
            {
                break;
            }
            else if (!response.Equals("DATA"))
            {
                throw new AdbInvalidResponseException(response);
            }

            var size = adbSocket.ReadInt32();

            adbSocket.Read(bytes, size);
            stream.Write(bytes, 0, size);
            total += size;

            response = adbSocket.ReadString(4);
        }
    }

    public void UploadFile(string localFileName, string remoteFileName, int remoteFilePermissions = 0x0666)
    {
        var adbFileInfo = GetFileInfo(remoteFileName);
        if (adbFileInfo.IsDirectory || adbFileInfo.IsSymbolicLink)
        {
            remoteFileName = AdbHelpers.CombinePath(remoteFileName, Path.GetFileName(localFileName));
        }

        using var adbSocket = GetSocket();
        var response = SendSyncCommand(adbSocket, "SEND", $"{remoteFileName},{remoteFilePermissions}", false);

        var localFileInfo = new FileInfo(localFileName);
        var left = (int)localFileInfo.Length;

        using (var stream = File.OpenRead(localFileName))
        {
            var bytes = new byte[65536];

            while (left > 0)
            {
                var size = left < bytes.Length ? left : bytes.Length;
                stream.ReadExactly(bytes, 0, size);

                adbSocket.WriteString("DATA");
                adbSocket.WriteInt32(size);
                adbSocket.Write(bytes, size);

                left -= size;
            }
        }

        adbSocket.WriteString("DONE");
        adbSocket.WriteInt32(AdbHelpers.ToUnixTime(localFileInfo.LastWriteTime));

        response = adbSocket.ReadString(4);

        if (!response.Equals("OKAY"))
        {
            throw new AdbInvalidResponseException(response);
        }
    }

    private string SendSyncCommand(AdbSocket adbSocket, string command, string parameter, bool readResponse = true)
    {
        if (null == parameter)
        {
            throw new ArgumentNullException(nameof(parameter));
        }

        SetDevice(adbSocket);

        adbSocket.SendCommand("sync:");

        return adbSocket.SendSyncCommand(command, parameter, readResponse);
    }

    #endregion

    #region File system

    public void DeleteFile(string remoteFileName)
    {
        _ = ExecuteRemoteCommand($"rm -f {remoteFileName}");
    }

    #endregion

    #region Applications

    public void InstallApplication(string localFileName, bool installOnSdCard)
    {
        // legacy install

        var baseName = Path.GetFileName(localFileName);
        var remoteFileName = installOnSdCard ? $"/sdcard/tmp/{baseName}" : $"/data/local/tmp/{baseName}";
        var options = installOnSdCard ? "-s" : "";

        if (GetFileInfo(remoteFileName).IsFile)
        {
            DeleteFile(remoteFileName);
        }

        UploadFile(localFileName, remoteFileName);

        ExecutePm($"install {options} {remoteFileName}");

        DeleteFile(remoteFileName);
    }

    public void UninstallApplication(string applicationName, bool keepDataAndCache = false)
    {
        // legacy uninstall

        var options = keepDataAndCache ? "-k" : "";

        ExecutePm($"uninstall {options} {applicationName}");
    }

    public AdbAppInfo[] GetInstalledApplications()
    {
        var response = ExecuteRemoteCommand($"pm list packages -f");

        var apps = new List<AdbAppInfo>();

        foreach (var line in response)
        {
            var match = Regex.Match(line, @"package:(.+?)=(.+)");
            if (match.Groups.Count != 3)
            {
                throw new Exception(line);
            }

            var fileName = match.Groups[1].Value;
            var type = fileName.StartsWith("/system/app/") ? AdbAppType.System : (fileName.StartsWith("/system/priv-app/") ? AdbAppType.Privileged : AdbAppType.ThirdParty);
            var location = fileName.StartsWith("/system/") || fileName.StartsWith("/data/") ? AdbAppLocation.InternalMemory : AdbAppLocation.ExternalMemory;

            var app = new AdbAppInfo(match.Groups[2].Value, fileName, type, location);
            apps.Add(app);
        }

        return [.. apps];
    }

    private void ExecutePm(string commandLine)
    {
        var response = ExecuteRemoteCommand($"pm {commandLine}");

        if ((null == response) || (0 == response.Length))
        {
            throw new Exception("Wrong pm output");
        }

        var line = response[^1];
        if (line.Equals("Success"))
        {
            return;
        }

        var match = Regex.Match(line, @"\[(.+?)]");
        throw new Exception(2 == match.Groups.Count ? match.Groups[1].Value : line);
    }

    #endregion

    private void SetDevice(AdbSocket adbSocket)
    {
        if (string.IsNullOrEmpty(DeviceSerialNumber))
        {
            throw new AdbException("No device selected");
        }

        adbSocket.SendCommand($"host:transport:{DeviceSerialNumber}");
    }

    private AdbSocket GetSocket()
    {
        return new AdbSocket(ServerHost, ServerPort);
    }
}
