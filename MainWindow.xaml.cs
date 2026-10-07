using System.Windows;

namespace TemperatureCalibratorUtil;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private DataFetcher _fetcher;

    public MainWindow()
    {
        InitializeComponent();
        _fetcher = ((App)Application.Current).Fetcher;
        _fetcher.Read += OnRead;
    }

    private void OnRead()
    {
        var values = _fetcher.Values;
    }
}