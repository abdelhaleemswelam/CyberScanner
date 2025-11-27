using System.Collections.Concurrent;
using System.Net.Sockets;
using CyberScanner.Models;

namespace CyberScanner.Services;

public class PortScannerService
{
    private readonly Dictionary<int, string> _commonServices = new()
    {
        { 21, "FTP" }, { 22, "SSH" }, { 23, "Telnet" }, { 25, "SMTP" }, { 53, "DNS" },
        { 80, "HTTP" }, { 110, "POP3" }, { 135, "RPC" }, { 139, "NetBIOS" }, { 143, "IMAP" },
        { 443, "HTTPS" }, { 445, "SMB" }, { 993, "IMAPS" }, { 995, "POP3S" }, { 1433, "MSSQL" },
        { 3306, "MySQL" }, { 3389, "RDP" }, { 5432, "PostgreSQL" }, { 5900, "VNC" }, { 8080, "HTTP-Alt" }
    };

    public async Task<List<PortResult>> ScanPortsAsync(string ipAddress, List<int> ports, int timeoutMs, int maxThreads,
        CancellationToken cancellationToken, IProgress<int> progress = null)
    {
        var results = new ConcurrentBag<PortResult>();
        var total = ports.Count;
        var completed = 0;

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxThreads,
            CancellationToken = cancellationToken
        };

        try
        {
            await Task.Run(() =>
            {
                Parallel.ForEach(ports, options, port =>
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    var result = ScanPort(ipAddress, port, timeoutMs).Result;
                    results.Add(result);

                    Interlocked.Increment(ref completed);
                    progress?.Report((completed * 100) / total);
                });
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Scan was cancelled, return partial results
        }

        return results.ToList();
    }

    private async Task<PortResult> ScanPort(string ipAddress, int port, int timeoutMs)
    {
        var result = new PortResult
        {
            IPAddress = ipAddress,
            Port = port,
            Service = GetServiceName(port),
            Protocol = "TCP"
        };

        try
        {
            using var client = new TcpClient();
            var task = client.ConnectAsync(ipAddress, port);

            if (await Task.WhenAny(task, Task.Delay(timeoutMs)) == task && client.Connected)
            {
                result.Status = "Open";
                client.Close();
            }
            else
            {
                result.Status = "Closed";
            }
        }
        catch
        {
            result.Status = "Closed";
        }

        return result;
    }

    private string GetServiceName(int port)
    {
        return _commonServices.ContainsKey(port) ? _commonServices[port] : "Unknown";
    }
}