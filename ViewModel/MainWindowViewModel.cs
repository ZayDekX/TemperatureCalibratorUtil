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
        _runner = runner;
        Graphs = [.. config.Graphs.Select(x => new ValueGraphViewModel(x.Name, x.ParameterId, config.Parameters.Input[x.ParameterId].Multiplier))];
        _runner.Device.Read += OnRead;
    }

    /// <summary>
    /// Graphs that should be displayed
    /// </summary>
    public ValueGraphViewModel[] Graphs { get; }

    [RelayCommand]
    private void Start()
    {
        _runner.Start();
    }

    [RelayCommand]
    public void Stop()
    {
        _runner.Stop();
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