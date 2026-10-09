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

    private ModbusConnection _connection = new();

    public ReadOnlySpan<ushort> Values => _values;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _connection.Connect(_config.Host, _config.Port);

        if(!_connection.Connected)
        {
            return;
        }

        Write([500, 1]);

        var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_config.Period));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                _connection.Master.ReadInputRegisters(1, 0, 2).CopyTo(_values.AsSpan()[0..2]);
                _connection.Master.ReadInputRegisters(2, 0, 2).CopyTo(_values.AsSpan()[2..4]);
                _connection.Master.ReadInputRegisters(3, 0, 2).CopyTo(_values.AsSpan()[4..6]);
                _connection.Master.ReadInputRegisters(4, 0, 2).CopyTo(_values.AsSpan()[6..8]);

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

    public void Write(ushort[] values)
    {
        _connection.Master?.WriteMultipleRegisters(5, 0, values);
    }

    public event Action? Read;

    public override void Dispose()
    {
        
    }
}