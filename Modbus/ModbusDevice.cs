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

        _inputInfo = [.. parameters.Input.Select(x => ((byte)x.Device, (ushort)x.Address))];
        _inputBuffer = new ushort[_inputInfo.Length];
    }

    private readonly DeviceConfig _deviceConfig;
    private readonly (byte deviceId, ushort address)[] _inputInfo;

    private readonly ushort[] _inputBuffer = new ushort[8];

    private readonly ModbusConnection _connection = new();

    /// <summary>
    /// Input buffer with read values
    /// </summary>
    public ReadOnlySpan<ushort> InputBuffer => _inputBuffer;

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
        if(IsConnected)
        {
            return;
        }

        _connection.Connect(_deviceConfig.Host, _deviceConfig.Port);
        if(IsConnected)
        {
            Connected?.Invoke();
        }
    }

    public void Disconnect()
    {
        if(!IsConnected)
        {
            return;
        }

        _connection.Disconnect();
        if(!IsConnected)
        {
            Disconnected?.Invoke();
        }
    }

    public void Read()
    {
        if (!IsConnected)
        {
            return;
        }

        try
        {
            var valueSpan = _inputBuffer.AsSpan();

            // straightforward implementation
            // main downside is that creates a lot of requests to remote device, which is very critical on RTU with many devices on one bus
            for (var i = 0; i < _inputInfo.Length; i++)
            {
                var (deviceId, address) = _inputInfo[i];
                _connection.Master!.ReadInputRegisters(deviceId, address, 1).CopyTo(valueSpan[i..]);
            }
        }
        catch (Exception ex)
        {
            OnError(ex);
        }
    }

    public void Write(ParameterConfig parameter, ushort value)
    {
        if (!IsConnected)
        {
            return;
        }

        try
        {
            _connection.Master!.WriteSingleRegister(parameter.Device, parameter.Address, value);
        }
        catch (Exception ex)
        {
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