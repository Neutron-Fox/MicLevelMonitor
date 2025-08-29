# Script to deploy MicLevelMonitor
# This script will:
# 1. Create a shortcut in the Public startup folder
# 2. Configure Windows 11 to show the MicLevelMonitor icon in the notification area for all users

# Configuration
$appName = "MicLevelMonitor"
$sourceFolder = "$env:SystemDrive\Ny�lt dokumentumok\$appName"
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

# Check if the application exists in the source folder
if (-not (Test-Path -Path $exePath)) {
    Write-Error "Az alkalmaz�s v�grehajthat� f�jlja nem tal�lhat�: $exePath"
    "APPLICATION_NOT_FOUND" | Out-File -FilePath (Join-Path -Path $sourceFolder -ChildPath "DEPLOYMENT_FAILED.txt") -Force
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
        
        Write-Host "Parancsikon l�trehozva: $ShortcutPath" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Error "Hiba t�rt�nt a parancsikon l�trehoz�sa k�zben: $_"
        return $false
    }
}

# Create startup directory if it doesn't exist
if (-not (Test-Path -Path $startupFolder)) {
    try {
        New-Item -Path $startupFolder -ItemType Directory -Force | Out-Null
        Write-Host "Ind�t�si mappa l�trehozva: $startupFolder" -ForegroundColor Green
    }
    catch {
        Write-Error "Nem siker�lt l�trehozni az ind�t�si mapp�t: $_"
    }
}

# Create a shortcut in the public startup folder
Write-Host "`nInd�t�si parancsikon l�trehoz�sa a k�vetkez�h�z: $appName..." -ForegroundColor Cyan
$shortcutCreated = Create-Shortcut -TargetPath $exePath -ShortcutPath $shortcutPath -Description "$appName - Mikrofon szintjelz�"

# First, start the application to register it in the system tray
if (-not (Get-Process -Name $appName -ErrorAction SilentlyContinue)) {
    Write-Host "`n$appName ind�t�sa a rendszert�lc�ban val� regisztr�l�shoz..." -ForegroundColor Cyan
    try {
        Start-Process -FilePath $exePath
        Start-Sleep -Seconds 3
        Write-Host "Alkalmaz�s sikeresen elind�tva" -ForegroundColor Green
    }
    catch {
        Write-Error "Nem siker�lt elind�tani az alkalmaz�st: $_"
    }
}

# Configure notification area settings for the MicLevelMonitor app for current user
Write-Host "`nRendszert�lca ikon be�ll�t�sa az aktu�lis felhaszn�l� sz�m�ra..." -ForegroundColor Cyan

# Define registry paths
$notificationAreaPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\NotificationAreaCustomization"

try {
    # Create notification area customization key if it doesn't exist
    if (-not (Test-Path $notificationAreaPath)) {
        New-Item -Path $notificationAreaPath -Force | Out-Null
    }
    
    # Wait for the application to register in the notification area
    Start-Sleep -Seconds 3
    
    # Check if the app has registered in the notification area
    $appIconFound = $false
    $registeredIcons = Get-ChildItem -Path $notificationAreaPath -ErrorAction SilentlyContinue | 
                        Where-Object { $_.PSChildName -notmatch "IconStreams|PastIconsStream" }
    
    foreach ($icon in $registeredIcons) {
        $iconPath = Get-ItemProperty -Path $icon.PSPath -Name "IconPath" -ErrorAction SilentlyContinue
        if ($iconPath -and $iconPath.IconPath -match $appName) {
            Write-Host "  MicLevelMonitor ikon megtal�lva, l�that�v� t�tele..." -ForegroundColor Yellow
            Set-ItemProperty -Path $icon.PSPath -Name "IsVisible" -Value 1 -Type DWord -Force
            $appIconFound = $true
        }
    }
    
    if (-not $appIconFound) {
        Write-Warning "  Az alkalmaz�s ikonja nem tal�lhat� a rendszert�lc�n. Pr�b�lja �jraind�tani az alkalmaz�st."
    }
    
    Write-Host "Az aktu�lis felhaszn�l� rendszert�lca be�ll�t�sai sikeresen friss�tve" -ForegroundColor Green
}
catch {
    Write-Error "Hiba t�rt�nt az aktu�lis felhaszn�l� rendszert�lca be�ll�t�sainak friss�t�se sor�n: $_"
}

