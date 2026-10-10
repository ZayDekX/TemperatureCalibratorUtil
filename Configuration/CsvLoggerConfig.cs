namespace TemperatureCalibratorUtil.Configuration;

public class CsvLoggerConfig
{
    /// <summary>
    /// Value separator
    /// </summary>
    /// <remarks>CSV uses comma as separator. If it's not a comma, then it's not really a CSV</remarks>
    public string Separator { get; set; } = ",";

    /// <summary>
    /// Name of file to write to
    /// </summary>
    public string FileName { get; set; } = "log.csv";

    /// <summary>
    /// Is logging enabled
    /// </summary>
    public bool Enabled { get; set; } = true;
}
