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

        _ = ReadData();
        Values = new DateTimePoint[count];

        var time = DateTime.Now;
        for (var i = 0; i < count; i++)
        {
            Values[i] = new(time, null);
        }

        Separators = new([0,0,0,0,0,0]);
    }

    public ObservableCollection<double> Separators { get; set; }

    public DateTimePoint[] Values { get; set; }

    public Func<DateTime, string> LabelsFormatter { get; } = Formatter;

    public object Sync { get; } = new object();

    public bool IsReading { get; set; } = true;

    private async Task ReadData()
    {
        var random = new Random();

        while (IsReading)
        {
            await Task.Delay(100);

            AddPoint(DateTime.Now, random.Next(0, 10));
        }
    }

    public void AddPoint(DateTime time, double value)
    {
        // to avoid unnecessary allocations, just move all points towards beginning and write first point to the end of list
        // so technically we just copy contiguous array of pointers into itself

        var firstPoint = Values[0];
        var span = Values.AsSpan();
        span[1..].CopyTo(span);

        // and set incoming value to the last item

        span[^1] = firstPoint;
        firstPoint.Value = value;
        firstPoint.DateTime = time;

        var separatorCount = Separators.Count;

        for (var i = 0; i < separatorCount; i++)
        {
            Separators[i] = time.AddSeconds(-5 * (separatorCount - i)).Ticks;
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