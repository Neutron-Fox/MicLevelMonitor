@echo off
:: Script to deploy MicLevelMonitor and configure system tray visibility
:: Run with administrative privileges

echo ===== MicLevelMonitor Telep�t�eszk�z =====
echo.

:: Check for administrative privileges
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo Ez a script adminisztr�tori jogosults�gokat ig�nyel!
    echo K�rj�k, futtassa a scriptet rendszergazdak�nt (jobb klikk - Futtat�s rendszergazdak�nt).
    echo.
    pause
    exit /b 1
)

echo MicLevelMonitor konfigur�l�sa...

:: Run the PowerShell script with ExecutionPolicy Bypass
powershell.exe -ExecutionPolicy Bypass -File "%~dp0Deploy-MicLevelMonitor.ps1"

echo.
echo K�sz!
echo.
pause