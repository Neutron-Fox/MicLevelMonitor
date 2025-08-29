# MicLevelMonitor SCCM Telepítési Lépések

## Összefoglaló

Ez a dokumentum egy egyszerû, lépésenkénti útmutatót tartalmaz a MicLevelMonitor alkalmazás SCCM-en keresztüli telepítéséhez, különös tekintettel arra, hogy a rendszertálca ikon minden felhasználó számára látható legyen.

## 1. SCCM Alkalmazás létrehozása

SCCM-ben **Application** típusú telepítést kell használni **Package** helyett, mivel az Application modell több elõnnyel rendelkezik:
- Jobb állapotjelentések és követhetõség
- Egyszerûbb telepítési függõségek kezelése
- Fejlettebb felhasználói élmény szabályozás

## 2. Telepítési fájlok elõkészítése

1. A telepítõ csomag tartalma:
   - MicLevelMonitor alkalmazás mappa (exe és függõségek)
   - Deploy-MicLevelMonitor.ps1
   - Setup-MicLevelMonitor.bat
   - Telepítési dokumentáció

2. Telepítés elõtt ellenõrizze, hogy minden fájl megfelelõen mûködik.

## 3. SCCM Alkalmazás konfigurációja

### 3.1 Alkalmazás létrehozása
1. SCCM konzol > Software Library > Applications
2. Create Application > Manually specify the application information
3. Adja meg: MicLevelMonitor, verziószám, kiadó, stb.

### 3.2 Telepítési típus hozzáadása
1. Az új alkalmazásnál > Deployment Types > Add > Script Installer
2. Konfiguráció:
   - **Installation program**: `Setup-MicLevelMonitor.bat`
   - **Installation start in**: `%CONTENTLOCATIONPATH%`
   - **Content location**: [Hálózati megosztás útvonala]
   - **Detection method**: `C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp\MicLevelMonitor.lnk` fájl létezése

### 3.3 Telepítési követelmények
- Windows 10/11 operációs rendszer
- 64-bites architektúra

## 4. Terjesztés konfigurálása

1. Új Deployment létrehozása a MicLevelMonitor alkalmazáshoz
2. Célgyûjtemény kiválasztása
3. Beállítások:
   - **Purpose**: Required
   - **Deployment settings**: Install for system if resource is device; Install for user if resource is user
   - **Schedule**: Megfelelõ telepítési idõpont kiválasztása
   - **User experience**: Hidden / Normal
   - **Allow end users to interact with the program installation**: No

## 5. A telepítési folyamat

### Mi történik a kliens számítógépeken:

1. **Tartalom letöltése**: Az SCCM letölti az alkalmazásfájlokat a megosztott hálózati helyrõl
2. **Telepítési hely létrehozása**: `C:\Nyílt dokumentumok\MicLevelMonitor`
3. **Script futtatása**: `Setup-MicLevelMonitor.bat` -> `Deploy-MicLevelMonitor.ps1`

### A PowerShell script végrehajtja:

1. **Indítási parancsikon létrehozása**:
   - Létrehoz egy parancsikont a `C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp` mappában
   - Ez minden felhasználó számára elindítja az alkalmazást rendszerindításkor

2. **Rendszertálca beállítások konfigurálása**:
   - Az aktuális felhasználó beállításai
   - Az alapértelmezett felhasználói profil beállításai (jövõbeli új felhasználóknak)
   - Az összes létezõ felhasználói profil beállításai
   - Ütemezett feladat létrehozása, amely a bejelentkezéskor frissíti a beállításokat

3. **Visszajelzés**:
   - Log fájlt hoz létre a telepítési mappában
   - Sikeres telepítés esetén létrehoz egy DEPLOYMENT_SUCCESS.txt fájlt
   - Hiba esetén hibakódot ad vissza az SCCM-nek

## 6. Ellenõrzési pontok

A sikeres telepítés után ellenõrizze:

1. ? A `C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp` mappában található MicLevelMonitor parancsikon
2. ? Az alkalmazás automatikusan elindul a rendszerindításkor
3. ? A rendszertálcán megjelenik az alkalmazás ikonja minden felhasználó számára
4. ? Létrejött a "Configure-MicLevelMonitor-ForNewUser" ütemezett feladat

## 7. Hibaelhárítás

Problémák esetén:
- Ellenõrizze a log fájlt: `C:\Nyílt dokumentumok\MicLevelMonitor\deployment_log.txt`
- Próbálja manuálisan futtatni a telepítõ scripteket
- Manuális beállításhoz használja a `ConfigureSystemTray.txt` fájlban leírt lépéseket