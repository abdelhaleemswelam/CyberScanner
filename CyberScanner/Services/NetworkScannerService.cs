using System.Collections.Concurrent;
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
        var start = IPAddress.Parse(startIP).GetAddressBytes();
        var end = IPAddress.Parse(endIP).GetAddressBytes();

        var ipAddresses = GenerateIPRange(start, end).ToList();
        var results = new ConcurrentBag<ScanResult>();
        var total = ipAddresses.Count;
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
                Parallel.ForEach(ipAddresses, options, ip =>
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    var result = ScanIPAddress(ip, timeoutMs).Result;
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

    private async Task<ScanResult> ScanIPAddress(string ipAddress, int timeoutMs)
    {
        var result = new ScanResult
        {
            IPAddress = ipAddress,
            ScanTime = DateTime.Now
        };

        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(ipAddress, timeoutMs);

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
        catch
        {
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