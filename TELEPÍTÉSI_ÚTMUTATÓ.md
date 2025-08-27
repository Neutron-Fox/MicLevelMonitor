# MicLevelMonitor Telepítési Útmutató

Ez a dokumentáció segít a MicLevelMonitor alkalmazás SCCM-en keresztüli telepítésében és a Windows 11 rendszertálcán való megfelelõ megjelenítésének beállításában.

## A telepítési csomag tartalma

- **MicLevelMonitor alkalmazás** - Az alkalmazás fõ exe fájlja és a szükséges függõségek
- **Deploy-MicLevelMonitor.ps1** - PowerShell script az alkalmazás konfigurálásához
- **Setup-MicLevelMonitor.bat** - Batch fájl a PowerShell script adminisztrátori jogosultsággal való futtatásához
- **TELEPÍTÉSI_ÚTMUTATÓ.md** - Ez a telepítési útmutató

## SCCM telepítési lépések

1. Készítsen egy SCCM telepítési csomagot, amely az alábbi tartalmakkal rendelkezik:
   - Az alkalmazás teljes mappája
   - A telepítõ scriptek (Deploy-MicLevelMonitor.ps1 és Setup-MicLevelMonitor.bat)
   - Ez az útmutató

2. Az SCCM deployment során állítsa be az alábbi telepítési paramétereket:
   - **Telepítési mappa**: `C:\Nyílt dokumentumok\MicLevelMonitor`
   - **Futtatandó program**: `Setup-MicLevelMonitor.bat`
   - **Futtatás módja**: Adminisztrátori jogosultsággal

## Mit tesz a telepítõ script?

A `Setup-MicLevelMonitor.bat` és a `Deploy-MicLevelMonitor.ps1` scriptek az alábbi mûveleteket hajtják végre:

1. **Indítási parancsikon létrehozása**:
   - Létrehoz egy parancsikont a MicLevelMonitor.exe alkalmazáshoz
   - Elhelyezi a parancsikont a publikus indítási mappában (`C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp`)
   - Ezzel biztosítja, hogy az alkalmazás minden felhasználó számára automatikusan elinduljon rendszerindításkor

2. **Rendszertálca beállítások konfigurálása**:
   - Beállítja a Windows 11 értesítési területének konfigurációját
   - Az alkalmazás rendszertálca ikonját mindig láthatóvá teszi
   - Ez megakadályozza, hogy az ikon a "rejtett ikonok" közé kerüljön

3. **Biztonsági megoldás alkalmazása**:
   - Létrehoz egy útmutató szöveges fájlt arra az esetre, ha az automatikus beállítás nem mûködne
   - Ez részletes leírást tartalmaz a rendszertálca ikon manuális konfigurálásához

## Telepítés utáni ellenõrzés

A telepítés után ellenõrizze az alábbiakat:

1. A MicLevelMonitor alkalmazás megfelelõen elindult-e
2. A rendszertálcán látható-e az alkalmazás ikonja
3. A `C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp` mappában megtalálható-e a MicLevelMonitor parancsikonja

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

## Támogatás

Technikai problémák esetén forduljon a rendszergazdához vagy a fejlesztõhöz.