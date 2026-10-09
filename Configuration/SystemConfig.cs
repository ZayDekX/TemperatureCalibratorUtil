namespace TemperatureCalibratorUtil.Configuration;

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
    public int MinStabilityFrameCount { get; set; } = 10;
    public double StabilityTolerance { get; set; } = 0.01;
}
