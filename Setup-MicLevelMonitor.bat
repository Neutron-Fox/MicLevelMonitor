@echo off
:: Script to deploy MicLevelMonitor and configure system tray visibility
:: Run with administrative privileges

echo ===== MicLevelMonitor Telepítõeszköz =====
echo.

:: Check for administrative privileges
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo Ez a script adminisztrátori jogosultságokat igényel!
    echo Kérjük, futtassa a scriptet rendszergazdaként (jobb klikk - Futtatás rendszergazdaként).
    echo.
    pause
    exit /b 1
)

echo MicLevelMonitor konfigurálása...

:: Run the PowerShell script with ExecutionPolicy Bypass
powershell.exe -ExecutionPolicy Bypass -File "%~dp0Deploy-MicLevelMonitor.ps1"

echo.
echo Kész!
echo.
pause