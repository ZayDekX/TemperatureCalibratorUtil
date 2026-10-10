using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using TemperatureCalibratorUtil.Configuration;
using TemperatureCalibratorUtil.Modbus;

namespace TemperatureCalibratorUtil.Model;

public class SystemRunner : BackgroundService
{
    public SystemRunner(Config config)
    {
        UpdatePeriod = config.System.UpdatePeriod;

        Device = new(config.Device, config.Parameters);
        System = new(config.System, config.Parameters);

        _targetHeaterTempParameter = config.Parameters.Output[config.System.TargetHeaterTemperatureParameterId];
        _heaterStateParameter = config.Parameters.Output[config.System.HeaterStateParameterId];
    }

    private readonly ParameterConfig _targetHeaterTempParameter;
    private readonly ParameterConfig _heaterStateParameter;

    private long _lastStamp;
    private TimeSpan _updatePeriod;

    /// <summary>
    /// Update period in milliseconds
    /// </summary>
    public int UpdatePeriod
    {
        get => (int)_updatePeriod.TotalMilliseconds;
        set => _updatePeriod = TimeSpan.FromMilliseconds(value);
    }

    /// <summary>
    /// Current state of system
    /// </summary>
    public SystemModel System { get; set; }

    /// <summary>
    /// Remote system
    /// </summary>
    public ModbusDevice Device { get; set; }

    public event Action? Started;
    public event Action? Stopped;
    public event Action? Updated;

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        EnableHeater();
        Started?.Invoke();
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        DisableHeater();
        Stopped?.Invoke();
        return base.StopAsync(cancellationToken);
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_updatePeriod);
        _lastStamp = Stopwatch.GetTimestamp();
        
        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            Device.Read();
            var stamp = Stopwatch.GetTimestamp();

            var dt = Stopwatch.GetElapsedTime(_lastStamp, stamp).TotalSeconds;

            System.Update(Device.InputBuffer, dt);
            _lastStamp = stamp;

            Device.Write(_targetHeaterTempParameter, (ushort)(System.TargetHeaterTemperature * _targetHeaterTempParameter.Multiplier));

            timer.Period = _updatePeriod;

            Updated?.Invoke();
        }
    }

    public void EnableHeater()
    {
        Device.Write(_heaterStateParameter, 1);
    }

    public void DisableHeater()
    {
        Device.Write(_heaterStateParameter, 0);
    }

}
