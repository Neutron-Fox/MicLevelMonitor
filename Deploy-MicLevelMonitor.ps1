# Script to deploy MicLevelMonitor
# This script will:
# 1. Create a shortcut in the Public startup folder
# 2. Configure Windows 11 to always show the MicLevelMonitor icon in the notification area for all users
# 3. Optimize for SCCM deployment

# Run with administrative privileges
if (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Warning "Ez a script adminisztrátori jogosultságokat igényel! Kérem, futtassa rendszergazdaként."
    Exit 1
}

# Configuration
$appName = "MicLevelMonitor"
$sourceFolder = "$env:SystemDrive\Nyílt dokumentumok\$appName"
$exePath = Join-Path -Path $sourceFolder -ChildPath "$appName.exe"
$startupFolder = "$env:ALLUSERSPROFILE\Microsoft\Windows\Start Menu\Programs\StartUp"
$shortcutPath = Join-Path -Path $startupFolder -ChildPath "$appName.lnk"
$defaultUserRegistryPath = "C:\Users\Default\NTUSER.DAT"
$logPath = Join-Path -Path $sourceFolder -ChildPath "deployment_log.txt"

# Start logging
Start-Transcript -Path $logPath -Append
Write-Host "===============================================" -ForegroundColor Yellow
Write-Host "MicLevelMonitor Deployment Script - $(Get-Date)" -ForegroundColor Yellow
Write-Host "===============================================" -ForegroundColor Yellow
Write-Host "Running on: $env:COMPUTERNAME as $env:USERNAME" -ForegroundColor Cyan
Write-Host "Source folder: $sourceFolder" -ForegroundColor Cyan
Write-Host "Executable path: $exePath" -ForegroundColor Cyan

# Check if running via SCCM
$isSCCM = $false
if (Get-Process -Name "CcmExec" -ErrorAction SilentlyContinue) {
    $isSCCM = $true
    Write-Host "Detected SCCM client running - executing in SCCM context" -ForegroundColor Cyan
}

# Check if the application exists in the source folder
if (-not (Test-Path -Path $exePath)) {
    Write-Error "Az alkalmazás végrehajtható fájlja nem található: $exePath"
    
    # If we're in SCCM context, create the directory and indicate a failure that SCCM can detect
    if ($isSCCM) {
        New-Item -Path $sourceFolder -ItemType Directory -Force -ErrorAction SilentlyContinue | Out-Null
        "APPLICATION_NOT_FOUND" | Out-File -FilePath (Join-Path -Path $sourceFolder -ChildPath "DEPLOYMENT_FAILED.txt") -Force
    }
    
    Stop-Transcript
    Exit 1
}

# Function to create shortcut
function Create-Shortcut {
    param (
        [string]$TargetPath,
        [string]$ShortcutPath,
        [string]$Description
    )
    
    try {
        $WshShell = New-Object -ComObject WScript.Shell
        $Shortcut = $WshShell.CreateShortcut($ShortcutPath)
        $Shortcut.TargetPath = $TargetPath
        $Shortcut.Description = $Description
        $Shortcut.WorkingDirectory = Split-Path -Path $TargetPath
        $Shortcut.Save()
        
        Write-Host "Parancsikon létrehozva: $ShortcutPath" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Error "Hiba történt a parancsikon létrehozása közben: $_"
        return $false
    }
}

# Create startup directory if it doesn't exist
if (-not (Test-Path -Path $startupFolder)) {
    try {
        New-Item -Path $startupFolder -ItemType Directory -Force | Out-Null
        Write-Host "Indítási mappa létrehozva: $startupFolder" -ForegroundColor Green
    }
    catch {
        Write-Error "Nem sikerült létrehozni az indítási mappát: $_"
    }
}

# Create a shortcut in the public startup folder
Write-Host "`nIndítási parancsikon létrehozása a következõhöz: $appName..." -ForegroundColor Cyan
$shortcutCreated = Create-Shortcut -TargetPath $exePath -ShortcutPath $shortcutPath -Description "$appName - Mikrofon szintjelzõ"

# Configure system tray icon to always be visible for current user
Write-Host "`nRendszertálca ikon konfigurálása az aktuális felhasználó számára..." -ForegroundColor Cyan

# Define registry paths
$explorerRegPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer"
$notificationAreaPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\NotificationAreaCustomization"

