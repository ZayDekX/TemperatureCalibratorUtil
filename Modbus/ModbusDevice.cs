using TemperatureCalibratorUtil.Configuration;

namespace TemperatureCalibratorUtil.Modbus;

/// <summary>
/// Represents remote modbus device
/// </summary>
public sealed class ModbusDevice : IDisposable
{
    public ModbusDevice(DeviceConfig connectionConfig, DeviceParameterConfig parameters)
    {
        _deviceConfig = connectionConfig;
        _parameters = parameters;

        _inputBuffer = new ushort[_parameters.Input.Count];
        _outputBuffer = new ushort[_parameters.Output.Count];
    }

    private readonly DeviceConfig _deviceConfig;
    private readonly DeviceParameterConfig _parameters;

    private readonly ushort[] _inputBuffer;
    private readonly ushort[] _outputBuffer;
    private readonly ModbusConnection _connection = new();

    /// <summary>
    /// Input buffer with read values
    /// </summary>
    public ReadOnlySpan<ushort> InputBuffer => _inputBuffer;
    public Span<ushort> OutputBuffer => _outputBuffer;

    /// <summary>
    /// Determines whether connection to Modbus device exists
    /// </summary>
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

    public void Connect()
    {
        if (IsConnected)
        {
            return;
        }

        _connection.Connect(_deviceConfig.Host, _deviceConfig.Port);
        if (IsConnected)
        {
            Connected?.Invoke();
        }
    }

    public void Disconnect()
    {
        if (!IsConnected)
        {
            return;
        }

        _connection.Disconnect();
        if (!IsConnected)
        {
            Disconnected?.Invoke();
        }
    }

    public void Read()
    {
        if (!IsConnected)
        {
            Disconnected?.Invoke();
            return;
        }

        try
        {
            var valueSpan = _inputBuffer.AsSpan();

            // straightforward implementation
            // main downside is that creates a lot of requests to remote device, which is very critical on RTU with many devices on one bus
            var pos = 0;
            foreach (var parameter in _parameters.Input)
            {
                _connection.Master!.ReadInputRegisters(parameter.Device, parameter.Address, 1).CopyTo(valueSpan[pos..]);
                pos++;
            }
        }
        catch (Exception ex)
        {
            if (!IsConnected)
            {
                Disconnected?.Invoke();
            }
            OnError(ex);
        }
    }

    public void Write()
    {
        if (!IsConnected)
        {
            return;
        }

        try
        {
            // straightforward implementation again
            // downsides are the same as with reader implementation
            var pos = 0;
            foreach (var parameter in _parameters.Output)
            {
                _connection.Master!.WriteSingleRegister(parameter.Device, parameter.Address, _outputBuffer[pos]);
                pos++;
            }
        }
        catch (Exception ex)
        {
            if (!IsConnected)
            {
                Disconnected?.Invoke();
            }
            OnError(ex);
        }
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    private void OnError(Exception ex)
    {
        Error?.Invoke(ex);
    }
}