# Create a scheduled task script for configuring the notification icon for new users
Write-Host "`n�temezett feladat l�trehoz�sa az �j felhaszn�l�k sz�m�ra..." -ForegroundColor Cyan
$taskName = "Configure-MicLevelMonitor-ForNewUser"

$taskScript = @"
# Script to configure MicLevelMonitor icon visibility in notification area
try {
    # Wait for Explorer to fully initialize
    Start-Sleep -Seconds 5
    
    # Start the MicLevelMonitor application if it's not running
    \$appPath = "$exePath"
    if (-not (Get-Process -Name "MicLevelMonitor" -ErrorAction SilentlyContinue)) {
        Start-Process -FilePath \$appPath
        Start-Sleep -Seconds 5
    }
    
    # Set the MicLevelMonitor icon to be visible
    \$notificationAreaPath = "HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\NotificationAreaCustomization"
    if (-not (Test-Path \$notificationAreaPath)) {
        New-Item -Path \$notificationAreaPath -Force | Out-Null
    }
    
    # Check all icon entries
    \$iconFound = \$false
    \$registeredIcons = Get-ChildItem -Path \$notificationAreaPath -ErrorAction SilentlyContinue | 
                        Where-Object { \$_.PSChildName -notmatch "IconStreams|PastIconsStream" }
    
    foreach (\$icon in \$registeredIcons) {
        \$iconPath = Get-ItemProperty -Path \$icon.PSPath -Name "IconPath" -ErrorAction SilentlyContinue
        if (\$iconPath -and \$iconPath.IconPath -match "MicLevelMonitor") {
            Set-ItemProperty -Path \$icon.PSPath -Name "IsVisible" -Value 1 -Type DWord -Force
            \$iconFound = \$true
        }
    }
    
    # If the icon wasn't found, try again after a delay
    if (-not \$iconFound) {
        Start-Sleep -Seconds 10
        \$registeredIcons = Get-ChildItem -Path \$notificationAreaPath -ErrorAction SilentlyContinue | 
                            Where-Object { \$_.PSChildName -notmatch "IconStreams|PastIconsStream" }
        
        foreach (\$icon in \$registeredIcons) {
            \$iconPath = Get-ItemProperty -Path \$icon.PSPath -Name "IconPath" -ErrorAction SilentlyContinue
            if (\$iconPath -and \$iconPath.IconPath -match "MicLevelMonitor") {
                Set-ItemProperty -Path \$icon.PSPath -Name "IsVisible" -Value 1 -Type DWord -Force
                \$iconFound = \$true
            }
        }
    }
    
    # Refresh Explorer to apply changes
    if (\$iconFound) {
        Stop-Process -Name "explorer" -Force -ErrorAction SilentlyContinue
        Start-Process "explorer.exe"
    }
} catch {
    # Error handling
    \$errorMessage = "Error configuring MicLevelMonitor icon: \$_"
    \$errorMessage | Out-File -FilePath "$sourceFolder\\NotificationIconError.log" -Append
}
"@

# Save the script to a file
$scriptPath = Join-Path -Path $sourceFolder -ChildPath "ConfigureNotificationIcon.ps1"
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
    Write-Host "�temezett feladat sikeresen l�trehozva: $taskName" -ForegroundColor Green
}
catch {
    Write-Error "Hiba t�rt�nt az �temezett feladat l�trehoz�sa sor�n: $_"
}

# Configure the Default User profile for future new users
Write-Host "`nAlap�rtelmezett felhaszn�l�i profil konfigur�l�sa (minden j�v�beli felhaszn�l� sz�m�ra)..." -ForegroundColor Cyan

