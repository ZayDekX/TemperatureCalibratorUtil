using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using NModbus;

namespace TemperatureCalibratorUtil.Modbus;

/// <summary>
/// Simple wrapper around <see cref="IModbusMaster"/> and <see cref="TcpClient"/> for connection handling
/// </summary>
public class ModbusConnection : IDisposable
{
    /// <summary>
    /// <see cref="TcpClient"/> used for connection
    /// </summary>
    public TcpClient? Client { get; private set; }

    /// <summary>
    /// <see cref="IModbusMaster"/> used for interactions with Client via Modbus protocol
    /// </summary>
    public IModbusMaster? Master { get; private set; }

    private ModbusFactory _factory = new();

    [MemberNotNullWhen(true, nameof(Master), nameof(Client))]
    public bool Connected => Client?.Connected ?? false;

    /// <summary>
    /// Remote host to connect to
    /// </summary>
    public string? Host { get; private set; }

    /// <summary>
    /// Port of remote host
    /// </summary>
    public ushort Port { get; private set; }

    private Lock _connectionLock = new();

    /// <summary>
    /// Connect to Modbus device
    /// </summary>
    /// <param name="host"></param>
    /// <param name="port"></param>
    public void Connect(string host, ushort port)
    {
        lock (_connectionLock)
        {
            if (Connected)
            {
                return;
            }

            Host = host;
            Port = port;

            Client = new(Host, Port);
            Master = _factory.CreateMaster(Client);
        }
    }

    /// <summary>
    /// Disconnect from connected Modbus device
    /// </summary>
    public void Disconnect()
    {
        lock (_connectionLock)
        {
            Master?.Dispose();
            Client?.Dispose();

            Master = null;
            Client = null;
            Host = null;
            Port = 0;
        }
    }

    /// <summary>
    /// Reconnect to connected Modbus device
    /// </summary>
    public void Reconnect()
    {
        if (!Connected || Host is null || Port is 0)
        {
            return;
        }

        var host = Host;
        var port = Port;

        Disconnect();
        Connect(host, port);
    }

    public void Dispose()
    {
        Disconnect();
    }
}