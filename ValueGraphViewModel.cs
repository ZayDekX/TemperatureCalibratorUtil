using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Defaults;

namespace TemperatureCalibratorUtil;

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

    public ObservableCollection<DateTimePoint> Values { get; set; } // ideally DateTimePoint should be a struct

    public double CurrentValue => Values[^1].Value ?? 0;

    public Func<DateTime, string> LabelsFormatter { get; } = Formatter;

    public object Sync { get; } = new object();

    public void AddPoint(DateTime time, double value)
    {
        // the best way to avoid unnecessary allocations would be just to move all points towards beginning and write first point to the end of list
        // but since LiveCharts doesn't like it (basically ignores), there is a ton of updates of every single point
        // it causes a bit less of GC pressure than adding new point and removing first one

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

public class PIDController
{
    public double Kp { get; set; }
    public double Ki { get; set; }
    public double Kd { get; set; }

    /// <summary>
    /// Exponential filter coefficient for differential part
    /// </summary>
    public double DFilterCoeff { get; set; } = 0.25;
    public double MinOutput { get; set; }
    public double MaxOutput { get; set; }

    private double _integralSum;

    private double _lastValue;
    private double _filteredDTerm;
    private bool _isFirstRun;

    public double Compute(double setpoint, double value, double dt)
    {
        if (dt <= 0) return MinOutput;

        // init system on first run
        if (_isFirstRun)
        {
            _lastValue = value;
            _isFirstRun = false;
        }

        var error = setpoint - value;

        // P
        var pTerm = Kp * error;

        // D

        var rawDTerm = Kd * (double)(-(value - _lastValue) / dt);

        // on small time steps dValue will become huge (dividing by 0.1 is same as multiplying by 10), so it needs to be filtered
        // exponential filter is used
        _filteredDTerm = double.Lerp(_filteredDTerm, rawDTerm, DFilterCoeff); // (1 - DFilterCoeff) * _filteredDTerm + DFilterCoeff * rawDTerm

        // I

        var iSum = _integralSum + error * dt;

        // conditionally update integral sum to avoid excessive influence of integral part in cases of slow system reaction
        var resultCandidate = pTerm + _filteredDTerm + Ki * iSum;
        var isSaturated = resultCandidate < MinOutput || resultCandidate > MaxOutput;
        var sameSign = Math.Sign(error) == Math.Sign(resultCandidate);

        // candidate value must have influence in different direction to error ("inertia" of integral part) or outside of valid range
        // that allows us to change integral part only when system is not stuck on boundary and error pushes it to the same direction

        if (!(isSaturated && sameSign))
        {
            _integralSum = iSum;
        }

        var iTerm = Ki * _integralSum;

        var result = pTerm + iTerm + _filteredDTerm;

        _lastValue = value;

        return Math.Clamp(result, MinOutput, MaxOutput);
    }
}