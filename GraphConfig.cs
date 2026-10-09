namespace TemperatureCalibratorUtil;

public class GraphConfig
{
    public int ParameterId { get; set; }
    public string Name { get; set; }
}

public class ParameterConfig
{
    public int Device { get; set; }
    public int Address { get; set; }
    public string Name { get; set; }
    public double Multiplier { get; set; }
}