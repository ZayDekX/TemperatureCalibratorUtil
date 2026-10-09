using Microsoft.Extensions.Hosting;
using TemperatureCalibratorUtil.Configuration;

namespace TemperatureCalibratorUtil.Modbus;

/// <summary>
/// Represents remote modbus device
/// </summary>
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

    public ReadOnlySpan<ushort> Inputs => _values;

    public event Action? Connected;
    public event Action? Disconnected;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _connection.Connect(_config.Host, _config.Port);

        if (!_connection.Connected)
        {
            return;
        }

        Connected?.Invoke();

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
                    _connection.Master.ReadInputRegisters(deviceId, address, 1).CopyTo(valueSpan[pos..]);
                    pos++;
                }

                OnRead();
            }
        }
        catch (Exception ex)
        {
            if(_connection.Connected)
            {
                return;
            }
            Disconnected?.Invoke();
        }
    }

    private void OnRead()
    {
        Read?.Invoke();
    }

    public void Write(ushort[] values)
    {
        _connection.Master?.WriteMultipleRegisters(5, 0, values);
    }

    public event Action? Read; // in production-grade app there would be much more suitable something like "Weak Reference Event" with lazy init

    public override void Dispose()
    {
        _connection.Dispose();
    }
}