using CyberScanner.Models;

namespace CyberScanner.Services;

public class CsvExportService
{
    public string ExportScanResults(List<ScanResult> results)
    {
        var csv = new System.Text.StringBuilder();

        // Header
        csv.AppendLine("IP Address,Hostname,MAC Address,Ping Time (ms),Status,Scan Time");

        // Data
        foreach (var result in results)
        {
            csv.AppendLine($"\"{result.IPAddress}\",\"{result.Hostname}\",\"{result.MACAddress}\",{result.PingTime},\"{(result.IsAlive ? "Alive" : "Dead")}\",\"{result.ScanTime:yyyy-MM-dd HH:mm:ss}\"");
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
            csv.AppendLine($"\"{result.IPAddress}\",{result.Port},\"{result.Service}\",\"{result.Status}\",\"{result.Protocol}\"");
        }

        return csv.ToString();
    }
}