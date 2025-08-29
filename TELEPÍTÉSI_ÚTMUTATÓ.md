# MicLevelMonitor Telep�t�si �tmutat�

Ez a dokument�ci� seg�t a MicLevelMonitor alkalmaz�s SCCM-en kereszt�li telep�t�s�ben �s a Windows 11 rendszert�lc�n val� megfelel� megjelen�t�s�nek be�ll�t�s�ban.

## A telep�t�si csomag tartalma

- **MicLevelMonitor alkalmaz�s** - Az alkalmaz�s f� exe f�jlja �s a sz�ks�ges f�gg�s�gek
- **Deploy-MicLevelMonitor.ps1** - PowerShell script az alkalmaz�s konfigur�l�s�hoz
- **TELEP�T�SI_�TMUTAT�.md** - Ez a telep�t�si �tmutat�

## SCCM telep�t�si l�p�sek

1. K�sz�tsen egy SCCM telep�t�si csomagot, amely az al�bbi tartalmakkal rendelkezik:
   - Az alkalmaz�s teljes mapp�ja
   - A Deploy-MicLevelMonitor.ps1 PowerShell script
   - Ez az �tmutat�

2. Az SCCM deployment sor�n �ll�tsa be az al�bbi telep�t�si param�tereket:
   - **Telep�t�si mappa**: `C:\Ny�lt dokumentumok\MicLevelMonitor`
   - **Futtatand� program**: `powershell.exe -ExecutionPolicy Bypass -File "Deploy-MicLevelMonitor.ps1"`

## Mit tesz a telep�t� script?

A `Deploy-MicLevelMonitor.ps1` script az al�bbi m�veleteket hajtja v�gre:

1. **Ind�t�si parancsikon l�trehoz�sa**:
   - L�trehoz egy parancsikont a MicLevelMonitor.exe alkalmaz�shoz
   - Elhelyezi a parancsikont a publikus ind�t�si mapp�ban (`C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp`)
   - Ezzel biztos�tja, hogy az alkalmaz�s minden felhaszn�l� sz�m�ra automatikusan elinduljon rendszerind�t�skor

2. **A MicLevelMonitor ikon megjelen�t�s�nek biztos�t�sa a rendszert�lc�n**:
   - Elind�tja az alkalmaz�st, hogy regisztr�lja mag�t a rendszert�lc�n
   - Megkeresi a MicLevelMonitor ikont a rendszert�lca be�ll�t�sokban
   - Be�ll�tja a MicLevelMonitor ikont "l�that�" �llapot�ra a jelenlegi felhaszn�l� sz�m�ra
   - El�k�sz�ti az alap�rtelmezett felhaszn�l�i profilt, hogy az �j felhaszn�l�k sz�m�ra is l�that� legyen az ikon
   - L�trehoz egy �temezett feladatot, amely bejelentkez�skor ellen�rzi �s biztos�tja, hogy a MicLevelMonitor ikon l�that� legyen

3. **Biztons�gi megold�s alkalmaz�sa**:
   - L�trehoz egy �tmutat� sz�veges f�jlt arra az esetre, ha az automatikus be�ll�t�s nem m�k�dne
   - Ez r�szletes le�r�st tartalmaz a rendszert�lca ikon manu�lis konfigur�l�s�hoz

## Telep�t�s ut�ni ellen�rz�s

A telep�t�s ut�n ellen�rizze az al�bbiakat:

1. A MicLevelMonitor alkalmaz�s megfelel�en elindult-e
2. A rendszert�lc�n l�that�-e az alkalmaz�s ikonja minden felhaszn�l� sz�m�ra
3. A `C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp` mapp�ban megtal�lhat�-e a MicLevelMonitor parancsikonja
4. L�trej�tt-e a "Configure-MicLevelMonitor-ForNewUser" nev� �temezett feladat a rendszeren

## Manu�lis be�ll�t�s (ha sz�ks�ges)

Ha az automatikus rendszert�lca be�ll�t�s nem m�k�dne, a felhaszn�l�k k�vethetik a `ConfigureSystemTray.txt` f�jlban tal�lhat� utas�t�sokat:

1. Kattintson a Windows t�lc�n a felfel� mutat� ny�lra (^)
2. Kattintson a "Testreszab�s" gombra
3. A megjelen� Be�ll�t�sok oldalon keresse meg a "MicLevelMonitor" alkalmaz�st
4. �ll�tsa be a kapcsol�t "Megjelen�t�s" �rt�kre

Vagy:

1. Nyissa meg a Windows Be�ll�t�sokat
2. Navig�ljon a "Szem�lyre szab�s" > "T�lca" > "T�lcasarok ikonjai" men�pontba
3. Keresse meg a "MicLevelMonitor" alkalmaz�st �s �ll�tsa "Be" poz�ci�ba

## Hibaelh�r�t�s

- **Az alkalmaz�s nem indul el automatikusan**: Ellen�rizze, hogy a parancsikon l�tezik-e a Startup mapp�ban �s a hivatkoz�s helyes-e
- **Az alkalmaz�s ikonja nem l�that�**: K�vesse a manu�lis be�ll�t�si �tmutat�t
- **Az alkalmaz�s fut, de nem jelenik meg az ikon**: Ind�tsa �jra a Windows Explorer folyamatot vagy ind�tsa �jra a sz�m�t�g�pet
- **�j felhaszn�l� sz�m�ra nem l�that� az ikon**: Ellen�rizze, hogy a "Configure-MicLevelMonitor-ForNewUser" nev� �temezett feladat l�tezik-e �s enged�lyezve van-e

## T�mogat�s

Technikai probl�m�k eset�n forduljon a rendszergazd�hoz vagy a fejleszt�h�z