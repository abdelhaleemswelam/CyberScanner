using System.Collections.ObjectModel;
using System.Windows.Input;
using CyberScanner.Models;
using CyberScanner.Services;

namespace CyberScanner.ViewModels;

public class PortScannerViewModel : BaseViewModel
{
    private readonly PortScannerService _portScanner;
    private string _targetIP = "127.0.0.1";
    private string _portRange = "1-1000";
    private int _timeoutMs = 1000;
    private int _maxThreads = 50;
    private bool _isScanning;
    private int _progress;
    private string _statusMessage = "Ready to scan";
    private string _selectedProfile = "Quick Scan";
    private string _customPorts = "";

    public PortScannerViewModel()
    {
        _portScanner = new PortScannerService();
        Title = "Port Scanner";

        ScanCommand = new Command(async () => await ExecuteScanAsync(), () => !IsScanning);
        StopCommand = new Command(ExecuteStopScan, () => IsScanning);
        ExportCsvCommand = new Command(ExecuteExportCsv, () => PortResults.Any());
        CopyToClipboardCommand = new Command(ExecuteCopyToClipboard, () => PortResults.Any());

        // Initialize port profiles
        PortProfiles = new List<string>
        {
            "Quick Scan",
            "Common Ports",
            "Web Services",
            "Full Scan",
            "Custom Ports"
        };

        LoadSampleData();
    }

    public ObservableCollection<PortResult> PortResults { get; } = new ObservableCollection<PortResult>();
    public List<string> PortProfiles { get; }

    public string TargetIP
    {
        get => _targetIP;
        set => SetProperty(ref _targetIP, value);
    }

    public string PortRange
    {
        get => _portRange;
        set => SetProperty(ref _portRange, value);
    }

    public string CustomPorts
    {
        get => _customPorts;
        set => SetProperty(ref _customPorts, value);
    }

    public string SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            SetProperty(ref _selectedProfile, value);
            UpdatePortRangeForProfile();
        }
    }

    public int TimeoutMs
    {
        get => _timeoutMs;
        set => SetProperty(ref _timeoutMs, value);
    }

    public int MaxThreads
    {
        get => _maxThreads;
        set => SetProperty(ref _maxThreads, value);
    }

    public bool IsScanning
    {
        get => _isScanning;
        set
        {
            SetProperty(ref _isScanning, value);
            ((Command)ScanCommand).ChangeCanExecute();
            ((Command)StopCommand).ChangeCanExecute();
        }
    }

    public int Progress
    {
        get => _progress;
        set => SetProperty(ref _progress, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ICommand ScanCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand ExportCsvCommand { get; }
    public ICommand CopyToClipboardCommand { get; }

    private CancellationTokenSource _cancellationTokenSource;

    private void UpdatePortRangeForProfile()
    {
        switch (SelectedProfile)
        {
            case "Quick Scan":
                PortRange = "1-1000";
                break;
            case "Common Ports":
                PortRange = "21,22,23,25,53,80,110,135,139,143,443,445,993,995,1433,3306,3389,5432,5900,8080";
                break;
            case "Web Services":
                PortRange = "80,443,8080,8443,3000,4200,5000,8000,8081,8888,9000";
                break;
            case "Full Scan":
                PortRange = "1-65535";
                break;
            case "Custom Ports":
                PortRange = CustomPorts;
                break;
        }
    }

    private async Task ExecuteScanAsync()
    {
        if (IsScanning) return;

        try
        {
            IsScanning = true;
            Progress = 0;
            StatusMessage = "Initializing port scan...";
            _cancellationTokenSource = new CancellationTokenSource();

            PortResults.Clear();

            var ports = ParsePorts(PortRange);
            var results = await _portScanner.ScanPortsAsync(
                TargetIP, ports, TimeoutMs, MaxThreads, _cancellationTokenSource.Token,
                new Progress<int>(p => Progress = p));

            foreach (var result in results.OrderBy(r => r.Port))
            {
                PortResults.Add(result);
            }

            var openPorts = results.Count(r => r.Status == "Open");
            StatusMessage = $"Scan completed. Found {openPorts} open ports out of {ports.Count} scanned.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Port scan cancelled.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Port scan failed: {ex.Message}";
        }
        finally
        {
            IsScanning = false;
            Progress = 100;
        }
    }

    private List<int> ParsePorts(string portRange)
    {
        var ports = new List<int>();

        if (string.IsNullOrWhiteSpace(portRange))
            return ports;

        // Handle comma-separated ports
        if (portRange.Contains(','))
        {
            foreach (var part in portRange.Split(','))
            {
                if (int.TryParse(part.Trim(), out int port))
                {
                    ports.Add(port);
                }
            }
            return ports.Distinct().ToList();
        }

        // Handle range (e.g., "1-1000")
        if (portRange.Contains('-'))
        {
            var rangeParts = portRange.Split('-');
            if (rangeParts.Length == 2 &&
                int.TryParse(rangeParts[0].Trim(), out int start) &&
                int.TryParse(rangeParts[1].Trim(), out int end))
            {
                for (int port = start; port <= end; port++)
                {
                    ports.Add(port);
                }
            }
            return ports;
        }

        // Single port
        if (int.TryParse(portRange.Trim(), out int singlePort))
        {
            ports.Add(singlePort);
        }

        return ports;
    }

    private void ExecuteStopScan()
    {
        _cancellationTokenSource?.Cancel();
        StatusMessage = "Stopping port scan...";
    }

    private async void ExecuteExportCsv()
    {
        try
        {
            var exportService = new CsvExportService();
            var csvContent = exportService.ExportPortResults(PortResults.ToList());

            var filePath = Path.Combine(FileSystem.CacheDirectory, $"port_scan_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            await File.WriteAllTextAsync(filePath, csvContent);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Export Port Scan Results",
                File = new ShareFile(filePath)
            });

            StatusMessage = "Port results exported successfully.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export failed: {ex.Message}";
        }
    }

    private async void ExecuteCopyToClipboard()
    {
        var text = string.Join(Environment.NewLine, PortResults.Select(r =>
            $"{r.IPAddress}:{r.Port} - {r.Service} - {r.Status}"));

        await Clipboard.Default.SetTextAsync(text);
        StatusMessage = "Port results copied to clipboard.";
    }

    private void LoadSampleData()
    {

    }
}