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
    echo MicLevelMonitor SCCM telepítés fut...
) else (
    echo ===== MicLevelMonitor Telepítõeszköz =====
    echo.
)

:: Check for administrative privileges
net session >nul 2>&1
if %errorlevel% neq 0 (
    if %SCCM_DEPLOYMENT%==0 (
        echo HIBA: Ez a script adminisztrátori jogosultságokat igényel!
        echo Kérjük, futtassa a scriptet rendszergazdaként (jobb klikk - Futtatás rendszergazdaként).
        echo.
        pause
    ) else (
        echo HIBA: Adminisztrátori jogosultságok hiányoznak.
    )
    exit /b %ERROR_ADMIN%
)

:: Verify PowerShell is available and has appropriate execution policy
powershell -Command "Get-ExecutionPolicy" >nul 2>&1
if %errorlevel% neq 0 (
    if %SCCM_DEPLOYMENT%==0 (
        echo HIBA: PowerShell nem érhetõ el vagy nem megfelelõen van konfigurálva.
        pause
    ) else (
        echo HIBA: PowerShell nem érhetõ el vagy nem megfelelõen van konfigurálva.
    )
    exit /b %ERROR_POWERSHELL%
)

:: Set destination path
set DEST_PATH=C:\Nyílt dokumentumok\MicLevelMonitor
if not exist "%DEST_PATH%" mkdir "%DEST_PATH%" 2>nul

:: Determine script path
set "SCRIPT_PATH=%~dp0"
if %SCCM_DEPLOYMENT%==0 (
    echo MicLevelMonitor alkalmazás konfigurálása...
    echo.
    echo A rendszertálca beállításai minden felhasználó számára konfigurálásra kerülnek...
    echo Ez a folyamat néhány percig tarthat, kérjük várjon...
    echo.
) else (
    echo Telepítés és konfigurálás folyamatban, kérem várjon...
)

:: Run the PowerShell script with ExecutionPolicy Bypass and no profile loading for speed
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT_PATH%Deploy-MicLevelMonitor.ps1"
if %errorlevel% neq 0 (
    if %SCCM_DEPLOYMENT%==0 (
        echo.
        echo HIBA: A PowerShell script futtatása során hiba történt! (Kód: %errorlevel%)
        echo Kérjük, ellenõrizze a hibaüzeneteket és próbálja újra.
        echo A log fájl itt található: %DEST_PATH%\deployment_log.txt
        echo.
        pause
    ) else (
        echo HIBA: PowerShell telepítési hiba. (Kód: %errorlevel%) Log: %DEST_PATH%\deployment_log.txt
    )
    exit /b %errorlevel%
)

:: Check if deployment verification file exists
if exist "%DEST_PATH%\DEPLOYMENT_SUCCESS.txt" (
    if %SCCM_DEPLOYMENT%==0 (
        echo.
        echo A telepítés sikeresen befejezõdött!
        echo Az alkalmazás automatikusan elindul minden rendszerindításkor minden felhasználó számára.
        echo Az alkalmazás ikonja mostantól látható lesz a Windows rendszertálcán.
        echo.
        echo Köszönjük, hogy a MicLevelMonitor alkalmazást használja!
        echo.
        pause
    ) else (
        echo Telepítés sikeresen befejezve.
    )
    exit /b 0
) else (
    if %SCCM_DEPLOYMENT%==0 (
        echo.
        echo FIGYELMEZTETÉS: A telepítés befejezõdött, de ellenõrizze a log fájlokat esetleges problémák miatt.
        echo A log fájl itt található: %DEST_PATH%\deployment_log.txt
        echo.
        pause
    ) else (
        echo FIGYELMEZTETÉS: A telepítés befejezõdött, de hiányzik a megerõsítõ fájl. Ellenõrizze a logokat.
    )
    exit /b %ERROR_UNKNOWN%
)