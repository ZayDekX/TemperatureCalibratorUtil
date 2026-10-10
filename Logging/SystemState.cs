namespace TemperatureCalibratorUtil.Logging;

public readonly struct SystemState(double systemTemperature, double heaterTemperature, double pressure, double ambientTemperature)
{
    public readonly double SystemTemperature = systemTemperature;
    public readonly double HeaterTemperature = heaterTemperature;
    public readonly double Pressure = pressure;
    public readonly double AmbientTemperature = ambientTemperature;
}
