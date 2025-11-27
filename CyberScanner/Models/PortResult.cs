using System.ComponentModel;

namespace CyberScanner.Models;

public class PortResult : INotifyPropertyChanged
{
    private string _ipAddress;
    private int _port;
    private string _service;
    private string _status;
    private string _protocol;

    public string IPAddress
    {
        get => _ipAddress;
        set { _ipAddress = value; OnPropertyChanged(nameof(IPAddress)); }
    }

    public int Port
    {
        get => _port;
        set { _port = value; OnPropertyChanged(nameof(Port)); }
    }

    public string Service
    {
        get => _service;
        set { _service = value; OnPropertyChanged(nameof(Service)); }
    }

    public string Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(nameof(Status)); }
    }

    public string Protocol
    {
        get => _protocol;
        set { _protocol = value; OnPropertyChanged(nameof(Protocol)); }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}