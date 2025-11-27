using System.Collections.ObjectModel;
using System.Windows.Input;
using CyberScanner.Models;
using CyberScanner.Services;

namespace CyberScanner.ViewModels;

public class IpScannerViewModel : BaseViewModel
{
    private readonly NetworkScannerService _scannerService;
    private string _startIP = "192.168.1.1";
    private string _endIP = "192.168.1.254";
    private int _timeoutMs = 1000;
    private int _maxThreads = 50;
    private bool _isScanning;
    private int _progress;
    private string _statusMessage = "Ready to scan";

    public IpScannerViewModel()
    {
        _scannerService = new NetworkScannerService();
        Title = "IP Range Scanner";

        ScanCommand = new Command(async () => await ExecuteScanAsync(), () => !IsScanning);
        StopCommand = new Command(ExecuteStopScan, () => IsScanning);
        ExportCsvCommand = new Command(ExecuteExportCsv, () => ScanResults.Any());
        CopyToClipboardCommand = new Command(ExecuteCopyToClipboard, () => ScanResults.Any());

        // Sample data for preview
        LoadSampleData();
    }

    public ObservableCollection<ScanResult> ScanResults { get; } = new ObservableCollection<ScanResult>();

    public string StartIP
    {
        get => _startIP;
        set => SetProperty(ref _startIP, value);
    }

    public string EndIP
    {
        get => _endIP;
        set => SetProperty(ref _endIP, value);
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

    private async Task ExecuteScanAsync()
    {
        if (IsScanning) return;

        try
        {
            IsScanning = true;
            Progress = 0;
            StatusMessage = "Initializing scan...";
            _cancellationTokenSource = new CancellationTokenSource();

            ScanResults.Clear();

            var results = await _scannerService.ScanIPRangeAsync(
                StartIP, EndIP, TimeoutMs, MaxThreads, _cancellationTokenSource.Token,
                new Progress<int>(p => Progress = p));

            foreach (var result in results.OrderByDescending(r => r.IsAlive).ThenBy(r => r.IPAddress))
            {
                ScanResults.Add(result);
            }

            StatusMessage = $"Scan completed. Found {results.Count(r => r.IsAlive)} alive devices.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Scan cancelled.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Scan failed: {ex.Message}";
        }
        finally
        {
            IsScanning = false;
            Progress = 100;
        }
    }

    private void ExecuteStopScan()
    {
        _cancellationTokenSource?.Cancel();
        StatusMessage = "Stopping scan...";
    }

    private async void ExecuteExportCsv()
    {
        try
        {
            var exportService = new CsvExportService();
            var csvContent = exportService.ExportScanResults(ScanResults.ToList());

            var filePath = Path.Combine(FileSystem.CacheDirectory, $"scan_results_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            await File.WriteAllTextAsync(filePath, csvContent);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Export Scan Results",
                File = new ShareFile(filePath)
            });

            StatusMessage = "Results exported successfully.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export failed: {ex.Message}";
        }
    }

    private async void ExecuteCopyToClipboard()
    {
        var text = string.Join(Environment.NewLine, ScanResults.Select(r =>
            $"{r.IPAddress}\t{r.Hostname}\t{r.MACAddress}\t{r.PingTime}ms\t{(r.IsAlive ? "Alive" : "Dead")}"));

        await Clipboard.Default.SetTextAsync(text);
        StatusMessage = "Results copied to clipboard.";
    }

    private void LoadSampleData()
    {

    }
}