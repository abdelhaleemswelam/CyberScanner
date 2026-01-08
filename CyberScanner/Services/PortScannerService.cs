using System.Collections.Concurrent;
using System.Diagnostics;
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
        if (timeoutMs <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(timeoutMs), "Timeout must be greater than zero.");
        }

        if (maxThreads <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxThreads), "Max threads must be greater than zero.");
        }

        var results = new ConcurrentBag<PortResult>();
        var total = ports.Count;
        var completed = 0;

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxThreads,
            CancellationToken = cancellationToken
        };

        if (total == 0)
        {
            progress?.Report(100);
            return results.ToList();
        }

        try
        {
            await Parallel.ForEachAsync(ports, options, async (port, token) =>
            {
                if (token.IsCancellationRequested)
                    return;

                var result = await ScanPort(ipAddress, port, timeoutMs, token);
                results.Add(result);

                var current = Interlocked.Increment(ref completed);
                progress?.Report((current * 100) / total);
            });
        }
        catch (OperationCanceledException)
        {
            // Scan was cancelled, return partial results
        }

        return results.ToList();
    }

    private async Task<PortResult> ScanPort(string ipAddress, int port, int timeoutMs, CancellationToken cancellationToken)
    {
        var result = new PortResult
        {
            IPAddress = ipAddress,
            Port = port,
            Service = GetServiceName(port),
            Protocol = "TCP",
            Status = "Closed"
        };

        try
        {
            using var client = new TcpClient();
            var task = client.ConnectAsync(ipAddress, port);

            if (await Task.WhenAny(task, Task.Delay(timeoutMs, cancellationToken)) == task && client.Connected)
            {
                result.Status = "Open";
                client.Close();
            }
        }
        catch (OperationCanceledException)
        {
            // Cancellation requested, return partial result
            result.Status = "Canceled";
        }
        catch
        {
            Debug.WriteLine($"Failed to scan {ipAddress}:{port}.");
            result.Status = "Closed";
        }

        return result;
    }

    private string GetServiceName(int port)
    {
        return _commonServices.ContainsKey(port) ? _commonServices[port] : "Unknown";
    }
}
