namespace TemperatureCalibratorUtil.Configuration;

public class DeviceConfig
{
    public ushort Port { get; set; } = 21316;
    public string Host { get; set; } = "127.0.0.1";
    public int Period { get; set; } = 50;
}