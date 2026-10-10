using System.Windows;
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
        IsConnected = _runner.Device.IsConnected;
        _runner.Device.Connected += OnConnected;
        _runner.Device.Disconnected += OnDisconnected;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ConnectionStatus))]
    [NotifyPropertyChangedFor(nameof(IsNotConnected))]
    [NotifyPropertyChangedFor(nameof(ToggleConnectionCommand))]
    [NotifyPropertyChangedFor(nameof(ToggleConnectionText))]
    public partial bool IsConnected { get; set; }

    public bool IsNotConnected => !IsConnected; // better to be implemented via converters

    public string ConnectionStatus => IsConnected ? "Connected" : "Disconnected";

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

    public IRelayCommand ToggleConnectionCommand => IsConnected ? DisconnectCommand : ConnectCommand;
    public string ToggleConnectionText => IsConnected ? "Disconnect" : "Connect";

    [RelayCommand]
    public void Connect()
    {
        try
        {
            _runner.Device.Connect();
        }
        catch
        {
            MessageBox.Show(
                "Failed to connect to system",
                caption: "Error",
                button: MessageBoxButton.OK,
                icon: MessageBoxImage.Error
            );
        }
    }

    [RelayCommand]
    public void Disconnect()
    {
        if(_runner.IsRunning)
        {
            MessageBox.Show(
                "Stop system before disconnect", 
                caption: "Warning", 
                button: MessageBoxButton.OK, 
                MessageBoxImage.Warning
            );
            return;
        }
        _runner.Device.Disconnect();
    }

    private void OnConnected()
    {
        IsConnected = true;
    }

    private void OnDisconnected()
    {
        IsConnected = false;
    }
}