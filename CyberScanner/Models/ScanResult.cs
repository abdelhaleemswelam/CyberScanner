using System.ComponentModel;

namespace CyberScanner.Models;

public class ScanResult : INotifyPropertyChanged
{
    private string _ipAddress;
    private string _hostname;
    private string _macAddress;
    private long _pingTime;
    private bool _isAlive;
    private DateTime _scanTime;

    public string IPAddress
    {
        get => _ipAddress;
        set { _ipAddress = value; OnPropertyChanged(nameof(IPAddress)); }
    }

    public string Hostname
    {
        get => _hostname;
        set { _hostname = value; OnPropertyChanged(nameof(Hostname)); }
    }

    public string MACAddress
    {
        get => _macAddress;
        set { _macAddress = value; OnPropertyChanged(nameof(MACAddress)); }
    }

    public long PingTime
    {
        get => _pingTime;
        set { _pingTime = value; OnPropertyChanged(nameof(PingTime)); }
    }

    public bool IsAlive
    {
        get => _isAlive;
        set { _isAlive = value; OnPropertyChanged(nameof(IsAlive)); }
    }

    public DateTime ScanTime
    {
        get => _scanTime;
        set { _scanTime = value; OnPropertyChanged(nameof(ScanTime)); }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}