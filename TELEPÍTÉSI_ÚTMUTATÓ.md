# MicLevelMonitor Telepítési Útmutató

Ez a dokumentáció segít a MicLevelMonitor alkalmazás SCCM-en keresztüli telepítésében és a Windows 11 rendszertálcán való megfelelõ megjelenítésének beállításában.

## A telepítési csomag tartalma

- **MicLevelMonitor alkalmazás** - Az alkalmazás fõ exe fájlja és a szükséges függõségek
- **Deploy-MicLevelMonitor.ps1** - PowerShell script az alkalmazás konfigurálásához
- **TELEPÍTÉSI_ÚTMUTATÓ.md** - Ez a telepítési útmutató

## SCCM telepítési lépések

1. Készítsen egy SCCM telepítési csomagot, amely az alábbi tartalmakkal rendelkezik:
   - Az alkalmazás teljes mappája
   - A Deploy-MicLevelMonitor.ps1 PowerShell script
   - Ez az útmutató

2. Az SCCM deployment során állítsa be az alábbi telepítési paramétereket:
   - **Telepítési mappa**: `C:\Nyílt dokumentumok\MicLevelMonitor`
   - **Futtatandó program**: `powershell.exe -ExecutionPolicy Bypass -File "Deploy-MicLevelMonitor.ps1"`

## Mit tesz a telepítõ script?

A `Deploy-MicLevelMonitor.ps1` script az alábbi mûveleteket hajtja végre:

1. **Indítási parancsikon létrehozása**:
   - Létrehoz egy parancsikont a MicLevelMonitor.exe alkalmazáshoz
   - Elhelyezi a parancsikont a publikus indítási mappában (`C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp`)
   - Ezzel biztosítja, hogy az alkalmazás minden felhasználó számára automatikusan elinduljon rendszerindításkor

2. **A MicLevelMonitor ikon megjelenítésének biztosítása a rendszertálcán**:
   - Elindítja az alkalmazást, hogy regisztrálja magát a rendszertálcán
   - Megkeresi a MicLevelMonitor ikont a rendszertálca beállításokban
   - Beállítja a MicLevelMonitor ikont "látható" állapotúra a jelenlegi felhasználó számára
   - Elõkészíti az alapértelmezett felhasználói profilt, hogy az új felhasználók számára is látható legyen az ikon
   - Létrehoz egy ütemezett feladatot, amely bejelentkezéskor ellenõrzi és biztosítja, hogy a MicLevelMonitor ikon látható legyen

3. **Biztonsági megoldás alkalmazása**:
   - Létrehoz egy útmutató szöveges fájlt arra az esetre, ha az automatikus beállítás nem mûködne
   - Ez részletes leírást tartalmaz a rendszertálca ikon manuális konfigurálásához

## Telepítés utáni ellenõrzés

A telepítés után ellenõrizze az alábbiakat:

1. A MicLevelMonitor alkalmazás megfelelõen elindult-e
2. A rendszertálcán látható-e az alkalmazás ikonja minden felhasználó számára
3. A `C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp` mappában megtalálható-e a MicLevelMonitor parancsikonja
4. Létrejött-e a "Configure-MicLevelMonitor-ForNewUser" nevû ütemezett feladat a rendszeren

## Manuális beállítás (ha szükséges)

Ha az automatikus rendszertálca beállítás nem mûködne, a felhasználók követhetik a `ConfigureSystemTray.txt` fájlban található utasításokat:

1. Kattintson a Windows tálcán a felfelé mutató nyílra (^)
2. Kattintson a "Testreszabás" gombra
3. A megjelenõ Beállítások oldalon keresse meg a "MicLevelMonitor" alkalmazást
4. Állítsa be a kapcsolót "Megjelenítés" értékre

Vagy:

1. Nyissa meg a Windows Beállításokat
2. Navigáljon a "Személyre szabás" > "Tálca" > "Tálcasarok ikonjai" menüpontba
3. Keresse meg a "MicLevelMonitor" alkalmazást és állítsa "Be" pozícióba

## Hibaelhárítás

- **Az alkalmazás nem indul el automatikusan**: Ellenõrizze, hogy a parancsikon létezik-e a Startup mappában és a hivatkozás helyes-e
- **Az alkalmazás ikonja nem látható**: Kövesse a manuális beállítási útmutatót
- **Az alkalmazás fut, de nem jelenik meg az ikon**: Indítsa újra a Windows Explorer folyamatot vagy indítsa újra a számítógépet
- **Új felhasználó számára nem látható az ikon**: Ellenõrizze, hogy a "Configure-MicLevelMonitor-ForNewUser" nevû ütemezett feladat létezik-e és engedélyezve van-e

## Támogatás

Technikai problémák esetén forduljon a rendszergazdához vagy a fejlesztõhöz