---- Mikrofon hang érzékelő .NET8 ----

** Win-x64 verzió, tálca ikonos program **

## Projekt fájlok

### 📦 MicLevelMonitor.csproj 
**INTEGRÁLT verzió** (.NET runtime benne van)
```
dotnet publish MicLevelMonitor.csproj -c Release
```

### 📦 MicLevelMonitorUnintegrated.csproj 
**FRAMEWORK FÜGGŐ verzió** (.NET runtime külön kell)
```
dotnet publish MicLevelMonitorUnintegrated.csproj -c Release
```

## Verzió különbségek

| **Funkció** | **Integrated** | **Unintegrated** |
|-------------|----------------|------------------|
| .NET Runtime | ✅ Benne van | ❌ Külön kell |
| Fájlméret | ~150 MB | ~10 MB |
| Telepítés | Önálló | .NET 8 szükséges |
| Kompatibilitás | Univerzális | .NET 8+ |
| Project fájl | `MicLevelMonitor.csproj` | `MicLevelMonitorUnintegrated.csproj` |

## "Telepítés":

### 🟢 INTEGRATED verzió (.NET8 integrálással):

- A .NET8 verziója integrálva lett a programba, az exe fájlba magába, így telepíteni nem kell semmit pluszba,
  hogy működjön a program.

- A mappában található a .exe fájl, melyet kirakva parancsikonként bárhova, el lehet indítani a programot,
  vagy a mappából a .exe-vel.

### 🟡 UNINTEGRATED verzió (.NET8 integrálás nélküli):

- A NET8 integrálása nélkül, .NET8 vagy magasabb verziójú .NET szükséges a működéshez. Ha nincs telepítve
  a rendszerre, akkor telepíteni kell.

- A .NET telepítése után, már elindítható a program a .exe fájl segítségével.

- A mappában található a .exe fájl, melyet kirakva parancsikonként bárhova, el lehet indítani a programot,
  vagy a mappából a .exe fájlal.

## A program használata:

A tálcán alapból, mint minden újonnan telepített program, aminek van tálcás ikonja és megjelenik a háttérben
futó alkalmazásoknál jobb alul a tálcán, elrejtve a ^ jel felett fog megjelenni. De ezt a kis ikont, ki lehet 
húzni a tálcára, hogy mindig látható legyen.

A program elindulása után, folyamatos, reszponzív módon fog működni és amíg nem állítják le a programot,
mutatni és érzékelni fogja az alapértelmezetten használt mikrofonba bejövő hang, hangerejét.

A program bezárása, úgy lehetséges, ha jobb egérgombbal rákattintva az ikonra, megjelenik egy kis menü,
kilépés gombbal. Erre a kilépésre rákattintva, teljesen bezárja és leállítja a programot. Nem fogja innentől
kezdve érzékelni a mikrofon hangerejét.

## Automatikus indítás a Windows-al

### 🔐 Rendszergazdai jogosultság kell hozzá:

- Minden felhasználónak fog automatikusan indulni a Windows indítása és a felhasználóba bejelentkezés után.

- Magát az exe fájlt érdemes beraknia, vagy az exe fájlra mutató parancsikont a `C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup\` mappába, és le kell
  ellenőrizni a Gépházban az alkalmazások, indítópultnál, hogy a program be van-e kapcsolva, vagy a feladatkezelőben az indítási alkalmazásoknál
  engedélyezve van-e a program. Ha igen, akkor mindig el fog indulni a program a windows indulásánál.

### 👤 Nem kell rendszergazdai jogosultság:

- Csak az adott felhasználónak fog indulni a program

- Az exe fájlt, vagy az exe fájlra mutató parancsikont a `C:\Users\%username%\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup\` mappába kell belerakni
  és ezáltal az adott felhasználónak el fog indulni a Windows indítása után és az adott felhasználó bejelntkezése után.

## Technikai részletek

- **Framework**: .NET 8.0 Windows
- **UI**: Windows Forms
- **Audio**: NAudio 2.2.1
- **Platform**: win-x64
- **Nyelv**: C# 12.0

## Build parancsok

### Integrált verzió (SelfContained=true):
```bash
dotnet build MicLevelMonitor.csproj -c Release
dotnet publish MicLevelMonitor.csproj -c Release
```

### Framework függő verzió (SelfContained=false):
```bash
dotnet build MicLevelMonitorUnintegrated.csproj -c Release
dotnet publish MicLevelMonitorUnintegrated.csproj -c Release
