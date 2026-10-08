using System.Windows;

namespace TemperatureCalibratorUtil;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
}

public class DataFetcherConfig
{
    public ushort Port { get; set; } = 21316;
    public int Period { get; set; } = 50;
    public string Host { get; set; } = "127.0.0.1";
}