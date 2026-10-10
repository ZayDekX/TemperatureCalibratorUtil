using TemperatureCalibratorUtil.Configuration;

namespace TemperatureCalibratorUtil.Model;

/// <summary>
/// Processes values from IO buffers
/// </summary>
public class ValueProcessor
{
    public ValueProcessor(DeviceParameterConfig parameters)
    {
        Parameters = parameters;
    }

    public DeviceParameterConfig Parameters { get; }

    /// <summary>
    /// Reads requested value from provided input buffer
    /// </summary>
    /// <param name="values">Input buffer</param>
    /// <param name="id">Id of parameter</param>
    /// <returns>Tuple with value and flag that specifies whether alert level is reached</returns>
    public (double value, bool alert) ReadInputValue(ReadOnlySpan<ushort> values, int id)
    {
        var parameter = Parameters.Input[id];

        var multiplier = parameter.Multiplier == 0 ? 1 : parameter.Multiplier;

        var value = values[id] / multiplier;

        if (parameter.AlertLevel is not null)
        {
            return (value, value > parameter.AlertLevel);
        }

        return (value, false);
    }

    /// <summary>
    /// Writes value to provided output buffer
    /// </summary>
    /// <param name="values">Output buffer</param>
    /// <param name="id">Id of parameter</param>
    /// <param name="value">Value to write</param>
    public void WriteOutputValue(Span<ushort> values, int id, double value)
    {
        var parameter = Parameters.Output[id];

        var multiplier = parameter.Multiplier == 0 ? 1 : parameter.Multiplier;

        var outputValue = (ushort)(short)(Math.Clamp(value, parameter.Min, parameter.Max) * multiplier);

        values[id] = outputValue;
    }
}