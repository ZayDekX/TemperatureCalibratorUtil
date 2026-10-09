using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TemperatureCalibratorUtil.Configuration;
using TemperatureCalibratorUtil.Model;

namespace TemperatureCalibratorUtil.ViewModel;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly SystemRunner _runner;

    public MainWindowViewModel(Config config, SystemRunner runner)
    {
        Config = config;
        _runner = runner;
        Graphs = [.. config.Graphs.Select(x => new ValueGraphViewModel(x.Name, x.ParameterId, x.ErrorId, config.Parameters.Input[x.ParameterId].Multiplier))];
        _runner.Device.Read += OnRead;
        _runner.Device.Connected += OnConnected;
        _runner.Device.Disconnected += OnDisconnected;
        _runner.System.Stabilized += OnStabilized;
        _runner.System.Destabilized += OnDestabilized;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotRunning))]
    [NotifyPropertyChangedFor(nameof(StartButtonText))]
    [NotifyPropertyChangedFor(nameof(StartButtonCommand))]
    public partial bool IsRunning { get; set; }

    public bool IsNotRunning => !IsRunning;

    public string StartButtonText => IsRunning ? "Stop" : "Start";

    public IRelayCommand StartButtonCommand => IsRunning ? StopCommand : StartCommand;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ConnectionStatus))]
    [NotifyPropertyChangedFor(nameof(ConnectionStatusColor))]
    public partial bool IsDeviceConnected { get; set; }

    public string ConnectionStatus => IsDeviceConnected ? "Connected" : "Disconnected";
    public Brush ConnectionStatusColor => IsDeviceConnected ? Colors.OkBrush : Colors.ErrorBrush;

    public string Stability => IsStable ? "Stable" : "Not stable";

    public Brush StabilityColor => IsStable ? Colors.OkBrush : Colors.ErrorBrush;

    public Config Config { get; }

    /// <summary>
    /// Graphs that should be displayed
    /// </summary>
    public ValueGraphViewModel[] Graphs { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Stability))]
    [NotifyPropertyChangedFor(nameof(StabilityColor))]
    public partial bool IsStable { get; set; }

    [RelayCommand]
    private void Start()
    {
        _runner.Start();
        IsRunning = true;
    }

    [RelayCommand]
    public void Stop()
    {
        _runner.Stop();
        IsRunning = false;
    }

    private void OnRead()
    {
        var time = DateTime.Now;

        foreach (var graph in Graphs)
        {
            graph.AddPoint(time, _runner.Device.Inputs[graph.ParameterId]);
            if (graph.ErrorId >= 0)
            {
                graph.HasError = _runner.Device.Inputs[graph.ErrorId] is not 0;
            }
        }
    }

    private void OnConnected()
    {
        IsDeviceConnected = true;
    }

    private void OnDisconnected()
    {
        IsDeviceConnected = false;
    }

    private void OnStabilized()
    {
        IsStable = true;
    }

    private void OnDestabilized()
    {
        IsStable = false;
    }
}