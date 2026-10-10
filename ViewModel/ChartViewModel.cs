using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Defaults;
using TemperatureCalibratorUtil.Configuration;

namespace TemperatureCalibratorUtil.ViewModel;

public partial class ChartViewModel : ObservableObject
{
    private ChartConfig _config;

    [ObservableProperty]
    public partial bool HasError { get; set; }

    public string Name => _config.Name;
    public int ParameterId => _config.ParameterId;

    public ChartViewModel(ChartConfig config, double multiplier)
    {
        _config = config;
        Multiplier = multiplier;

        Separators = new(new double[_config.SeparatorCount]);

        var time = DateTime.Now;

        Values = [];
        for (var i = 0; i < _config.PointCount; i++)
        {
            Values.Add(new(time, null));
        }
    }

    public ObservableCollection<double> Separators { get; set; }

    public ObservableCollection<DateTimePoint> Values { get; set; } // ideally DateTimePoint should be a struct

    public double CurrentValue => Values[^1].Value ?? 0;

    public Func<DateTime, string> LabelsFormatter { get; } = Formatter;

    public object Sync { get; } = new();

    public double Multiplier { get; }

    public int ErrorId => _config.ErrorId;

    public void AddPoint(DateTime time, double value)
    {
        // the best way to avoid unnecessary allocations would be just to move all points towards beginning and write first point to the end of list
        // or custom collection with rolling start position
        // but since LiveCharts doesn't like movement of points (basically ignores), a ton of updates is kinda mandatory
        // in any case current solution causes a bit less of GC pressure than adding new point and removing first one (as in LiveCharts example)

        lock (Sync)
        {
            for (var i = 1; i < Values.Count; i++)
            {
                var prev = Values[i - 1];
                var current = Values[i];

                prev.Value = current.Value;
                prev.DateTime = current.DateTime;
            }

            var lastPoint = Values[^1];

            lastPoint.Value = value / Multiplier;
            lastPoint.DateTime = time;

            var separatorCount = Separators.Count;

            var displayedTime = lastPoint.DateTime - Values[0].DateTime;
            var displayedSeconds = displayedTime.TotalSeconds;
            var step = displayedSeconds / 5;

            for (var i = 0; i < separatorCount; i++)
            {
                Separators[i] = time.AddSeconds(-step * (separatorCount - i)).Ticks;
            }
            OnPropertyChanged(nameof(CurrentValue));
        }
    }

    private static string Formatter(DateTime date)
    {
        return $"{date:HH:mm:ss}";
    }
}