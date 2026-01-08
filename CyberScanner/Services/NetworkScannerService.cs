using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using CyberScanner.Models;

namespace CyberScanner.Services;

public class NetworkScannerService
{
    public async Task<List<ScanResult>> ScanIPRangeAsync(string startIP, string endIP, int timeoutMs, int maxThreads,
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

        if (!IPAddress.TryParse(startIP, out var startAddress) || startAddress.AddressFamily != AddressFamily.InterNetwork ||
            !IPAddress.TryParse(endIP, out var endAddress) || endAddress.AddressFamily != AddressFamily.InterNetwork)
        {
            throw new ArgumentException("Start and end IP must be valid IPv4 addresses.");
        }

        var start = startAddress.GetAddressBytes();
        var end = endAddress.GetAddressBytes();

        if (IPAddress.NetworkToHostOrder(BitConverter.ToInt32(start, 0)) >
            IPAddress.NetworkToHostOrder(BitConverter.ToInt32(end, 0)))
        {
            (start, end) = (end, start);
        }

        var ipAddresses = GenerateIPRange(start, end).ToList();
        var results = new ConcurrentBag<ScanResult>();
        var total = ipAddresses.Count;
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
            await Parallel.ForEachAsync(ipAddresses, options, async (ip, token) =>
            {
                if (token.IsCancellationRequested)
                    return;

                var result = await ScanIPAddress(ip, timeoutMs, token);
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

    private IEnumerable<string> GenerateIPRange(byte[] start, byte[] end)
    {
        var current = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(start, 0));
        var final = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(end, 0));

        for (uint i = current; i <= final; i++)
        {
            var bytes = BitConverter.GetBytes(i);
            yield return $"{bytes[3]}.{bytes[2]}.{bytes[1]}.{bytes[0]}";
        }
    }

    private async Task<ScanResult> ScanIPAddress(string ipAddress, int timeoutMs, CancellationToken cancellationToken)
    {
        var result = new ScanResult
        {
            IPAddress = ipAddress,
            ScanTime = DateTime.Now,
            IsAlive = false,
            Hostname = "Unknown",
            MACAddress = "Unknown"
        };

        try
        {
            using var ping = new Ping();
            var pingTask = ping.SendPingAsync(ipAddress, timeoutMs);
            var completedTask = await Task.WhenAny(pingTask, Task.Delay(timeoutMs, cancellationToken));

            if (completedTask != pingTask)
            {
                return result;
            }

            var reply = await pingTask;

            if (reply.Status == IPStatus.Success)
            {
                result.IsAlive = true;
                result.PingTime = reply.RoundtripTime;

                // Get hostname
                result.Hostname = await GetHostnameAsync(ipAddress);

                // Get MAC address
                result.MACAddress = await MacAddressResolver.GetMacAddressAsync(ipAddress);
            }
        }
        catch (OperationCanceledException)
        {
            // Cancellation requested, return partial result
        }
        catch
        {
            Debug.WriteLine($"Failed to scan {ipAddress}.");
            result.IsAlive = false;
        }

        return result;
    }

    private async Task<string> GetHostnameAsync(string ipAddress)
    {
        try
        {
            var entry = await Dns.GetHostEntryAsync(ipAddress);
            return entry.HostName;
        }
        catch
        {
            return "Unknown";
        }
    }
}
