namespace TemperatureCalibratorUtil.Configuration;

/// <summary>
/// Configuration of parameter transferred via Modbus
/// </summary>
public class ParameterConfig
{
    /// <summary>
    /// Id of device on Modbus master
    /// </summary>
    public byte Device { get; set; }

    /// <summary>
    /// Cell on device
    /// </summary>
    public ushort Address { get; set; }

    /// <summary>
    /// Name of parameter
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Multiplier used as encoding
    /// </summary>
    public double Multiplier { get; set; }

    /// <summary>
    /// Min allowed value
    /// </summary>
    public double Min { get; set; }

    /// <summary>
    /// Max allowed value
    /// </summary>
    public double Max { get; set; }

    /// <summary>
    /// Value that will trigger alert for value
    /// </summary>
    public double? AlertLevel { get; set; }
}