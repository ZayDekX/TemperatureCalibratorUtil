using System.IO;
using System.Text.Json;
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
    private const string ConfigName = "config.json";
    private SystemRunner _runner;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Config config;

        try
        {
            config = JsonSerializer.Deserialize<Config>(File.ReadAllText(ConfigName))!;
        }
        catch
        {
            config = new();
            File.WriteAllText(ConfigName, JsonSerializer.Serialize(config));
        }

        _runner = new SystemRunner(config);

        MainWindow = new MainWindow()
        {
            DataContext = new MainWindowViewModel(config, _runner)
        };

        MainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _runner.StopAsync(CancellationToken.None).Wait();
        base.OnExit(e);
    }
}