# Configure Windows 11 to show all notification icons for current user
try {
    if (-not (Test-Path $explorerRegPath)) {
        New-Item -Path $explorerRegPath -Force | Out-Null
    }

    $enableAutoTray = Get-ItemProperty -Path $explorerRegPath -Name "EnableAutoTray" -ErrorAction SilentlyContinue

    if ($null -eq $enableAutoTray) {
        New-ItemProperty -Path $explorerRegPath -Name "EnableAutoTray" -Value 0 -PropertyType DWord -Force | Out-Null
    } else {
        Set-ItemProperty -Path $explorerRegPath -Name "EnableAutoTray" -Value 0 -Type DWord -Force | Out-Null
    }
    
    # Make sure notification area customization path exists
    if (-not (Test-Path $notificationAreaPath)) {
        New-Item -Path $notificationAreaPath -Force | Out-Null
    }
    
    Write-Host "Az aktuális felhasználó rendszertálca beállításai sikeresen frissítve" -ForegroundColor Green
}
catch {
    Write-Error "Hiba történt az aktuális felhasználó rendszertálca beállításainak frissítése során: $_"
}

# First, run the app to make it register in the system tray if it's not running already
if (-not (Get-Process -Name $appName -ErrorAction SilentlyContinue)) {
    Write-Host "`n$appName indítása a rendszertálcában való regisztráláshoz..." -ForegroundColor Cyan
    try {
        Start-Process -FilePath $exePath
        Start-Sleep -Seconds 3
        Write-Host "Alkalmazás sikeresen elindítva" -ForegroundColor Green
    }
    catch {
        Write-Error "Nem sikerült elindítani az alkalmazást: $_"
    }
}

# Create a more robust scheduled task for new users
Write-Host "`nÜtemezett feladat létrehozása az új felhasználók számára..." -ForegroundColor Cyan
$taskName = "Configure-MicLevelMonitor-ForNewUser"

# Define a more comprehensive PowerShell script for the task
$taskScript = @"
# Configure notification area settings for MicLevelMonitor
`$explorerRegPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer"
`$notificationAreaPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\NotificationAreaCustomization"

