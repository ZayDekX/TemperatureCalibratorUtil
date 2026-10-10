namespace TemperatureCalibratorUtil.Configuration;

/// <summary>
/// Configuration of Modbus device parameters
/// </summary>
public class DeviceParameterConfig
{
    /// <summary>
    /// Parameters that are read from master
    /// </summary>
    public List<ParameterConfig> Input { get; set; } = [
        new() {Device = 1, Address = 0, Multiplier = 10, Name = "T_system"},
        new() {Device = 1, Address = 1, Multiplier = 1, Name = "T_system_err"},
        new() {Device = 2, Address = 0, Multiplier = 10, Name = "T_heater"},
        new() {Device = 2, Address = 1, Multiplier = 1, Name = "T_heater_err"},
        new() {Device = 3, Address = 0, Multiplier = 10, Name = "T_env"},
        new() {Device = 3, Address = 1, Multiplier = 1, Name = "T_env_err"},
        new() {Device = 4, Address = 0, Multiplier = 10, Name = "P"},
        new() {Device = 4, Address = 1, Multiplier = 1, Name = "P_error"},
    ];

    /// <summary>
    /// Parameters that are written to master
    /// </summary>
    public List<ParameterConfig> Output { get; set; } = [
        new() {Device = 4, Address = 1, Multiplier = 10, Name = "T_heater_target"},
        new() {Device = 4, Address = 2, Multiplier = 1, Name = "heater_on"},
    ];
}