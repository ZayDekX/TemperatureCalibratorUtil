namespace TemperatureCalibratorUtil.Configuration;

/// <summary>
/// Configuration of chart
/// </summary>
public class ChartConfig
{
    /// <summary>
    /// Displayed name of chart
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Id of parameter to display
    /// </summary>
    public int ParameterId { get; set; }

    /// <summary>
    /// Id of error parameter that indicates error
    /// </summary>
    public int ErrorId { get; set; }

    /// <summary>
    /// Count of separators to display
    /// </summary>
    public int SeparatorCount { get; set; }

    /// <summary>
    /// Count of points displayed in chart
    /// </summary>
    public int PointCount { get; set; }
}