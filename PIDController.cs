namespace TemperatureCalibratorUtil;

/// <summary>
/// PID Controller implementation
/// </summary>
public class PIDController
{
    public PIDController()
    {
        
    }

    public PIDController(PIDControllerConfig config)
    {
        Kp = config.Kp;
        Ki = config.Ki;
        Kd = config.Kd;
        DFilterCoeff = config.DFilterCoeff;
        MinOutput = config.MinOutput;
        MaxOutput = config.MaxOutput;
    }

    /// <summary>
    /// Proportional part coefficient
    /// </summary>
    public double Kp { get; set; }
    /// <summary>
    /// Integral part coefficient
    /// </summary>
    public double Ki { get; set; }
    /// <summary>
    /// Differential part coefficient
    /// </summary>
    public double Kd { get; set; }

    /// <summary>
    /// Exponential filter coefficient for differential part
    /// </summary>
    /// <remarks>Valid value range is between 0.0 and 1.0</remarks>
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