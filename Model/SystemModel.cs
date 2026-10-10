using TemperatureCalibratorUtil.Configuration;

namespace TemperatureCalibratorUtil.Model;

public class SystemModel
{
    private readonly int[] _index;
    private readonly SystemConfig _config;
    private readonly PIDController _controller;
    private readonly DeviceParameterConfig _parameters;

    public SystemModel(SystemConfig config, DeviceParameterConfig parameters)
    {
        _config = config;
        _controller = new(_config.PidController);
        _parameters = parameters;

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

    /// <summary>
    /// Target temperature that system should reach
    /// </summary>
    public double TargetTemperature => _config.TargetTemperature;

    /// <summary>
    /// Target heater temperature to be applied
    /// </summary>
    public double TargetHeaterTemperature { get; set; }

    /// <summary>
    /// Determines whether the system temperature has reached <see cref="TargetTemperature"/> and stabilized on that value
    /// </summary>
    public bool Stable
    {
        get; 
        set
        {
            if(field == value)
            {
                return;
            }

            field = value;
            if(value)
            {
                Stabilized?.Invoke();
            }
            else
            {
                Destabilized?.Invoke();
            }
        }
    }

    public event Action? Stabilized;
    public event Action? Destabilized;

    private uint _stableFrames;

    public void Update(ReadOnlySpan<ushort> inputs, double dt)
    {
        var tSystem = GetValue(inputs, _config.SystemTemperatureParameterId);

        TargetHeaterTemperature = _controller.Compute(TargetTemperature, tSystem, dt);

        if (Math.Abs(TargetTemperature - tSystem) <= _config.StabilityTolerance)
        {
            unchecked { _stableFrames++; }
        }
        else
        {
            Stable = false;
            _stableFrames = 0;
        }

        if (_stableFrames >= _config.MinStabilityFrameCount)
        {
            Stable = true;
        }
    }

    private double GetValue(ReadOnlySpan<ushort> inputs, int id)
    {
        return inputs[_index[id]] / _parameters.Input[id].Multiplier;
    }
}
