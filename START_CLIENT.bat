@echo off
REM ============================================
REM LabGuard Client Agent - Lab Computer
REM ============================================
REM Run this on each STUDENT/LAB computer
REM ============================================

echo.
echo ============================================
echo  LabGuard Client - Lab Monitoring Agent
echo ============================================
echo.
echo Enter the Host/Server IP Address
echo (The computer where Host is running)
echo.

set /p HOST_IP="Host IP Address (example: 192.168.1.10): "

if "%HOST_IP%"=="" (
    echo Using default: 127.0.0.1
    set HOST_IP=127.0.0.1
)

echo.
echo Connecting to Host at: %HOST_IP%:9000
echo.
timeout /t 2

REM Start the Client application with Host IP
C:\23g133\Project\Labguard\LabGuard.Client\bin\Release\net8.0-windows\LabGuard.Client.exe %HOST_IP%

pause
