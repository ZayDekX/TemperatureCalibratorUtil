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
        Graphs = [.. config.Graphs.Select(x => new ValueGraphViewModel(x.Name, x.ParameterId, config.Parameters.Input[x.ParameterId].Multiplier))];
        _runner.Device.Read += OnRead;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotRunning))]
    [NotifyPropertyChangedFor(nameof(StartButtonText))]
    [NotifyPropertyChangedFor(nameof(StartButtonCommand))]
    public partial bool IsRunning { get; set; }

    public bool IsNotRunning => !IsRunning;

    public string StartButtonText => IsRunning ? "Stop" : "Start";

    public IRelayCommand StartButtonCommand => IsRunning ? StopCommand : StartCommand;

    public Config Config { get; }

    /// <summary>
    /// Graphs that should be displayed
    /// </summary>
    public ValueGraphViewModel[] Graphs { get; }

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
        }
    }
}