# Ensure the Explorer key exists
if (-not (Test-Path `$explorerRegPath)) {
    New-Item -Path `$explorerRegPath -Force | Out-Null
}

# Disable auto-hide for system tray icons
if (Get-ItemProperty -Path `$explorerRegPath -Name "EnableAutoTray" -ErrorAction SilentlyContinue) {
    Set-ItemProperty -Path `$explorerRegPath -Name "EnableAutoTray" -Value 0 -Type DWord -Force
} else {
    New-ItemProperty -Path `$explorerRegPath -Name "EnableAutoTray" -Value 0 -PropertyType DWord -Force
}

# Create notification area customization key if it doesn't exist
if (-not (Test-Path `$notificationAreaPath)) {
    New-Item -Path `$notificationAreaPath -Force | Out-Null
}

# Also create specific settings for the MicLevelMonitor icon if it exists
`$iconGuid = `$null
Get-ChildItem -Path `$notificationAreaPath -ErrorAction SilentlyContinue | ForEach-Object {
    if (`$_.PSChildName -ne "IconStreams" -and `$_.PSChildName -ne "PastIconsStream") {
        `$iconPath = Get-ItemProperty -Path `$_.PSPath -Name "IconPath" -ErrorAction SilentlyContinue
        if (`$iconPath -and `$iconPath.IconPath -like "*MicLevelMonitor*") {
            Set-ItemProperty -Path `$_.PSPath -Name "IsVisible" -Value 1 -Type DWord -Force
        }
    }
}

# Refresh Explorer to apply changes immediately
Stop-Process -Name "explorer" -Force -ErrorAction SilentlyContinue
Start-Process "explorer.exe"
"@

# Save the script to a file
$scriptPath = Join-Path -Path $sourceFolder -ChildPath "ConfigureNotificationArea.ps1"
$taskScript | Out-File -FilePath $scriptPath -Encoding UTF8 -Force

# Create the scheduled task XML
$taskXml = @"
<?xml version="1.0" encoding="UTF-16"?>
<Task version="1.2" xmlns="http://schemas.microsoft.com/windows/2004/02/mit/task">
  <RegistrationInfo>
    <Description>Configure MicLevelMonitor tray icon visibility for user profiles</Description>
    <URI>\Configure-MicLevelMonitor-ForNewUser</URI>
  </RegistrationInfo>
  <Triggers>
    <LogonTrigger>
      <Enabled>true</Enabled>
    </LogonTrigger>
  </Triggers>
  <Principals>
    <Principal id="Author">
      <GroupId>S-1-5-32-545</GroupId>
      <RunLevel>LeastPrivilege</RunLevel>
    </Principal>
  </Principals>
  <Settings>
    <MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>
    <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>
    <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>
    <AllowHardTerminate>true</AllowHardTerminate>
    <StartWhenAvailable>false</StartWhenAvailable>
    <RunOnlyIfNetworkAvailable>false</RunOnlyIfNetworkAvailable>
    <IdleSettings>
      <StopOnIdleEnd>false</StopOnIdleEnd>
      <RestartOnIdle>false</RestartOnIdle>
    </IdleSettings>
    <AllowStartOnDemand>true</AllowStartOnDemand>
    <Enabled>true</Enabled>
    <Hidden>false</Hidden>
    <RunOnlyIfIdle>false</RunOnlyIfIdle>
    <WakeToRun>false</WakeToRun>
    <ExecutionTimeLimit>PT10M</ExecutionTimeLimit>
    <Priority>7</Priority>
  </Settings>
  <Actions Context="Author">
    <Exec>
      <Command>PowerShell.exe</Command>
      <Arguments>-ExecutionPolicy Bypass -WindowStyle Hidden -File "$scriptPath"</Arguments>
    </Exec>
  </Actions>
</Task>
"@

try {
    # Remove existing task if present
    $existingTask = Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue
    if ($null -ne $existingTask) {
        Unregister-ScheduledTask -TaskName $taskName -Confirm:$false
    }
    
    # Register the new task
    Register-ScheduledTask -TaskName $taskName -Xml $taskXml -Force | Out-Null
    Write-Host "Ütemezett feladat sikeresen létrehozva: $taskName" -ForegroundColor Green
}
catch {
    Write-Error "Hiba történt az ütemezett feladat létrehozása során: $_"
}

# Configure the Default User profile (for all future users)
Write-Host "`nAlapértelmezett felhasználói profil konfigurálása (minden jövõbeli felhasználó számára)..." -ForegroundColor Cyan

try {
    if (Test-Path $defaultUserRegistryPath) {
        # Load default user hive
        $process = Start-Process -FilePath "reg.exe" -ArgumentList "load HKU\DefaultUser `"$defaultUserRegistryPath`"" -PassThru -Wait -WindowStyle Hidden
        
        if ($process.ExitCode -eq 0) {
            # Set EnableAutoTray to 0 in default user profile
            Start-Process -FilePath "reg.exe" -ArgumentList "add `"HKU\DefaultUser\Software\Microsoft\Windows\CurrentVersion\Explorer`" /v EnableAutoTray /t REG_DWORD /d 0 /f" -PassThru -Wait -WindowStyle Hidden | Out-Null
            
            # Create the notification area customization key
            Start-Process -FilePath "reg.exe" -ArgumentList "add `"HKU\DefaultUser\Software\Microsoft\Windows\CurrentVersion\Explorer\NotificationAreaCustomization`"" -PassThru -Wait -WindowStyle Hidden | Out-Null
            
            # Unload the hive
            Start-Process -FilePath "reg.exe" -ArgumentList "unload HKU\DefaultUser" -PassThru -Wait -WindowStyle Hidden | Out-Null
            
            Write-Host "Az alapértelmezett felhasználói profil sikeresen konfigurálva az összes értesítési ikon megjelenítésére" -ForegroundColor Green
        }
        else {
            Write-Warning "Nem sikerült betölteni az alapértelmezett felhasználói registry hive-ot. Kilépési kód: $($process.ExitCode)"
        }
    } 
    else {
        Write-Warning "Az alapértelmezett felhasználói registry fájl nem található: $defaultUserRegistryPath"
    }
}
catch {
    Write-Error "Hiba történt az alapértelmezett felhasználói profil konfigurálása során: $_"
}

# Configure all existing user profiles
Write-Host "`nMinden létezõ felhasználói profil konfigurálása..." -ForegroundColor Cyan

try {
    # Get all user profiles except Default and Public
    $userProfiles = Get-ChildItem -Path "C:\Users" -Directory | Where-Object { $_.Name -ne "Default" -and $_.Name -ne "Public" -and $_.Name -ne "All Users" -and $_.Name -ne "Default User" }
    
    foreach ($profile in $userProfiles) {
        $ntUserDatPath = Join-Path -Path $profile.FullName -ChildPath "NTUSER.DAT"
        
        if (Test-Path $ntUserDatPath) {
            Write-Host "Felhasználói profil feldolgozása: $($profile.Name)" -ForegroundColor Yellow
            
            try {
                # Check if the user is currently logged in
                $isUserLoggedIn = $false
                $loggedInUsers = Get-WmiObject -Class Win32_ComputerSystem | Select-Object -ExpandProperty UserName
                if ($loggedInUsers -like "*\$($profile.Name)") {
                    $isUserLoggedIn = $true
                    Write-Host "  A felhasználó jelenleg be van jelentkezve - registry közvetlenül nem módosítható" -ForegroundColor Yellow
                    continue
                }
                
                # Load user hive
                $hiveKey = "HKU\TempUser_$($profile.Name)"
                $process = Start-Process -FilePath "reg.exe" -ArgumentList "load `"$hiveKey`" `"$ntUserDatPath`"" -PassThru -Wait -WindowStyle Hidden
                
                if ($process.ExitCode -eq 0) {
                    # Set EnableAutoTray to 0
                    Start-Process -FilePath "reg.exe" -ArgumentList "add `"$hiveKey\Software\Microsoft\Windows\CurrentVersion\Explorer`" /v EnableAutoTray /t REG_DWORD /d 0 /f" -PassThru -Wait -WindowStyle Hidden | Out-Null
                    
                    # Create the notification area customization key
                    Start-Process -FilePath "reg.exe" -ArgumentList "add `"$hiveKey\Software\Microsoft\Windows\CurrentVersion\Explorer\NotificationAreaCustomization`"" -PassThru -Wait -WindowStyle Hidden | Out-Null
                    
                    # Unload the hive
                    Start-Process -FilePath "reg.exe" -ArgumentList "unload `"$hiveKey`"" -PassThru -Wait -WindowStyle Hidden | Out-Null
                    
                    Write-Host "  A profil sikeresen konfigurálva: $($profile.Name)" -ForegroundColor Green
                }
                else {
                    Write-Warning "  Nem sikerült betölteni a felhasználói registry hive-ot: $($profile.Name). Kilépési kód: $($process.ExitCode)"
                }
            }
            catch {
                Write-Warning "  Hiba történt a profil konfigurálása során: $($profile.Name): $_"
            }
        }
        else {
            Write-Warning "  NTUSER.DAT nem található a következõ felhasználói profilhoz: $($profile.Name)"
        }
    }
}
catch {
    Write-Error "Hiba történt a felhasználói profilok feldolgozása során: $_"
}

# Create instructions for manual configuration if automatic fails
Write-Host "`nManuális konfigurációs útmutató létrehozása..." -ForegroundColor Cyan

$instructionsPath = Join-Path -Path $sourceFolder -ChildPath "ConfigureSystemTray.txt"
@"
Ha az ikon nem lenne látható a tálcán, kövesse az alábbi lépéseket:

1. Kattintson a Windows tálcán a felfelé mutató nyílra (^)
2. Kattintson a "Testreszabás" gombra
3. A megjelenõ Beállítások oldalon keresse meg a "MicLevelMonitor" alkalmazást
4. Állítsa be a kapcsolót "Megjelenítés" értékre

Vagy:

1. Nyissa meg a Windows Beállításokat
2. Navigáljon a "Személyre szabás" > "Tálca" > "Tálcasarok ikonjai" menüpontba
3. Keresse meg a "MicLevelMonitor" alkalmazást és állítsa "Be" pozícióba

Ha az alkalmazás nem indul el automatikusan rendszerindításkor:
- Ellenõrizze, hogy a parancsikon létezik-e a következõ mappában:
  C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp
- Ha nem létezik, hozzon létre egy parancsikont a MicLevelMonitor.exe fájlhoz
  és helyezze el a fenti mappában

Technikai problémák esetén forduljon a rendszergazdához vagy a fejlesztõhöz.
"@ | Out-File -FilePath $instructionsPath -Encoding UTF8

# Create a deployment verification file
Write-Host "`nTelepítési ellenõrzõ fájl létrehozása..." -ForegroundColor Cyan
@"
Deployment Completed: $(Get-Date)
Computer Name: $env:COMPUTERNAME
Deployment User: $env:USERNAME
Shortcut Created: $shortcutCreated
Application Path: $exePath
"@ | Out-File -FilePath (Join-Path -Path $sourceFolder -ChildPath "DEPLOYMENT_SUCCESS.txt") -Encoding UTF8

Write-Host "`nTelepítés befejezve!" -ForegroundColor Green
Write-Host "Az alkalmazás hozzáadva az indítási mappához és konfigurálva, hogy mindig látható legyen a rendszertálcán." -ForegroundColor Green
Write-Host "Egy ütemezett feladat lett létrehozva a rendszertálca beállítások konfigurálásához az új felhasználóknál bejelentkezéskor." -ForegroundColor Green
Write-Host "Ha a rendszertálca ikon továbbra sem látható, a manuális utasítások a következõ helyen érhetõek el: $instructionsPath" -ForegroundColor Yellow

Stop-Transcript