try {
    if (Test-Path $defaultUserRegistryPath) {
        # Load default user hive
        $process = Start-Process -FilePath "reg.exe" -ArgumentList "load HKU\DefaultUser `"$defaultUserRegistryPath`"" -PassThru -Wait -WindowStyle Hidden
        
        if ($process.ExitCode -eq 0) {
            # Create the notification area customization key
            Start-Process -FilePath "reg.exe" -ArgumentList "add `"HKU\DefaultUser\Software\Microsoft\Windows\CurrentVersion\Explorer\NotificationAreaCustomization`"" -PassThru -Wait -WindowStyle Hidden | Out-Null
            
            # Unload the hive
            Start-Process -FilePath "reg.exe" -ArgumentList "unload HKU\DefaultUser" -PassThru -Wait -WindowStyle Hidden | Out-Null
            
            Write-Host "Az alap�rtelmezett felhaszn�l�i profil sikeresen el�k�sz�tve a MicLevelMonitor ikon sz�m�ra" -ForegroundColor Green
        }
        else {
            Write-Warning "Nem siker�lt bet�lteni az alap�rtelmezett felhaszn�l�i registry hive-ot. Kil�p�si k�d: $($process.ExitCode)"
        }
    } 
    else {
        Write-Warning "Az alap�rtelmezett felhaszn�l�i registry f�jl nem tal�lhat�: $defaultUserRegistryPath"
    }
}
catch {
    Write-Error "Hiba t�rt�nt az alap�rtelmezett felhaszn�l�i profil konfigur�l�sa sor�n: $_"
}

# Create instructions for manual configuration if automatic fails
Write-Host "`nManu�lis konfigur�ci�s �tmutat� l�trehoz�sa..." -ForegroundColor Cyan

$instructionsPath = Join-Path -Path $sourceFolder -ChildPath "ConfigureSystemTray.txt"
@"
Ha a MicLevelMonitor ikon nem lenne l�that� a t�lc�n, k�vesse az al�bbi l�p�seket:

1. Kattintson a Windows t�lc�n a felfel� mutat� ny�lra (^)
2. Kattintson a "Testreszab�s" gombra
3. A megjelen� Be�ll�t�sok oldalon keresse meg a "MicLevelMonitor" alkalmaz�st
4. �ll�tsa be a kapcsol�t "Megjelen�t�s" �rt�kre

Vagy:

1. Nyissa meg a Windows Be�ll�t�sokat
2. Navig�ljon a "Szem�lyre szab�s" > "T�lca" > "T�lcasarok ikonjai" men�pontba
3. Keresse meg a "MicLevelMonitor" alkalmaz�st �s �ll�tsa "Be" poz�ci�ba

Ha az alkalmaz�s nem indul el automatikusan rendszerind�t�skor:
- Ellen�rizze, hogy a parancsikon l�tezik-e a k�vetkez� mapp�ban:
  C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp
- Ha nem l�tezik, hozzon l�tre egy parancsikont a MicLevelMonitor.exe f�jlhoz
  �s helyezze el a fenti mapp�ban

Technikai probl�m�k eset�n forduljon a rendszergazd�hoz vagy a fejleszt�h�z.
"@ | Out-File -FilePath $instructionsPath -Encoding UTF8

# Create a deployment verification file
Write-Host "`nTelep�t�si ellen�rz� f�jl l�trehoz�sa..." -ForegroundColor Cyan
@"
Deployment Completed: $(Get-Date)
Computer Name: $env:COMPUTERNAME
Deployment User: $env:USERNAME
Shortcut Created: $shortcutCreated
Application Path: $exePath
"@ | Out-File -FilePath (Join-Path -Path $sourceFolder -ChildPath "DEPLOYMENT_SUCCESS.txt") -Encoding UTF8

Write-Host "`nTelep�t�s befejezve!" -ForegroundColor Green
Write-Host "Az alkalmaz�s hozz�adva az ind�t�si mapp�hoz" -ForegroundColor Green
Write-Host "A MicLevelMonitor ikonnak l�that�nak kell lennie a rendszert�lc�n minden felhaszn�l� sz�m�ra" -ForegroundColor Green
Write-Host "Egy �temezett feladat lett l�trehozva a rendszert�lca be�ll�t�sok konfigur�l�s�hoz az �j felhaszn�l�kn�l bejelentkez�skor" -ForegroundColor Green
Write-Host "Ha a rendszert�lca ikon tov�bbra sem l�that�, a manu�lis utas�t�sok a k�vetkez� helyen �rhet�ek el: $instructionsPath" -ForegroundColor Yellow

Stop-Transcript