@echo off
REM ============================================
REM LabGuard Host Console - Staff Monitoring
REM ============================================
REM Run this ONCE on the Host/Staff Computer
REM ============================================

echo.
echo Starting LabGuard Host Console...
echo Listening on port 9000...
echo.
echo All lab computers will connect here.
echo.
timeout /t 2

REM Start the Host application
C:\23g133\Project\Labguard\LabGuard.Host\bin\Release\net8.0-windows\LabGuard.Host.exe

pause
