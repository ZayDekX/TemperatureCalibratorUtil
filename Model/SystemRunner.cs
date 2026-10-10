using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using TemperatureCalibratorUtil.Configuration;
using TemperatureCalibratorUtil.Logging;
using TemperatureCalibratorUtil.Modbus;

namespace TemperatureCalibratorUtil.Model;

public class SystemRunner : BackgroundService
{
    public SystemRunner(Config config)
    {
        UpdatePeriod = config.System.UpdatePeriod;

        var logger = new CsvLogger(config.Logging);

        Processor = new(config.Parameters);
        Device = new(config.Device, config.Parameters);
        System = new(Processor, logger, config.System, config.Parameters);

        _targetHeaterTempParameterId = config.System.TargetHeaterTemperatureParameterId;
        _heaterStateParameterId = config.System.HeaterStateParameterId;

        Device.Disconnected += OnDisconnected;
    }

    private void OnDisconnected()
    {
        StopAsync(CancellationToken.None);
    }

    private readonly int _targetHeaterTempParameterId;
    private readonly int _heaterStateParameterId;

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
    public ValueProcessor Processor { get; }

    /// <summary>
    /// Determines whether the system is currently running
    /// </summary>
    public bool IsRunning { get; private set; }

    public event Action? Started;
    public event Action? Stopped;
    public event Action? Updated;

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        EnableHeater();
        IsRunning = true;
        Started?.Invoke();
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        DisableHeater();
        IsRunning = false;
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

            Span<ushort> output = [0];

            Processor.WriteOutputValue(Device.OutputBuffer, _targetHeaterTempParameterId, System.TargetHeaterTemperature);
            Device.Write();

            timer.Period = _updatePeriod;

            Updated?.Invoke();
        }
    }

    public void EnableHeater()
    {
        Processor.WriteOutputValue(Device.OutputBuffer, _targetHeaterTempParameterId, System.TargetHeaterTemperature);
        Processor.WriteOutputValue(Device.OutputBuffer, _heaterStateParameterId, 1);

        Device.Write();
    }

    public void DisableHeater()
    {
        Processor.WriteOutputValue(Device.OutputBuffer, _targetHeaterTempParameterId, System.TargetHeaterTemperature);
        Processor.WriteOutputValue(Device.OutputBuffer, _heaterStateParameterId, 0);

        Device.Write();
    }
}