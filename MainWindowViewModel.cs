using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TemperatureCalibratorUtil;

public partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel()
    {
        var config = new DataFetcherConfig();

        _controller = new()
        {
            DFilterCoeff = 0.25,
            Kp = 6,
            Ki = 0.125,
            Kd = 0.8,
            MinOutput = 0,
            MaxOutput = 470
        };

        _fetcher = new(config);
        _fetcher.Read += OnRead;
    }

    private readonly DataFetcher _fetcher;
    private readonly PIDController _controller;

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

    private long _lastStamp = 0;

    private void OnRead()
    {
        if(_lastStamp is 0)
        {
            _lastStamp = Stopwatch.GetTimestamp();
        }
        var rawValues = _fetcher.Values;

        var time = DateTime.Now;

        var a = rawValues[0] / 10d;
        var b = rawValues[2] / 10d;

        Graphs[0].AddPoint(time, a);
        Graphs[1].AddPoint(time, b);

        var stamp = Stopwatch.GetTimestamp();
        var newSetpoint = _controller.Compute(50, a, Stopwatch.GetElapsedTime(_lastStamp, stamp).TotalSeconds);
        _lastStamp = stamp;

        _fetcher.Write([(ushort)(newSetpoint * 10), 1]);
    }
}