using System.Globalization;
using System.IO;
using TemperatureCalibratorUtil.Configuration;

namespace TemperatureCalibratorUtil.Logging;

public class CsvLogger
{
    private readonly CsvLoggerConfig _config;

    public CsvLogger(CsvLoggerConfig config)
    {
        _config = config;
    }

    public void Write(DateTime timestamp, SystemState state)
    {
        if(!_config.Enabled)
        {
            return;
        }

        if (!File.Exists(_config.FileName))
        {
            File.Create(_config.FileName);
        }

        // AppendAllText is kinda slow, so writing into stream will be preferred
        // calling ToString is also not good for GC

        File.AppendAllText(_config.FileName, string.Join(_config.Separator, 
        [
            timestamp, 
            state.SystemTemperature.ToString(NumberFormatInfo.InvariantInfo),
            state.HeaterTemperature.ToString(NumberFormatInfo.InvariantInfo),
            state.Pressure.ToString(NumberFormatInfo.InvariantInfo),
            state.AmbientTemperature.ToString(NumberFormatInfo.InvariantInfo)
        ]));
        File.AppendAllText(_config.FileName, Environment.NewLine);
    }
}
