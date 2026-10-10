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
        new() {Device = 1, Address = 0, Multiplier = 10, Name = "T_system",      Min = 15,  Max = 150, AlertLevel = 160 },
        new() {Device = 1, Address = 1, Multiplier = 1,  Name = "T_system_err",  Min = 0,   Max = 1},
        new() {Device = 2, Address = 0, Multiplier = 10, Name = "T_heater",      Min = 20,  Max = 450, AlertLevel = 470 },
        new() {Device = 2, Address = 1, Multiplier = 1,  Name = "T_heater_err",  Min = 0,   Max = 0},
        new() {Device = 3, Address = 0, Multiplier = 10, Name = "T_env",         Min = -30, Max = 60},
        new() {Device = 3, Address = 1, Multiplier = 1,  Name = "T_env_err",     Min = 0,   Max = 0},
        new() {Device = 4, Address = 0, Multiplier = 10, Name = "P",             Min = 80,  Max = 120},
        new() {Device = 4, Address = 1, Multiplier = 1,  Name = "P_error",       Min = 0,   Max = 0},
    ];

    /// <summary>
    /// Parameters that are written to master
    /// </summary>
    public List<ParameterConfig> Output { get; set; } = [
        new() {Device = 5, Address = 0, Multiplier = 10, Name = "T_heater_target", Min = 0, Max = 450},
        new() {Device = 5, Address = 1, Multiplier = 1,  Name = "heater_on",       Min = 0, Max = 1},
    ];
}