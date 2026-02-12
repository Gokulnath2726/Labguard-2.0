# LabGuard - Setup Guide

## Prerequisites Installation

### 1. Install .NET 8 SDK

**Option A: Download from Microsoft (Recommended)**
- Go to: https://aka.ms/dotnet/download
- Select **.NET 8.0** (LTS)
- Choose **Windows x64** installer
- Run the installer and follow prompts
- Restart your terminal/PowerShell

**Option B: Install via Chocolatey (if installed)**
```powershell
choco install dotnet-sdk-8.0
```

**Option C: Install via winget**
```powershell
winget install Microsoft.DotNet.SDK.8
```

### 2. Verify Installation

```powershell
dotnet --version
dotnet --list-sdks
```

Expected output: `8.0.x` (e.g., `8.0.1`)

---

## Build & Run After SDK Installation

### Build Solution
```powershell
cd c:\23g133\Project\Labguard
dotnet build
```

### Run Host Console (WPF)
```powershell
dotnet run --project LabGuard.Host
```

### Run Client Agent (in separate terminal)
```powershell
dotnet run --project LabGuard.Client
```

---

## Troubleshooting

**"No .NET SDKs were found"**
- Restart PowerShell or VS Code after installing SDK
- Verify with `dotnet --version`
- Check PATH includes `C:\Program Files\dotnet`

**"The application 'build' does not exist"**
- Same issue — SDK not found or PATH not updated
- Restart terminal and try again

**WPF Won't Display**
- WPF requires Windows 10/11
- Cannot run on Windows Server without GUI packages

---

## Project Status

✓ Solution structure created  
✓ All project files generated  
✓ Message protocol ready  
⏳ Awaiting .NET 8 SDK installation to compile
