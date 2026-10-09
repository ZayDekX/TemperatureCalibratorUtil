using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using NModbus;

namespace TemperatureCalibratorUtil;

/// <summary>
/// Simple wrapper around <see cref="IModbusMaster"/> for connection handling
/// </summary>
public class ModbusConnection : IDisposable
{
    public TcpClient? Client { get; private set; }

    public IModbusMaster? Master { get; private set; }

    private ModbusFactory _factory = new();

    [MemberNotNullWhen(true, nameof(Master), nameof(Client))]
    public bool Connected => Client?.Connected ?? false;

    public string? Host { get; private set; }
    public ushort Port { get; private set; }

    public void Connect(string host, ushort port)
    {
        if(Connected)
        {
            return;
        }

        Host = host;
        Port = port;

        Client = new(Host, Port);
        Master = _factory.CreateMaster(Client);
    }

    public void Disconnect()
    {
        Master?.Dispose();
        Client?.Dispose();

        Master = null;
        Client = null;
        Host = null;
        Port = 0;
    }

    public void Reconnect()
    {
        if(!Connected || Host is null || Port is 0)
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