using TemperatureCalibratorUtil.Configuration;

namespace TemperatureCalibratorUtil.Model;

/// <summary>
/// PID Controller implementation
/// </summary>
public class PIDController
{
    public PIDController(PIDControllerConfig config)
    {
        _config = config;
    }

    private double _integralSum;

    private double _lastValue;
    private double _filteredDTerm;
    private bool _isFirstRun;
    private PIDControllerConfig _config;

    public double Compute(double setpoint, double value, double dt)
    {
        if (dt <= 0) return _config.MinOutput;

        // init system on first run
        if (_isFirstRun)
        {
            _lastValue = value;
            _isFirstRun = false;
        }

        var error = setpoint - value;

        // P
        var pTerm = _config.Kp * error;

        // D

        var rawDTerm = _config.Kd * (double)(-(value - _lastValue) / dt);

        // on small time steps dValue will become huge (dividing by 0.1 is same as multiplying by 10), so it needs to be filtered
        // exponential filter is used
        _filteredDTerm = double.Lerp(_filteredDTerm, rawDTerm, _config.DFilterCoeff); // (1 - DFilterCoeff) * _filteredDTerm + DFilterCoeff * rawDTerm

        // I

        var iSum = _integralSum + error * dt;

        // conditionally update integral sum to avoid excessive influence of integral part in cases of slow system reaction
        var resultCandidate = pTerm + _filteredDTerm + _config.Ki * iSum;
        var isSaturated = resultCandidate < _config.MinOutput || resultCandidate > _config.MaxOutput;
        var sameSign = Math.Sign(error) == Math.Sign(resultCandidate);

        // candidate value must have influence in different direction to error ("inertia" of integral part) or outside of valid range
        // that allows us to change integral part only when system is not stuck on boundary and error pushes it to the same direction

        if (!(isSaturated && sameSign))
        {
            _integralSum = iSum;
        }

        var iTerm = _config.Ki * _integralSum;

        var result = pTerm + iTerm + _filteredDTerm;

        _lastValue = value;

        return Math.Clamp(result, _config.MinOutput, _config.MaxOutput);
    }
}