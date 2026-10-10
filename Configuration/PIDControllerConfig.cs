namespace TemperatureCalibratorUtil.Configuration;

public class PIDControllerConfig
{
    /// <summary>
    /// Proportional part coefficient
    /// </summary>
    public double Kp { get; set; } = 6;

    /// <summary>
    /// Integral part coefficient
    /// </summary>
    public double Ki { get; set; } = 0.125;

    /// <summary>
    /// Differential part coefficient
    /// </summary>
    public double Kd { get; set; } = 0.8;

    /// <summary>
    /// Exponential filter coefficient for differential part
    /// </summary>
    /// <remarks>Valid value range is between 0.0 and 1.0</remarks>
    public double DFilterCoeff { get; set; } = 0.25;

    /// <summary>
    /// Min evaluated output result
    /// </summary>
    public double MinOutput { get; set; } = 0;

    /// <summary>
    /// Max evaluated output result
    /// </summary>
    public double MaxOutput { get; set; } = 470;
}