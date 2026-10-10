using TemperatureCalibratorUtil.Configuration;

namespace TemperatureCalibratorUtil.Model;

public class SystemModel
{
    private readonly int[] _index;
    private readonly SystemConfig _config;
    private readonly PIDController _controller;
    private readonly DeviceParameterConfig _parameters;
    private readonly ValueProcessor _processor;

    public SystemModel(SystemConfig config, DeviceParameterConfig parameters, ValueProcessor processor)
    {
        _config = config;
        _controller = new(_config.PidController);
        _parameters = parameters;
        _processor = processor;
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
    public event Action<int>? Alert;

    private uint _stableFrames;

    public void Update(ReadOnlySpan<ushort> inputs, double dt)
    {
        ValidateInputs(inputs);

        UpdateTargetHeaterTemperature(inputs, dt);
    }

    private void ValidateInputs(ReadOnlySpan<ushort> inputs)
    {
        var id = 0;
        foreach (var parameter in _parameters.Input)
        {
            var (value, alert) = _processor.ReadInputValue(inputs, id);

            if (alert)
            {
                Alert?.Invoke(id);
            }

            id++;
        }
    }

    private void UpdateTargetHeaterTemperature(ReadOnlySpan<ushort> inputs, double dt)
    {
        var (tSystem, _) = _processor.ReadInputValue(inputs, _config.SystemTemperatureParameterId);

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
}
