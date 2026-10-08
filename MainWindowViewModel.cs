using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TemperatureCalibratorUtil;

public partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel()
    {
        var config = new DataFetcherConfig();

        _fetcher = new(config);
        _fetcher.Read += OnRead;
    }

    private DataFetcher _fetcher;

    /// <summary>
    /// Graphs that should be displayed
    /// </summary>
    public ValueGraphViewModel[] Graphs { get; } = [new(), new()];

    [RelayCommand]
    private void Start()
    {
        var startSource = new CancellationTokenSource();
        _fetcher.StartAsync(startSource.Token);
    }

    [RelayCommand]
    public void Stop()
    {
        var stopSource = new CancellationTokenSource();
        _fetcher.StopAsync(stopSource.Token);
    }

    private void OnRead()
    {
        var rawValues = _fetcher.Values;

        var time = DateTime.Now;

        var a = rawValues[0] / 10d;
        var b = rawValues[2] / 10d;

        Graphs[0].AddPoint(time, a);
        Graphs[1].AddPoint(time, b);
    }
}