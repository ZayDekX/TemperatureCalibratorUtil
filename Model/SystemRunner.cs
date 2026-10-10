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

        _parameterConfig = config.Parameters;
        _systemConfig = config.System;
    }

    private long _lastStamp;
    private readonly DeviceParameterConfig _parameterConfig;
    private readonly SystemConfig _systemConfig;

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

        Device.Write([(ushort)(System.TargetHeaterTemperature * _parameterConfig.Output[_systemConfig.TargetHeaterTemperatureParameterId].Multiplier), 1]);
    }

    /// <summary>
    /// Starts system
    /// </summary>
    public void Start()
    {
        var startSource = new CancellationTokenSource();
        Device.StartAsync(startSource.Token);
        Started?.Invoke();
    }

    /// <summary>
    /// Stops system
    /// </summary>
    public void Stop()
    {
        var stopSource = new CancellationTokenSource();
        Device.Write([0, 0]);
        Device.StopAsync(stopSource.Token).Wait();
        Stopped?.Invoke();
    }
}
