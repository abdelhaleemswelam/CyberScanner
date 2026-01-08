using CyberScanner.Models;

namespace CyberScanner.Services;

public class CsvExportService
{
    private static string EscapeCsv(string value)
    {
        if (value == null)
        {
            return "\"\"";
        }

        var escaped = value.Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }

    public string ExportScanResults(List<ScanResult> results)
    {
        var csv = new System.Text.StringBuilder();

        // Header
        csv.AppendLine("IP Address,Hostname,MAC Address,Ping Time (ms),Status,Scan Time");

        // Data
        foreach (var result in results)
        {
            csv.AppendLine($"{EscapeCsv(result.IPAddress)},{EscapeCsv(result.Hostname)},{EscapeCsv(result.MACAddress)},{result.PingTime},{EscapeCsv(result.IsAlive ? "Alive" : "Dead")},{EscapeCsv(result.ScanTime.ToString("yyyy-MM-dd HH:mm:ss"))}");
        }

        return csv.ToString();
    }

    public string ExportPortResults(List<PortResult> results)
    {
        var csv = new System.Text.StringBuilder();

        // Header
        csv.AppendLine("IP Address,Port,Service,Status,Protocol");

        // Data
        foreach (var result in results)
        {
            csv.AppendLine($"{EscapeCsv(result.IPAddress)},{result.Port},{EscapeCsv(result.Service)},{EscapeCsv(result.Status)},{EscapeCsv(result.Protocol)}");
        }

        return csv.ToString();
    }
}
