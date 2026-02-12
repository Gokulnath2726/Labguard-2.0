#!/usr/bin/env pwsh
<#
.SYNOPSIS
    LabGuard Quick Test Script
    
.DESCRIPTION
    This script helps test the LabGuard laboratory monitoring system
    on a single machine (localhost) or across multiple machines on a LAN.
    
.EXAMPLE
    .\RUN_TESTS.ps1
#>

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  LabGuard - Laboratory Monitoring System" -ForegroundColor Cyan
Write-Host "  Quick Test Script" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Check if .NET 8 is installed
$dnVersion = & dotnet --version 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: .NET 8 SDK is not installed" -ForegroundColor Red
    Write-Host "Please install from: https://dotnet.microsoft.com/download"
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "✓ .NET Version: $dnVersion" -ForegroundColor Green
Write-Host ""

Write-Host "[Step 1] Building LabGuard solution..." -ForegroundColor Yellow
Write-Host ""
dotnet build
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Build failed" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host ""
Write-Host "✓ Build successful!" -ForegroundColor Green
Write-Host ""

Write-Host "[Step 2] System ready for testing" -ForegroundColor Green
Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  TEST SCENARIO 1: Local Testing (Same Machine)        ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""
Write-Host "Terminal 1 - Run the Host (Monitoring Console):" -ForegroundColor Yellow
Write-Host '  cd LabGuard.Host'
Write-Host '  dotnet run'
Write-Host ""
Write-Host "Terminal 2 - Run the Client (Agent):" -ForegroundColor Yellow
Write-Host '  cd LabGuard.Client'
Write-Host '  dotnet run'
Write-Host ""
Write-Host "Expected Behavior:" -ForegroundColor Yellow
Write-Host "  1. Host window appears with 'Listening for clients...'"
Write-Host "  2. Client console shows 'Connected to host'"
Write-Host "  3. Client sends heartbeats every 10 seconds"
Write-Host "  4. Host displays client in the connected list"
Write-Host "  5. You can select client and click buttons:"
Write-Host "     - Warn: Send warning message"
Write-Host "     - Screenshot: Capture screen (saved to %LOCALAPPDATA%\LabGuard\Evidence)"
Write-Host "     - Shutdown: Initiate 60-second shutdown countdown"
Write-Host ""

Write-Host "╔════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  TEST SCENARIO 2: Multiple Clients                    ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Run Host on one machine"
Write-Host "2. Run Client on multiple machines, each connecting to Host IP"
Write-Host "3. Edit LabGuard.Client/Program.cs NetworkClient(IP) as needed"
Write-Host ""

Write-Host "╔════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  DOCUMENTATION                                         ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""
Write-Host "📄 README.md          - Project overview and features" -ForegroundColor Green
Write-Host "📄 IMPLEMENTATION.md  - Detailed architecture and design" -ForegroundColor Green
Write-Host "📄 SETUP.md           - Installation and configuration" -ForegroundColor Green
Write-Host ""

Read-Host "Press Enter to exit"

