using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TemperatureCalibratorUtil.Configuration;
using TemperatureCalibratorUtil.Model;

namespace TemperatureCalibratorUtil.ViewModel;

public partial class ConnectionControlViewModel : ObservableObject
{
    private readonly SystemRunner _runner;
    private readonly DeviceConfig _config;

    public ConnectionControlViewModel(SystemRunner runner, DeviceConfig config)
    {
        _runner = runner;
        _config = config;
        IsDeviceConnected = _runner.Device.IsConnected;
        _runner.Device.Connected += OnConnected;
        _runner.Device.Disconnected += OnDisconnected;
        _runner.Started += OnStarted;
        _runner.Stopped += OnStopped;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ConnectionStatus))]
    [NotifyPropertyChangedFor(nameof(ToggleConnectionCommand))]
    [NotifyPropertyChangedFor(nameof(ToggleConnectionText))]
    public partial bool IsDeviceConnected { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotRunning))]
    public partial bool IsRunning { get; private set; }
    public bool IsNotRunning => !IsRunning; // better to be implemented via converters

    public string ConnectionStatus => IsDeviceConnected ? "Connected" : "Disconnected";

    public string Host
    {
        get => _config.Host;
        set
        {
            if (value == _config.Host)
            {
                return;
            }
            OnPropertyChanging(nameof(Host));
            _config.Host = value;
            OnPropertyChanged(nameof(Host));
        }
    }

    public ushort Port
    {
        get => _config.Port;
        set
        {
            if (value == _config.Port)
            {
                return;
            }
            OnPropertyChanging(nameof(Port));
            _config.Port = value;
            OnPropertyChanged(nameof(Port));
        }
    }

    public IRelayCommand ToggleConnectionCommand => IsDeviceConnected ? DisconnectCommand : ConnectCommand;
    public string ToggleConnectionText => IsDeviceConnected ? "Disconnect" : "Connect";

    [RelayCommand]
    public void Connect()
    {
        _runner.Device.Connect();
    }

    [RelayCommand]
    public void Disconnect()
    {
        _runner.Device.Disconnect();
    }

    private void OnConnected()
    {
        IsDeviceConnected = true;
    }

    private void OnDisconnected()
    {
        IsDeviceConnected = false;
    }

    private void OnStarted()
    {
        IsRunning = true;
    }

    private void OnStopped()
    {
        IsRunning = false;
    }
}