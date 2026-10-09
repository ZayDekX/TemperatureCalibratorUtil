namespace TemperatureCalibratorUtil.Configuration;

public class Config
{
    public DeviceConfig Device { get; set; } = new();
    public List<GraphConfig> Graphs { get; set; } = [
        new() {Name = "T_system", ParameterId = 0, ErrorId = 1},
        new() {Name = "T_heater", ParameterId = 2, ErrorId = 3},
        new() {Name = "T_env", ParameterId = 4, ErrorId = 5},
        new() {Name = "P", ParameterId = 6, ErrorId = 7}
    ];
    public SystemConfig System { get; set; } = new();
    public DeviceParameterConfig Parameters { get; set; } = new();
}