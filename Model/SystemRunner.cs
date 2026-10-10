using System.Diagnostics;
using TemperatureCalibratorUtil.Configuration;
using TemperatureCalibratorUtil.Modbus;

namespace TemperatureCalibratorUtil.Model;

public class SystemRunner
{
    public SystemRunner(Config config)
    {
        Device = new(config.Device, config.Parameters);
        System = new(config.System, config.Parameters);

        Device.Read += OnRead;

        _targetHeaterTempParameter = config.Parameters.Output[config.System.TargetHeaterTemperatureParameterId];
        _heaterStateParameter = config.Parameters.Output[config.System.HeaterStateParameterId];
    }

    private long _lastStamp;
    private readonly ParameterConfig _targetHeaterTempParameter;
    private readonly ParameterConfig _heaterStateParameter;

    public SystemModel System { get; set; }
    public ModbusDevice Device { get; set; }

    public event Action? Started;
    public event Action? Stopped;

    private void OnRead()
    {
        if (_lastStamp is 0)
        {
            _lastStamp = Stopwatch.GetTimestamp();
        }

        var stamp = Stopwatch.GetTimestamp();
        System.Update(Device.Inputs, Stopwatch.GetElapsedTime(_lastStamp, stamp).TotalSeconds);
        _lastStamp = stamp;

        Device.Write(_targetHeaterTempParameter, (ushort)(System.TargetHeaterTemperature * _targetHeaterTempParameter.Multiplier));
    }

    /// <summary>
    /// Start system
    /// </summary>
    public void Start()
    {
        var startSource = new CancellationTokenSource();
        Device.StartAsync(startSource.Token);
        EnableHeater();
        Started?.Invoke();
    }

    public void EnableHeater()
    {
        Device.Write(_heaterStateParameter, 1);
    }

    public void DisableHeater()
    {
        Device.Write(_heaterStateParameter, 0);
    }

    /// <summary>
    /// Stop system
    /// </summary>
    public void Stop()
    {
        var stopSource = new CancellationTokenSource();

        DisableHeater();
        Device.StopAsync(stopSource.Token).Wait();
        Stopped?.Invoke();
    }
}
