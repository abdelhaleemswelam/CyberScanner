using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace CyberScanner.Services;

public static class MacAddressResolver
{
    public static async Task<string> GetMacAddressAsync(string ipAddress)
    {
        return await Task.Run(() =>
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return GetMacAddressWindows(ipAddress);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                         RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return GetMacAddressUnix(ipAddress);
                }
            }
            catch
            {
                // Fall through to ARP table method
            }

            return GetMacFromArpTable(ipAddress);
        });
    }

    private static string GetMacAddressWindows(string ipAddress)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "arp",
                    Arguments = $"-a {ipAddress}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return ParseArpOutput(output, ipAddress);
        }
        catch
        {
            return "Unknown";
        }
    }

    private static string GetMacAddressUnix(string ipAddress)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "arp",
                    Arguments = $"-n {ipAddress}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return ParseArpOutput(output, ipAddress);
        }
        catch
        {
            return "Unknown";
        }
    }

    private static string GetMacFromArpTable(string ipAddress)
    {
        try
        {
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                var properties = networkInterface.GetIPProperties();
                foreach (var unicastAddress in properties.UnicastAddresses)
                {
                    if (unicastAddress.Address.ToString() == ipAddress)
                    {
                        return networkInterface.GetPhysicalAddress().ToString();
                    }
                }
            }
        }
        catch
        {
            // Ignore errors
        }

        return "Unknown";
    }

    private static string ParseArpOutput(string output, string ipAddress)
    {
        var lines = output.Split('\n');
        foreach (var line in lines)
        {
            if (line.Contains(ipAddress))
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    // Find MAC address in the line
                    foreach (var part in parts)
                    {
                        if (part.Contains('-') || part.Contains(':'))
                        {
                            return part.ToUpper();
                        }
                    }
                }
            }
        }
        return "Unknown";
    }
}