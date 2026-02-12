@echo off
REM LabGuard Quick Test Script
REM This script helps test the LabGuard system on a single machine

setlocal enabledelayedexpansion

echo.
echo ============================================
echo   LabGuard - Laboratory Monitoring System
echo   Quick Test Script
echo ============================================
echo.

REM Check if .NET 8 is installed
dotnet --version >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo ERROR: .NET 8 SDK is not installed
    echo Please install .NET 8 SDK from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo [Step 1] Building LabGuard solution...
echo.
dotnet build
if %ERRORLEVEL% neq 0 (
    echo ERROR: Build failed
    pause
    exit /b 1
)

echo.
echo [Step 2] Setup complete. You can now test the system:
echo.
echo.
echo TEST SCENARIO 1: Local Testing (Same Machine)
echo ==============================================
echo.
echo Terminal 1: Run the Host (Monitoring Console)
echo   cd LabGuard.Host
echo   dotnet run
echo.
echo Terminal 2: Run the Client (Agent)
echo   cd LabGuard.Client
echo   dotnet run
echo.
echo Expected:
echo   - Host window appears with status "Listening for clients..."
echo   - Client console shows "Connected to host"
echo   - Client sends heartbeats every 10 seconds
echo   - Host shows client in the list
echo   - Click buttons to send commands to client
echo.
echo.
echo TEST SCENARIO 2: Multiple Clients
echo ==================================
echo Run the Host once, then run multiple clients in separate terminals.
echo.
echo.
echo For more information, see README.md and IMPLEMENTATION.md
echo.
pause

