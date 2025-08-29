@echo off
:: Script to deploy MicLevelMonitor and configure system tray visibility
:: Run with administrative privileges via SCCM or manually
setlocal EnableDelayedExpansion

:: Set error codes
set ERROR_ADMIN=1
set ERROR_POWERSHELL=2
set ERROR_UNKNOWN=99

:: Detect if running from SCCM
set SCCM_DEPLOYMENT=0
tasklist /fi "imagename eq CcmExec.exe" 2>NUL | find /i "CcmExec.exe" >NUL
if not errorlevel 1 set SCCM_DEPLOYMENT=1

:: If running from SCCM, minimize console output
if %SCCM_DEPLOYMENT%==1 (
    echo MicLevelMonitor SCCM telep�t�s fut...
) else (
    echo ===== MicLevelMonitor Telep�t�eszk�z =====
    echo.
)

:: Check for administrative privileges
net session >nul 2>&1
if %errorlevel% neq 0 (
    if %SCCM_DEPLOYMENT%==0 (
        echo HIBA: Ez a script adminisztr�tori jogosults�gokat ig�nyel!
        echo K�rj�k, futtassa a scriptet rendszergazdak�nt (jobb klikk - Futtat�s rendszergazdak�nt).
        echo.
        pause
    ) else (
        echo HIBA: Adminisztr�tori jogosults�gok hi�nyoznak.
    )
    exit /b %ERROR_ADMIN%
)

:: Verify PowerShell is available and has appropriate execution policy
powershell -Command "Get-ExecutionPolicy" >nul 2>&1
if %errorlevel% neq 0 (
    if %SCCM_DEPLOYMENT%==0 (
        echo HIBA: PowerShell nem �rhet� el vagy nem megfelel�en van konfigur�lva.
        pause
    ) else (
        echo HIBA: PowerShell nem �rhet� el vagy nem megfelel�en van konfigur�lva.
    )
    exit /b %ERROR_POWERSHELL%
)

:: Set destination path
set DEST_PATH=C:\Ny�lt dokumentumok\MicLevelMonitor
if not exist "%DEST_PATH%" mkdir "%DEST_PATH%" 2>nul

:: Determine script path
set "SCRIPT_PATH=%~dp0"
if %SCCM_DEPLOYMENT%==0 (
    echo MicLevelMonitor alkalmaz�s konfigur�l�sa...
    echo.
    echo A rendszert�lca be�ll�t�sai minden felhaszn�l� sz�m�ra konfigur�l�sra ker�lnek...
    echo Ez a folyamat n�h�ny percig tarthat, k�rj�k v�rjon...
    echo.
) else (
    echo Telep�t�s �s konfigur�l�s folyamatban, k�rem v�rjon...
)

:: Run the PowerShell script with ExecutionPolicy Bypass and no profile loading for speed
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT_PATH%Deploy-MicLevelMonitor.ps1"
if %errorlevel% neq 0 (
    if %SCCM_DEPLOYMENT%==0 (
        echo.
        echo HIBA: A PowerShell script futtat�sa sor�n hiba t�rt�nt! (K�d: %errorlevel%)
        echo K�rj�k, ellen�rizze a hiba�zeneteket �s pr�b�lja �jra.
        echo A log f�jl itt tal�lhat�: %DEST_PATH%\deployment_log.txt
        echo.
        pause
    ) else (
        echo HIBA: PowerShell telep�t�si hiba. (K�d: %errorlevel%) Log: %DEST_PATH%\deployment_log.txt
    )
    exit /b %errorlevel%
)

:: Check if deployment verification file exists
if exist "%DEST_PATH%\DEPLOYMENT_SUCCESS.txt" (
    if %SCCM_DEPLOYMENT%==0 (
        echo.
        echo A telep�t�s sikeresen befejez�d�tt!
        echo Az alkalmaz�s automatikusan elindul minden rendszerind�t�skor minden felhaszn�l� sz�m�ra.
        echo Az alkalmaz�s ikonja mostant�l l�that� lesz a Windows rendszert�lc�n.
        echo.
        echo K�sz�nj�k, hogy a MicLevelMonitor alkalmaz�st haszn�lja!
        echo.
        pause
    ) else (
        echo Telep�t�s sikeresen befejezve.
    )
    exit /b 0
) else (
    if %SCCM_DEPLOYMENT%==0 (
        echo.
        echo FIGYELMEZTET�S: A telep�t�s befejez�d�tt, de ellen�rizze a log f�jlokat esetleges probl�m�k miatt.
        echo A log f�jl itt tal�lhat�: %DEST_PATH%\deployment_log.txt
        echo.
        pause
    ) else (
        echo FIGYELMEZTET�S: A telep�t�s befejez�d�tt, de hi�nyzik a meger�s�t� f�jl. Ellen�rizze a logokat.
    )
    exit /b %ERROR_UNKNOWN%
)