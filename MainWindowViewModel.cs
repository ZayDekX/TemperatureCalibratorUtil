using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TemperatureCalibratorUtil;

public partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel()
    {
        var config = new Config();

        _runner = new SystemRunner(config);

        Graphs = [.. config.Graphs.Select(x => new ValueGraphViewModel(x.Name, x.ParameterId))];
        _runner.Device.Read += OnRead;
    }

    private SystemRunner _runner;

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