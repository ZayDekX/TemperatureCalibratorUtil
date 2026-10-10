using CommunityToolkit.Mvvm.ComponentModel;
using TemperatureCalibratorUtil.Configuration;
using TemperatureCalibratorUtil.Model;

namespace TemperatureCalibratorUtil.ViewModel;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly SystemRunner _runner;

    public MainWindowViewModel(Config config, SystemRunner runner)
    {
        _runner = runner;
        Graphs = [.. config.Charts.Select(x => new ChartViewModel(x, config.Parameters.Input[x.ParameterId].Multiplier))];

        ConnectionControl = new(_runner, config.Device);
        SystemControl = new(_runner, config.System);

        _runner.Device.Read += OnRead;
    }

    [ObservableProperty]
    public partial ConnectionControlViewModel ConnectionControl { get; set; }

    [ObservableProperty]
    public partial SystemControlViewModel SystemControl { get; set; }

    /// <summary>
    /// Charts that should be displayed
    /// </summary>
    public IEnumerable<ChartViewModel> Graphs { get; }

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
}