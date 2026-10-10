namespace TemperatureCalibratorUtil.Configuration;

public class SystemConfig
{
    /// <summary>
    /// PID controller settings
    /// </summary>
    public PIDControllerConfig PidController { get; set; } = new();

    /// <summary>
    /// Default target system temperature
    /// </summary>
    public double TargetTemperature { get; set; } = 50;

    /// <summary>
    /// System update period in milliseconds
    /// </summary>
    public int UpdatePeriod { get; set; } = 500;

    /// <summary>
    /// Minimal count of frames that should have same value (within <see cref="StabilityTolerance"/>) to treat system as stable
    /// </summary>
    public int MinStabilityFrameCount { get; set; } = 10;

    /// <summary>
    /// Tolerance of system stability. System temperature must differ from Target system temperature by that amount or less
    /// </summary>
    public double StabilityTolerance { get; set; } = 0.01;

    // inputs

    /// <summary>
    /// Id of System temperature input parameter
    /// </summary>
    public int SystemTemperatureParameterId { get; set; } = 0;

    /// <summary>
    /// Id of heater temperature input parameter
    /// </summary>
    public int HeaterTemperatureParameterId { get; set; } = 2;

    /// <summary>
    /// Id of environment temperature input parameter
    /// </summary>
    public int EnvironmentTemperatureParameterId { get; set; } = 4;

    /// <summary>
    /// Id of pressure input parameter
    /// </summary>
    public int PressureParameterId { get; set; } = 6;

    // outputs

    /// <summary>
    /// Id of target heater temperature output parameter
    /// </summary>
    public int TargetHeaterTemperatureParameterId { get; set; } = 0;

    /// <summary>
    /// Id of heater state output parameter
    /// </summary>
    public int HeaterStateParameterId { get; set; } = 1;
}
