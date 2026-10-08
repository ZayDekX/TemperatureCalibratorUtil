using Microsoft.Extensions.Hosting;
using NModbus;

namespace TemperatureCalibratorUtil;

/// <summary>
/// Fetches data from remote server
/// </summary>
public class DataFetcher(DataFetcherConfig config) : BackgroundService
{
    private DataFetcherConfig _config = config;

    private ushort[] _values = new ushort[8];

    public ReadOnlySpan<ushort> Values => _values;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ModbusFactory();

        using var device = new ModbusConnection();

        device.Connect(_config.Host, _config.Port);

        if(!device.Connected)
        {
            return;
        }

        device.Master.WriteMultipleRegisters(5, 0, [500, 1]);

        var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_config.Period));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                device.Master.ReadInputRegisters(1, 0, 2).CopyTo(_values.AsSpan()[0..2]);
                device.Master.ReadInputRegisters(2, 0, 2).CopyTo(_values.AsSpan()[2..4]);
                device.Master.ReadInputRegisters(3, 0, 2).CopyTo(_values.AsSpan()[4..6]);
                device.Master.ReadInputRegisters(4, 0, 2).CopyTo(_values.AsSpan()[6..8]);

                OnRead();
            }
        }
        catch (Exception ex)
        {

        }
    }

    private void OnRead()
    {
        Read?.Invoke();
    }

    public event Action? Read;
}