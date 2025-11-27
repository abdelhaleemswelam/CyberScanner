using System.Windows.Input;

namespace CyberScanner.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    public DashboardViewModel()
    {
        Title = "Dashboard";

        NavigateToIpScannerCommand = new Command(async () => await NavigateToPage("//IpScanner"));
        NavigateToPortScannerCommand = new Command(async () => await NavigateToPage("//PortScanner"));
        NavigateToSettingsCommand = new Command(async () => await NavigateToPage("//Settings"));

        Platform = DeviceInfo.Platform.ToString();
    }

    public string Platform { get; }

    public ICommand NavigateToIpScannerCommand { get; }
    public ICommand NavigateToPortScannerCommand { get; }
    public ICommand NavigateToSettingsCommand { get; }

    private async Task NavigateToPage(string route)
    {
        await Shell.Current.GoToAsync(route);
    }
}