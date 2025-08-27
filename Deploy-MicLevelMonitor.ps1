# Script to deploy MicLevelMonitor
# This script will:
# 1. Create a shortcut in the Public startup folder
# 2. Configure Windows 11 to always show the MicLevelMonitor icon in the notification area

# Run with administrative privileges
if (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Warning "Please run this script as Administrator!"
    Exit
}

# Configuration
$appName = "MicLevelMonitor"
$sourceFolder = "$env:SystemDrive\Nyílt dokumentumok\$appName"
$exePath = Join-Path -Path $sourceFolder -ChildPath "$appName.exe"
$startupFolder = "$env:ALLUSERSPROFILE\Microsoft\Windows\Start Menu\Programs\StartUp"
$shortcutPath = Join-Path -Path $startupFolder -ChildPath "$appName.lnk"

# Check if the application exists in the source folder
if (-not (Test-Path -Path $exePath)) {
    Write-Error "Application executable not found at: $exePath"
    Exit 1
}

# Function to create shortcut
function Create-Shortcut {
    param (
        [string]$TargetPath,
        [string]$ShortcutPath,
        [string]$Description
    )

    $WshShell = New-Object -ComObject WScript.Shell
    $Shortcut = $WshShell.CreateShortcut($ShortcutPath)
    $Shortcut.TargetPath = $TargetPath
    $Shortcut.Description = $Description
    $Shortcut.WorkingDirectory = Split-Path -Path $TargetPath
    $Shortcut.Save()
    
    Write-Host "Shortcut created at: $ShortcutPath" -ForegroundColor Green
}

# Create a shortcut in the public startup folder
Write-Host "Creating startup shortcut for $appName..." -ForegroundColor Cyan
Create-Shortcut -TargetPath $exePath -ShortcutPath $shortcutPath -Description "$appName - Mikrofon szintjelzõ"

# Registry path for system tray icons
$registryPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\NotificationAreaCustomization\{0}"
$iconGuid = $null

# Function to modify registry to show system tray icon
function Configure-SystemTrayIcon {
    # First, run the app to make it register in the system tray
    if (-not (Get-Process -Name $appName -ErrorAction SilentlyContinue)) {
        Write-Host "Starting $appName to register it in the system tray..." -ForegroundColor Cyan
        Start-Process -FilePath $exePath
        Start-Sleep -Seconds 3
    }
    
    # Get all notification area customization GUIDs
    $naRegPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\NotificationAreaCustomization"
    if (-not (Test-Path -Path $naRegPath)) {
        Write-Warning "Notification area registry path not found. Windows may be using a different registry structure."
        return
    }
    
    # Find the app's icon GUID
    Get-ChildItem -Path $naRegPath | ForEach-Object {
        $guidKey = $_.PSChildName
        if ($guidKey -ne "IconStreams" -and $guidKey -ne "PastIconsStream") {
            $iconPath = Get-ItemProperty -Path $_.PSPath -Name "IconPath" -ErrorAction SilentlyContinue
            if ($iconPath -and $iconPath.IconPath -like "*$appName*") {
                $script:iconGuid = $guidKey
                Write-Host "Found $appName system tray icon with GUID: $iconGuid" -ForegroundColor Green
                
                # Set the icon to always show
                $fullRegPath = $registryPath -f $iconGuid
                if (Test-Path -Path $fullRegPath) {
                    Set-ItemProperty -Path $fullRegPath -Name "IsVisible" -Value 1 -Type DWord
                    Write-Host "System tray icon set to always visible" -ForegroundColor Green
                }
            }
        }
    }
    
    # If icon not found, wait a bit and try again
    if (-not $iconGuid) {
        Write-Host "Icon not found in registry. Trying alternative approach..." -ForegroundColor Yellow
        
        # Alternative approach for Windows 11
        $trayIconsPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\TrayNotify"
        if (Test-Path -Path $trayIconsPath) {
            Write-Host "Configuring notification preferences for all icons..." -ForegroundColor Cyan
            
            # Configure Windows to show all notification icons
            $explorerRegPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer"
            $enableAutoTray = Get-ItemProperty -Path $explorerRegPath -Name "EnableAutoTray" -ErrorAction SilentlyContinue
            
            if ($null -eq $enableAutoTray) {
                New-ItemProperty -Path $explorerRegPath -Name "EnableAutoTray" -Value 0 -PropertyType DWord -Force
            } else {
                Set-ItemProperty -Path $explorerRegPath -Name "EnableAutoTray" -Value 0 -Type DWord -Force
            }
            
            Write-Host "Configured Windows to show all notification icons" -ForegroundColor Green
        }
    }
}

# Configure system tray icon to always be visible
Write-Host "Configuring system tray icon for $appName..." -ForegroundColor Cyan
Configure-SystemTrayIcon

# Create instructions for manual configuration if automatic fails
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
"@ | Out-File -FilePath $instructionsPath -Encoding UTF8

Write-Host "`nDeployment complete!" -ForegroundColor Green
Write-Host "The application has been added to startup and configured to always show in the system tray." -ForegroundColor Green
Write-Host "If the system tray icon is still not visible, manual instructions are available at: $instructionsPath" -ForegroundColor Yellow