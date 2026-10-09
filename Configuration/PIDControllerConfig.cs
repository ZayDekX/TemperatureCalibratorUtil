namespace TemperatureCalibratorUtil.Configuration;

public class PIDControllerConfig
{
    public double Kp {get; set;} = 6;
    public double Ki {get; set;} = 0.125;
    public double Kd {get; set;} = 0.8;
    public double DFilterCoeff { get; set; } = 0.25;
    public double MinOutput {get; set;} = 0;
    public double MaxOutput {get; set;} = 470;
}