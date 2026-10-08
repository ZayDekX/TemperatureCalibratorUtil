using System.Collections.ObjectModel;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Defaults;

namespace TemperatureCalibratorUtil;

/// <summary>
/// Interaction logic for ValueGraph.xaml
/// </summary>
public partial class ValueGraph : UserControl
{
    public ValueGraph()
    {
        InitializeComponent();
    }
}

public partial class ValueGraphViewModel : ObservableObject
{
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

    public ObservableCollection<double> Separators { get; set; }

    public ObservableCollection<DateTimePoint> Values { get; set; }

    public Func<DateTime, string> LabelsFormatter { get; } = Formatter;

    public object Sync { get; } = new object();

    public void AddPoint(DateTime time, double value)
    {
        // to avoid unnecessary allocations, just move all points towards beginning and write first point to the end of list
        // so technically we just copy contiguous array of pointers into itself
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

            lastPoint.Value = value;
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