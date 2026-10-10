using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using TemperatureCalibratorUtil.Configuration;
using TemperatureCalibratorUtil.Model;

namespace TemperatureCalibratorUtil.ViewModel;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly SystemRunner _runner;

    public MainWindowViewModel(Config config, SystemRunner runner)
    {
        _runner = runner;
        Charts = [.. config.Charts.Select(x => new ChartViewModel(x, config.Parameters.Input[x.ParameterId]))];

        ConnectionControl = new(_runner, config.Device);
        SystemControl = new(_runner, config.System, config.Parameters);

        _runner.Updated += OnUpdate;
        _runner.Device.Disconnected += OnDisconnected;
        _runner.Device.Error += OnError;
    }

    private void OnError(Exception exception)
    {
        MessageBox.Show(
            exception.Message,
            caption: "An error occured!",
            button: MessageBoxButton.OK,
            icon: MessageBoxImage.Error
        );
    }

    private void OnDisconnected()
    {
        MessageBox.Show(
            "Remote device has been disconnected",
            caption: "Warning",
            button: MessageBoxButton.OK,
            icon: MessageBoxImage.Warning
        );
    }

    [ObservableProperty]
    public partial ConnectionControlViewModel ConnectionControl { get; set; }

    [ObservableProperty]
    public partial SystemControlViewModel SystemControl { get; set; }

    /// <summary>
    /// Charts that should be displayed
    /// </summary>
    public IEnumerable<ChartViewModel> Charts { get; }

    private void OnUpdate()
    {
        var time = DateTime.Now;

        foreach (var chart in Charts)
        {
            chart.AddPoint(time, _runner.Device.InputBuffer[chart.ParameterId]);
            if (chart.ErrorId >= 0)
            {
                chart.HasError = _runner.Device.InputBuffer[chart.ErrorId] is not 0;
            }
        }
    }
}