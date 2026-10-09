using System.Windows;
using System.Windows.Navigation;
using TemperatureCalibratorUtil.Configuration;
using TemperatureCalibratorUtil.Model;
using TemperatureCalibratorUtil.ViewModel;

namespace TemperatureCalibratorUtil;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private SystemRunner _runner;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var config = new Config();

        _runner = new SystemRunner(config);

        MainWindow = new MainWindow()
        {
            DataContext = new MainWindowViewModel(config, _runner)
        };

        MainWindow.Show();
    }
}