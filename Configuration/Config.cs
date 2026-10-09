namespace TemperatureCalibratorUtil.Configuration;

public class Config
{
    public DeviceConfig Device { get; set; } = new();
    public List<GraphConfig> Graphs { get; set; } = [
        new() {Name = "T_system", ParameterId = 0},
        new() {Name = "T_heater", ParameterId = 2},
    ];
    public SystemConfig System { get; set; } = new();
    public DeviceParameterConfig Parameters { get; set; } = new();
}

public class SystemConfig
{
    public PIDControllerConfig PidController { get; set; } = new();
    public double TargetTemperature { get; set; } = 50;

    // inputs

    public int SystemTemperatureParameterId { get; set; } = 0;
    public int HeaterTemperatureParameterId { get; set; } = 2;
    public int EnvironmentTemperatureParameterId { get; set; } = 4;
    public int PressureParameterId { get; set; } = 6;

    // outputs

    public int TargetHeaterTemperatureParameterId { get; set; } = 0;
    public int HeaterStateParameterId { get; set; } = 1;
}
