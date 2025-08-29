# MicLevelMonitor SCCM Telepítési Útmutató

Ez a részletes útmutató segít a MicLevelMonitor alkalmazás SCCM-en keresztüli telepítésében, különös tekintettel arra, hogy az alkalmazás ikonja minden felhasználó számára látható legyen a Windows 11 értesítési területén (rendszertálcán).

## 1. SCCM telepítési módszer kiválasztása

### Alkalmazás vagy Csomag?

**Ajánlás: SCCM Alkalmazás (Application)**

Az SCCM Alkalmazás modell jobb választás a MicLevelMonitor telepítéséhez a következõ elõnyök miatt:
- Jobb követhetõség és állapotjelentések
- Egyszerûbb kezelés és frissítés
- Részletesebb telepítési feltételek és követelmények beállíthatósága
- Felhasználói élmény szabályozhatósága (csendes telepítés)
- Hatékonyabb függõség- és verziószabályozás

## 2. Alkalmazás telepítési csomag létrehozása

### 2.1. Telepítési fájlok elõkészítése

1. Készítse elõ a következõ fájlokat egy megosztott hálózati mappában vagy a ConfigMgr forráskönyvtárában:
   - A teljes MicLevelMonitor alkalmazás mappa, amely tartalmazza:
     - MicLevelMonitor.exe
     - Függõségek (DLL-ek, konfigurációs fájlok)
   - Setup-MicLevelMonitor.bat
   - Deploy-MicLevelMonitor.ps1
   - TELEPÍTÉSI_ÚTMUTATÓ.md és/vagy ezt a dokumentumot

2. Gyõzõdjön meg róla, hogy a fájlok helyesen vannak beállítva és az exe fájl mûködik.

### 2.2. Alkalmazás létrehozása az SCCM konzolban

1. Nyissa meg az SCCM konzolt.
2. Navigáljon a **Software Library** > **Application Management** > **Applications** menüponthoz.
3. Jobb gombbal kattintson az **Applications** elemre, és válassza a **Create Application** lehetõséget.
4. Válassza a **Manually specify the application information** opciót, majd kattintson a **Next** gombra.

5. Adja meg az alkalmazás adatait:
   - **Name**: MicLevelMonitor
   - **Publisher**: [Az Ön szervezete]
   - **Software version**: [Alkalmazás verziószáma]
   - **Installation program visibility**: Hidden (Rejtett)
   - Egyéb mezõk kitöltése szükség szerint
   - Kattintson a **Next** gombra.

6. Folytassa a varázsló lépéseit, majd a befejezéshez kattintson a **Next**, majd a **Close** gombra.

### 2.3. Telepítési típus létrehozása

1. A létrehozott alkalmazás tulajdonságaiban kattintson a **Deployment Types** fülre.
2. Jobb gombbal kattintson a területre, és válassza az **Add Deployment Type** lehetõséget.
3. Válassza a **Script Installer** opciót, majd kattintson a **Next** gombra.

4. Adja meg a telepítési típus adatait:
   - **Name**: MicLevelMonitor Installer
   - **Content location**: [A telepítési fájlokat tartalmazó hálózati mappa elérési útja]
   - **Installation program**: `Setup-MicLevelMonitor.bat`
   - **Installation start in**: `%CONTENTLOCATIONPATH%`
   - **Uninstall program**: (hagyja üresen, vagy adjon meg eltávolító parancsot, ha szükséges)
   - **Detection method**: Állítson be megfelelõ észlelési szabályt:
     - Fájl észlelés: `C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp\MicLevelMonitor.lnk`
     - Vagy regisztrációs kulcs észlelés: `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MicLevelMonitor`

5. **Requirements** fülön adja meg:
   - Operációs rendszer: Windows 10/Windows 11
   - Egyéb követelmények szükség szerint

6. Kattintson a **Next** gombra a további lépéseken keresztül, majd a befejezéshez kattintson a **Close** gombra.

## 3. Telepítés terjesztése (Deployment)

### 3.1. Terjesztés létrehozása

1. Az SCCM konzolban navigáljon a **Software Library** > **Application Management** > **Applications** menüponthoz.
2. Jelölje ki a létrehozott MicLevelMonitor alkalmazást.
3. Jobb gombbal kattintson rá és válassza a **Deploy** lehetõséget.

4. Adja meg a terjesztés céladatait:
   - **Collection**: Válassza ki a megfelelõ eszközgyûjteményt, amelyekre telepíteni szeretné
   - Kattintson a **Next** gombra.

5. Adja meg a tartalom forrását (Content):
   - Válassza ki a megfelelõ disztribúciós pontot vagy disztribúciós pont csoportot
   - Kattintson a **Next** gombra.

6. Adja meg a telepítés beállításait:
   - **Purpose**: Available (választható) vagy Required (kötelezõ)
   - **Make this task sequence available to the following**: Configuration Manager Clients
   - **Schedule**: Állítsa be a kívánt telepítési ütemezést
   - Kattintson a **Next** gombra.

7. Deployment beállítások:
   - **User experience**: Hidden (ajánlott)
   - **Software installation**: Allow installations (engedélyezve)
   - **System restart**: Suppress restarts on servers / workstations
   - **Write filter handling for Windows Embedded devices**: Commit changes at deadline or maintenance windows
   - Kattintson a **Next** gombra.

