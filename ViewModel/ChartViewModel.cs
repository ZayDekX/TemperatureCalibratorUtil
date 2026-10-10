using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Defaults;
using TemperatureCalibratorUtil.Configuration;

namespace TemperatureCalibratorUtil.ViewModel;

public partial class ChartViewModel : ObservableObject
{
    private readonly ChartConfig _config;
    private readonly ParameterConfig _parameterConfig;

    [ObservableProperty]
    public partial bool HasError { get; set; }

    public bool HasAlert => AlertLevel is not null && CurrentValue >= AlertLevel;

    public string Name => _config.Name;
    public int ParameterId => _config.ParameterId;

    public ChartViewModel(ChartConfig config, ParameterConfig parameter)
    {
        _config = config;
        _parameterConfig = parameter;

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

    public double Multiplier => _parameterConfig.Multiplier;

    public double Min => Math.Min(_parameterConfig.Min, CurrentValue - 10);

    public double Max => Math.Max(_parameterConfig.Max, CurrentValue + 10);

    public double? AlertLevel => _parameterConfig.AlertLevel;

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
            OnPropertyChanged(nameof(HasAlert));
            OnPropertyChanged(nameof(Min));
            OnPropertyChanged(nameof(Max));
        }
    }

    private static string Formatter(DateTime date)
    {
        return $"{date:HH:mm:ss}";
    }
}