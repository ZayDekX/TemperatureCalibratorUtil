namespace TemperatureCalibratorUtil;

public class SystemModel
{
    private int[] _index;
    private SystemConfig _config;
    private PIDController _controller;
    private DeviceParameterConfig _parameters;

    public SystemModel(SystemConfig processorConfig, DeviceParameterConfig parameters)
    {
        _config = processorConfig;
        _controller = new(_config.PidController);
        _parameters = parameters;

        TargetTemperature = _config.TargetTemperature;

        // order and index parameters
        var rawIndex = parameters.Input
            .Select((x, i) => (id: i, config: x))
            .GroupBy(x => x.config.Device)
            .SelectMany(x => x.OrderBy(p => p.config.Address).Select(p => p.id))
            .ToArray();

        // save ids of parameters in fetcher buffer
        _index = new int[rawIndex.Length];

        var i = 0;
        foreach (var idx in rawIndex)
        {
            _index[idx] = i;
            i++;
        }
    }

    public double TargetTemperature { get; set; }
    public double TargetHeaterTemperature { get; set; }

    public void Update(ReadOnlySpan<ushort> inputs, double dt)
    {
        var tSystem = GetValue(inputs, _config.SystemTemperatureParameterId);

        TargetHeaterTemperature = _controller.Compute(TargetTemperature, tSystem, dt);
    }

    private double GetValue(ReadOnlySpan<ushort> inputs, int id)
    {
        return inputs[_index[id]] / _parameters.Input[id].Multiplier;
    }
}
