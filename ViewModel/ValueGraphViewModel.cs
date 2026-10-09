using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Defaults;

namespace TemperatureCalibratorUtil.ViewModel;

public partial class ValueGraphViewModel : ObservableObject
{
    public string Name { get; set; }
    public int ParameterId { get; set; }

    public ValueGraphViewModel()
    {
        var count = 250;
        var time = DateTime.Now;
        Values = [];

        for (var i = 0; i < count; i++)
        {
            Values.Add(new(time, null));
        }

        Separators = new([0, 0, 0, 0, 0, 0]);
    }

    public ValueGraphViewModel(string name, int parameterId, double multiplier) : this()
    {
        Name = name;
        ParameterId = parameterId;
        Multiplier = multiplier;
    }

    public ObservableCollection<double> Separators { get; set; }

    public ObservableCollection<DateTimePoint> Values { get; set; } // ideally DateTimePoint should be a struct

    public double CurrentValue => Values[^1].Value ?? 0;

    public Func<DateTime, string> LabelsFormatter { get; } = Formatter;

    public object Sync { get; } = new object();

    public double Multiplier { get; set; }

    public void AddPoint(DateTime time, double value)
    {
        // the best way to avoid unnecessary allocations would be just to move all points towards beginning and write first point to the end of list
        // or custom collection with rolling beginning
        // but since LiveCharts doesn't like movement of points (basically ignores), there is a ton of updates
        // in any case current solution causes a bit less of GC pressure than adding new point and removing first one

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

            for (var i = 0; i < separatorCount; i++)
            {
                Separators[i] = time.AddSeconds(-5 * (separatorCount - i)).Ticks;
            }
        }
    }

    private static string Formatter(DateTime date)
    {
        var delta = (DateTime.Now - date).TotalSeconds;

        return delta < 1
            ? "now"
            : $"{delta:N0}s ago";
    }
}