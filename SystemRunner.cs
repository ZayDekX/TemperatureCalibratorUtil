using System.Diagnostics;

namespace TemperatureCalibratorUtil;

public class SystemRunner
{
    public SystemModel System { get; set; }
    public ModbusDevice Device { get; set; }

    public SystemRunner(Config config)
    {
        Device = new(config.Device, config.Parameters);
        System = new(config.System, config.Parameters);

        Device.Read += OnRead;

        _parameterConfig = config.Parameters;
        _systemConfig = config.System;
    }

    private long _lastStamp;
    private DeviceParameterConfig _parameterConfig;
    private SystemConfig _systemConfig;

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

    public void Start()
    {
        var startSource = new CancellationTokenSource();
        Device.StartAsync(startSource.Token);
    }

    public void Stop()
    {
        var stopSource = new CancellationTokenSource();
        Device.StopAsync(stopSource.Token);
    }
}
