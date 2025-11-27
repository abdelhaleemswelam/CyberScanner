# 🔮 CyberScanner

<div align="center">

![CyberScanner Banner](https://via.placeholder.com/800x200/0A0A0F/C000FF?text=CYBERSCANNER+NETWORK+ANALYSIS+SUITE)

*A futuristic network scanning tool with cyberpunk aesthetics*

[![.NET MAUI](https://img.shields.io/badge/.NET%20MAUI-10.0-512BD4?style=for-the-badge&logo=.net)](https://dotnet.microsoft.com/en-us/apps/maui)
[![Platforms](https://img.shields.io/badge/Platforms-Windows%20%7C%20macOS%20%7C%20Android-00E5FF?style=for-the-badge)](https://dotnet.microsoft.com/en-us/platform)
[![License](https://img.shields.io/badge/License-MIT-00FF88?style=for-the-badge)](LICENSE)

</div>

## 🌟 Features

### 🎯 IP Range Scanner
- **Multi-threaded IP discovery** with configurable thread limits
- **Ping latency measurement** in milliseconds
- **Reverse DNS lookup** for hostname resolution
- **MAC address detection** using ARP table parsing
- **Alive/Dead status** with color-coded results
- **Export to CSV** and clipboard support

### 🔍 Port Scanner
- **Multiple scan profiles**: Quick Scan, Common Ports, Web Services, Full Scan
- **Custom port ranges** and individual port specification
- **Service name detection** for common ports (HTTP, SSH, FTP, etc.)
- **TCP socket probing** with configurable timeouts
- **Real-time progress tracking** during scans

### 🎨 Cyberpunk UI
- **Futuristic design** with dark theme and neon accents
- **Purple (#C000FF) and cyan (#00E5FF)** color scheme
- **Glowing borders** and modern card-based layout
- **Responsive design** for desktop and mobile
- **Smooth animations** and visual feedback

### ⚡ Performance
- **Async/await architecture** for non-blocking UI
- **Parallel processing** with cancellation support
- **Memory-efficient** scanning with progress reporting
- **Cross-platform compatibility** using .NET MAUI

## 🚀 Quick Start

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Visual Studio 2026](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)

### Installation & Build

```bash
# Clone the repository
git clone https://github.com/abdelhaleemswelam/cyberscanner.git
cd cyberscanner

# Restore dependencies
dotnet restore

# Build the application
dotnet build

# Run on specific platform
dotnet build -t:Run -f net10.0-android
dotnet build -t:Run -f net10.0-maccatalyst
dotnet build -t:Run -f net10.0-windows10.0.19041.0
