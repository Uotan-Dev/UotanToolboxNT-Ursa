namespace UotanToolboxNT_Ursa.Models;

public record LogUpdateMessage(string LogContent);
public record HardwareInfoUpdateMessage(int CardIndex, string Info);
