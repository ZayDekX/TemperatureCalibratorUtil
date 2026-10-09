namespace TemperatureCalibratorUtil.Configuration;

public class GraphConfig
{
    public string Name { get; set; }
    public int ParameterId { get; set; }
    public int ErrorId { get; set; }
}

public class ParameterConfig
{
    public int Device { get; set; }
    public int Address { get; set; }
    public string Name { get; set; }
    public double Multiplier { get; set; }
}