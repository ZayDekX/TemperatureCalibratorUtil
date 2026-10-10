namespace TemperatureCalibratorUtil.Configuration;

/// <summary>
/// Configuration of parameter transferred via Modbus
/// </summary>
public class ParameterConfig
{
    /// <summary>
    /// Id of device on Modbus master
    /// </summary>
    public int Device { get; set; }

    /// <summary>
    /// Cell on device
    /// </summary>
    public int Address { get; set; }

    /// <summary>
    /// Name of parameter
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Multiplier used as encoding
    /// </summary>
    public double Multiplier { get; set; }
}