8. Állítsa be a figyelmeztetéseket, majd kattintson a **Next** gombra.
9. Ellenõrizze a beállításokat, és kattintson a **Next** gombra a terjesztés létrehozásához.
10. Kattintson a **Close** gombra a befejezéshez.

## 4. A telepítés folyamata

Az SCCM kliens a következõ mûveleteket hajtja végre:

### 4.1. Tartalom letöltése és telepítése

1. **Tartalom letöltése**: Az SCCM kliens letölti a MicLevelMonitor alkalmazást és a telepítési scripteket a disztribúciós pontról a helyi cache-be.

2. **Cél mappa létrehozása**: Az SCCM létrehozza a célmappát: `C:\Nyílt dokumentumok\MicLevelMonitor`.

3. **Fájlok másolása**: Az SCCM átmásolja az összes alkalmazásfájlt a célmappába.

### 4.2. Telepítõ script futtatása

1. **Adminisztrátori jogosultságok**: Az SCCM a **Setup-MicLevelMonitor.bat** fájlt rendszergazdai jogosultsággal futtatja.

2. **PowerShell script indítása**: A batch fájl elindítja a **Deploy-MicLevelMonitor.ps1** PowerShell scriptet.

### 4.3. A PowerShell script mûködése

A PowerShell script a következõ mûveleteket hajtja végre:

1. **Indítási parancsikon létrehozása**:
   - Létrehoz egy parancsikont a MicLevelMonitor.exe alkalmazáshoz
   - Elhelyezi ezt a parancsikont a publikus indítási mappában: `C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp`
   - Ez biztosítja, hogy az alkalmazás minden felhasználó számára automatikusan elinduljon a rendszerindításkor

2. **Az aktuális felhasználó beállításainak konfigurálása**:
   - Módosítja a registry beállításokat, hogy a rendszertálca ikonok mindig láthatóak legyenek
   - Beállítja az `EnableAutoTray` értéket 0-ra a `HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer` útvonalon
   - Létrehozza a `NotificationAreaCustomization` registry kulcsot, ha még nem létezik

3. **Az alapértelmezett felhasználói profil konfigurálása**:
   - Betölti a `C:\Users\Default\NTUSER.DAT` fájlt
   - Beállítja az `EnableAutoTray` értéket 0-ra az alapértelmezett profilban
   - Létrehozza a szükséges registry kulcsokat
   - Kirakja a registry hive-ot

4. **Minden létezõ felhasználói profil konfigurálása**:
   - Megkeresi az összes felhasználói profilt a C:\Users mappában
   - Minden felhasználónál beállítja az `EnableAutoTray` értéket 0-ra
   - Létrehozza a szükséges registry kulcsokat

5. **Ütemezett feladat létrehozása új felhasználóknak**:
   - Létrehoz egy `Configure-MicLevelMonitor-ForNewUser` nevû ütemezett feladatot
   - Az ütemezett feladat bejelentkezéskor fut le minden felhasználónál
   - A feladat ugyanazokat a registry beállításokat alkalmazza, hogy az ikon látható legyen

6. **Útmutató létrehozása**:
   - Létrehoz egy `ConfigureSystemTray.txt` nevû szöveges fájlt
   - A fájl tartalmazza a manuális beállítási lehetõségeket arra az esetre, ha az automatikus beállítás nem mûködne

## 5. Telepítés utáni ellenõrzés

Az SCCM telepítés után a következõket érdemes ellenõrizni:

1. **Indítási parancsikon**: A `C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp` mappában található-e a MicLevelMonitor parancsikonja.

2. **Alkalmazás mûködése**: A MicLevelMonitor alkalmazás megfelelõen elindul-e a rendszerindításkor.

3. **Rendszertálca ikon**: A rendszertálcán látható-e az alkalmazás ikonja minden felhasználó számára.

4. **Ütemezett feladat**: Létrejött-e a "Configure-MicLevelMonitor-ForNewUser" nevû ütemezett feladat a rendszeren.

## 6. Hibaelhárítás

Ha problémák merülnek fel a telepítés során:

### 6.1. Alkalmazás nem indul el

- Ellenõrizze, hogy a parancsikon létezik-e a Startup mappában.
- Ellenõrizze, hogy a hivatkozás helyes-e és a célalkalmazás létezik.
- Nézze meg az eseménynaplókat a hibákért.

### 6.2. Az ikon nem látható a rendszertálcán

- Próbálja meg manuálisan beállítani a rendszertálca ikont a `ConfigureSystemTray.txt` fájlban leírt módon.
- Ellenõrizze, hogy az alkalmazás fut-e (Task Manager).
- Indítsa újra a Windows Explorer folyamatot vagy a számítógépet.

### 6.3. SCCM telepítési hibák

- Ellenõrizze az SCCM kliens naplófájlokat a `C:\Windows\CCM\Logs` mappában.
- Futtassa manuálisan a telepítõ scriptet a hibaüzenet megtekintéséhez.
- Ellenõrizze a jogosultságokat és a hálózati kapcsolatot.

## 7. Támogatás

Technikai problémák esetén forduljon a rendszergazdához vagy a fejlesztõhöz.