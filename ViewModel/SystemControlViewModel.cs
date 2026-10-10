using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TemperatureCalibratorUtil.Configuration;
using TemperatureCalibratorUtil.Model;

namespace TemperatureCalibratorUtil.ViewModel;

public partial class SystemControlViewModel : ObservableObject
{
    private readonly SystemRunner _runner;
    private readonly SystemConfig _config;

    public SystemControlViewModel(SystemRunner runner, SystemConfig config)
    {
        _runner = runner;
        _config = config;
        IsStable = _runner.System.Stable;
        _runner.System.Stabilized += OnStabilized;
        _runner.System.Destabilized += OnDestabilized;
    }

    public string Stability => IsStable ? "Stable" : "Not stable";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Stability))]
    public partial bool IsStable { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotRunning))]
    [NotifyPropertyChangedFor(nameof(StartButtonText))]
    [NotifyPropertyChangedFor(nameof(StartButtonCommand))]
    public partial bool IsRunning { get; set; }

    public bool IsNotRunning => !IsRunning;

    public string StartButtonText => IsRunning ? "Stop" : "Start";

    public IRelayCommand StartButtonCommand => IsRunning ? StopCommand : StartCommand;

    public int Period
    {
        get => _runner.UpdatePeriod;
        set
        {
            if (value == _runner.UpdatePeriod)
            {
                return;
            }
            OnPropertyChanging(nameof(Period));
            _runner.UpdatePeriod = value;
            _config.UpdatePeriod = value;
            OnPropertyChanged(nameof(Period));
        }
    }

    public double TargetTemperature
    {
        get => _config.TargetTemperature;
        set
        {
            if (_config.TargetTemperature == value)
            {
                return;
            }

            OnPropertyChanging(nameof(TargetTemperature));
            _config.TargetTemperature = value;
            OnPropertyChanged(nameof(TargetTemperature));
        }
    }

    [RelayCommand]
    private async Task Start(CancellationToken cancellationToken)
    {
        await _runner.StartAsync(cancellationToken);
        IsRunning = true;
    }

    [RelayCommand]
    public async Task Stop(CancellationToken cancellationToken)
    {
        await _runner.StopAsync(cancellationToken);
        IsRunning = false;
    }

    private void OnStabilized()
    {
        IsStable = true;
    }

    private void OnDestabilized()
    {
        IsStable = false;
    }
}