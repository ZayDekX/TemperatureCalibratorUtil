namespace TemperatureCalibratorUtil.Configuration;

public class Config
{
    /// <summary>
    /// Modbus connection settings
    /// </summary>
    public DeviceConfig Device { get; set; } = new();

    /// <summary>
    /// Chart configuration
    /// </summary>
    public List<ChartConfig> Charts { get; set; } = [
        new() {Name = "T_system", ParameterId = 0, ErrorId = 1, PointCount = 250, SeparatorCount = 5},
        new() {Name = "T_heater", ParameterId = 2, ErrorId = 3, PointCount = 250, SeparatorCount = 5},
        new() {Name = "T_env", ParameterId = 4, ErrorId = 5, PointCount = 250, SeparatorCount = 5},
        new() {Name = "P", ParameterId = 6, ErrorId = 7, PointCount = 250, SeparatorCount = 5}
    ];

    /// <summary>
    /// Model configuration
    /// </summary>
    public SystemConfig System { get; set; } = new();

    /// <summary>
    /// Mosbus parameters
    /// </summary>
    public DeviceParameterConfig Parameters { get; set; } = new();
}