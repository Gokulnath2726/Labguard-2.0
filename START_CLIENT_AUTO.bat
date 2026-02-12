@echo off
REM ============================================
REM LabGuard Client - Pre-Configured for Lab
REM ============================================
REM EDIT THE LINE BELOW WITH YOUR HOST IP:
REM ============================================

REM *** CONFIGURE THIS FOR YOUR LAB ***
set HOST_IP=192.168.1.10
REM Change 192.168.1.10 to your actual Host IP

echo.
echo ============================================
echo  LabGuard Client Agent - Starting...
echo ============================================
echo.
echo Connecting to: %HOST_IP%:9000
echo.

REM Start the Client application
C:\23g133\Project\Labguard\LabGuard.Client\bin\Release\net8.0-windows\LabGuard.Client.exe %HOST_IP%

pause
