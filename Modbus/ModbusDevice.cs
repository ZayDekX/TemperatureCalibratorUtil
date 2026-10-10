using Microsoft.Extensions.Hosting;
using TemperatureCalibratorUtil.Configuration;

namespace TemperatureCalibratorUtil.Modbus;

/// <summary>
/// Represents remote modbus device
/// </summary>
/// <remarks>
/// When active performs automatic polling of connected device
/// </remarks>
public class ModbusDevice : BackgroundService
{
    public ModbusDevice(DeviceConfig config, DeviceParameterConfig parameters)
    {
        _config = config;

        // order by device and address
        _inputInfo = [.. parameters.Input
            .GroupBy(x => x.Device)
            .OrderBy(x => x.Key)
            .SelectMany(x => x.OrderBy(p => p.Address))
            .Select(x => ((byte)x.Device, (ushort)x.Address))
        ];

        _values = new ushort[_inputInfo.Length];
    }

    private (byte deviceId, ushort address)[] _inputInfo;

    private DeviceConfig _config;

    private ushort[] _values = new ushort[8];

    private ModbusConnection _connection = new();

    /// <summary>
    /// Input buffer with read values
    /// </summary>
    public ReadOnlySpan<ushort> Inputs => _values;

    public bool IsConnected => _connection.Connected;

    /// <summary>
    /// Invoked when device have been successfully connected
    /// </summary>
    public event Action? Connected;

    /// <summary>
    /// Invoked when connection has been lost or terminated
    /// </summary>
    public event Action? Disconnected;

    /// <summary>
    /// Invoked when some error occured
    /// </summary>
    public event Action<Exception>? Error;

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        EnsureConnected();

        return base.StartAsync(cancellationToken);
    }

    private void EnsureConnected()
    {
        if (IsConnected)
        {
            return;
        }

        _connection.Connect(_config.Host, _config.Port);

        if (!IsConnected)
        {
            throw new Exception("Failed to connect to remote host");
        }

        Connected?.Invoke();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if(!IsConnected)
        {
            Disconnected?.Invoke();
            return;
        }

        var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_config.Period));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                var pos = 0;
                var valueSpan = _values.AsSpan();
                
                // straightforward implementation
                // main downside is that creates a lot of requests to remote device, which is very critical on RTU with many devices on one bus
                foreach (var (deviceId, address) in _inputInfo)
                {
                    _connection.Master!.ReadInputRegisters(deviceId, address, 1).CopyTo(valueSpan[pos..]);
                    pos++;
                }

                OnRead();
            }
        }
        catch (Exception ex)
        {
            OnError(ex);
        }
    }

    private void OnError(Exception ex)
    {
        Error?.Invoke(ex);
    }

    private void OnRead()
    {
        Read?.Invoke();
    }

    public void Write(ParameterConfig parameter, ushort value)
    {
        if(!IsConnected)
        {
            return;
        }

        try
        {
            _connection.Master?.WriteSingleRegister(parameter.Device, parameter.Address, value);
        }
        catch(Exception ex)
        {
            OnError(ex);
        }
    }

    public event Action? Read; // in production-grade app there would be much more suitable something like "Weak Reference Event" with lazy init. Applicable to all events in this app

    public override void Dispose()
    {
        _connection.Dispose();
    }
}