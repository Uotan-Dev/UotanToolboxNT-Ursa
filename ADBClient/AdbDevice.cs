namespace ADBClient;
public class AdbDevice(string id, string product, string model, string device)
{
    public string SerialNumber { get; private set; } = id;
    public string Product { get; private set; } = product;
    public string Model { get; private set; } = model;
    public string Device { get; private set; } = device;

    public override string ToString()
    {
        return $"serialNumber:{SerialNumber}, product:{Product}, model:{Model}, device:{Device}";
    }
}
