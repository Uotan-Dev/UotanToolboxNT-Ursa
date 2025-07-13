namespace ADBClient;
public enum AdbAppType
{
    Unknown = 0,
    System = 1,
    Privileged = 2,
    ThirdParty = 3
}

public enum AdbAppLocation
{
    InternalMemory = 0,
    ExternalMemory = 1
}

public class AdbAppInfo(string name, string fileName, AdbAppType type, AdbAppLocation location)
{
    public string Name { get; private set; } = name;
    public string FileName { get; private set; } = fileName;
    public AdbAppType Type { get; private set; } = type;
    public AdbAppLocation Location { get; private set; } = location;
}
