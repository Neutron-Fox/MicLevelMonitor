# MicLevelMonitor SCCM Telep�t�si �tmutat�

Ez a r�szletes �tmutat� seg�t a MicLevelMonitor alkalmaz�s SCCM-en kereszt�li telep�t�s�ben, k�l�n�s tekintettel arra, hogy az alkalmaz�s ikonja minden felhaszn�l� sz�m�ra l�that� legyen a Windows 11 �rtes�t�si ter�let�n (rendszert�lc�n).

## 1. SCCM telep�t�si m�dszer kiv�laszt�sa

### Alkalmaz�s vagy Csomag?

**Aj�nl�s: SCCM Alkalmaz�s (Application)**

Az SCCM Alkalmaz�s modell jobb v�laszt�s a MicLevelMonitor telep�t�s�hez a k�vetkez� el�ny�k miatt:
- Jobb k�vethet�s�g �s �llapotjelent�sek
- Egyszer�bb kezel�s �s friss�t�s
- R�szletesebb telep�t�si felt�telek �s k�vetelm�nyek be�ll�that�s�ga
- Felhaszn�l�i �lm�ny szab�lyozhat�s�ga (csendes telep�t�s)
- Hat�konyabb f�gg�s�g- �s verzi�szab�lyoz�s

## 2. Alkalmaz�s telep�t�si csomag l�trehoz�sa

### 2.1. Telep�t�si f�jlok el�k�sz�t�se

1. K�sz�tse el� a k�vetkez� f�jlokat egy megosztott h�l�zati mapp�ban vagy a ConfigMgr forr�sk�nyvt�r�ban:
   - A teljes MicLevelMonitor alkalmaz�s mappa, amely tartalmazza:
     - MicLevelMonitor.exe
     - F�gg�s�gek (DLL-ek, konfigur�ci�s f�jlok)
   - Setup-MicLevelMonitor.bat
   - Deploy-MicLevelMonitor.ps1
   - TELEP�T�SI_�TMUTAT�.md �s/vagy ezt a dokumentumot

2. Gy�z�dj�n meg r�la, hogy a f�jlok helyesen vannak be�ll�tva �s az exe f�jl m�k�dik.

### 2.2. Alkalmaz�s l�trehoz�sa az SCCM konzolban

1. Nyissa meg az SCCM konzolt.
2. Navig�ljon a **Software Library** > **Application Management** > **Applications** men�ponthoz.
3. Jobb gombbal kattintson az **Applications** elemre, �s v�lassza a **Create Application** lehet�s�get.
4. V�lassza a **Manually specify the application information** opci�t, majd kattintson a **Next** gombra.

5. Adja meg az alkalmaz�s adatait:
   - **Name**: MicLevelMonitor
   - **Publisher**: [Az �n szervezete]
   - **Software version**: [Alkalmaz�s verzi�sz�ma]
   - **Installation program visibility**: Hidden (Rejtett)
   - Egy�b mez�k kit�lt�se sz�ks�g szerint
   - Kattintson a **Next** gombra.

6. Folytassa a var�zsl� l�p�seit, majd a befejez�shez kattintson a **Next**, majd a **Close** gombra.

### 2.3. Telep�t�si t�pus l�trehoz�sa

1. A l�trehozott alkalmaz�s tulajdons�gaiban kattintson a **Deployment Types** f�lre.
2. Jobb gombbal kattintson a ter�letre, �s v�lassza az **Add Deployment Type** lehet�s�get.
3. V�lassza a **Script Installer** opci�t, majd kattintson a **Next** gombra.

4. Adja meg a telep�t�si t�pus adatait:
   - **Name**: MicLevelMonitor Installer
   - **Content location**: [A telep�t�si f�jlokat tartalmaz� h�l�zati mappa el�r�si �tja]
   - **Installation program**: `Setup-MicLevelMonitor.bat`
   - **Installation start in**: `%CONTENTLOCATIONPATH%`
   - **Uninstall program**: (hagyja �resen, vagy adjon meg elt�vol�t� parancsot, ha sz�ks�ges)
   - **Detection method**: �ll�tson be megfelel� �szlel�si szab�lyt:
     - F�jl �szlel�s: `C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp\MicLevelMonitor.lnk`
     - Vagy regisztr�ci�s kulcs �szlel�s: `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MicLevelMonitor`

5. **Requirements** f�l�n adja meg:
   - Oper�ci�s rendszer: Windows 10/Windows 11
   - Egy�b k�vetelm�nyek sz�ks�g szerint

6. Kattintson a **Next** gombra a tov�bbi l�p�seken kereszt�l, majd a befejez�shez kattintson a **Close** gombra.

## 3. Telep�t�s terjeszt�se (Deployment)

### 3.1. Terjeszt�s l�trehoz�sa

1. Az SCCM konzolban navig�ljon a **Software Library** > **Application Management** > **Applications** men�ponthoz.
2. Jel�lje ki a l�trehozott MicLevelMonitor alkalmaz�st.
3. Jobb gombbal kattintson r� �s v�lassza a **Deploy** lehet�s�get.

4. Adja meg a terjeszt�s c�ladatait:
   - **Collection**: V�lassza ki a megfelel� eszk�zgy�jtem�nyt, amelyekre telep�teni szeretn�
   - Kattintson a **Next** gombra.

5. Adja meg a tartalom forr�s�t (Content):
   - V�lassza ki a megfelel� disztrib�ci�s pontot vagy disztrib�ci�s pont csoportot
   - Kattintson a **Next** gombra.

6. Adja meg a telep�t�s be�ll�t�sait:
   - **Purpose**: Available (v�laszthat�) vagy Required (k�telez�)
   - **Make this task sequence available to the following**: Configuration Manager Clients
   - **Schedule**: �ll�tsa be a k�v�nt telep�t�si �temez�st
   - Kattintson a **Next** gombra.

7. Deployment be�ll�t�sok:
   - **User experience**: Hidden (aj�nlott)
   - **Software installation**: Allow installations (enged�lyezve)
   - **System restart**: Suppress restarts on servers / workstations
   - **Write filter handling for Windows Embedded devices**: Commit changes at deadline or maintenance windows
   - Kattintson a **Next** gombra.

8. �ll�tsa be a figyelmeztet�seket, majd kattintson a **Next** gombra.
9. Ellen�rizze a be�ll�t�sokat, �s kattintson a **Next** gombra a terjeszt�s l�trehoz�s�hoz.
10. Kattintson a **Close** gombra a befejez�shez.

## 4. A telep�t�s folyamata

Az SCCM kliens a k�vetkez� m�veleteket hajtja v�gre:

### 4.1. Tartalom let�lt�se �s telep�t�se

1. **Tartalom let�lt�se**: Az SCCM kliens let�lti a MicLevelMonitor alkalmaz�st �s a telep�t�si scripteket a disztrib�ci�s pontr�l a helyi cache-be.

2. **C�l mappa l�trehoz�sa**: Az SCCM l�trehozza a c�lmapp�t: `C:\Ny�lt dokumentumok\MicLevelMonitor`.

3. **F�jlok m�sol�sa**: Az SCCM �tm�solja az �sszes alkalmaz�sf�jlt a c�lmapp�ba.

### 4.2. Telep�t� script futtat�sa

1. **Adminisztr�tori jogosults�gok**: Az SCCM a **Setup-MicLevelMonitor.bat** f�jlt rendszergazdai jogosults�ggal futtatja.

2. **PowerShell script ind�t�sa**: A batch f�jl elind�tja a **Deploy-MicLevelMonitor.ps1** PowerShell scriptet.

### 4.3. A PowerShell script m�k�d�se

A PowerShell script a k�vetkez� m�veleteket hajtja v�gre:

1. **Ind�t�si parancsikon l�trehoz�sa**:
   - L�trehoz egy parancsikont a MicLevelMonitor.exe alkalmaz�shoz
   - Elhelyezi ezt a parancsikont a publikus ind�t�si mapp�ban: `C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp`
   - Ez biztos�tja, hogy az alkalmaz�s minden felhaszn�l� sz�m�ra automatikusan elinduljon a rendszerind�t�skor

2. **Az aktu�lis felhaszn�l� be�ll�t�sainak konfigur�l�sa**:
   - M�dos�tja a registry be�ll�t�sokat, hogy a rendszert�lca ikonok mindig l�that�ak legyenek
   - Be�ll�tja az `EnableAutoTray` �rt�ket 0-ra a `HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer` �tvonalon
   - L�trehozza a `NotificationAreaCustomization` registry kulcsot, ha m�g nem l�tezik

3. **Az alap�rtelmezett felhaszn�l�i profil konfigur�l�sa**:
   - Bet�lti a `C:\Users\Default\NTUSER.DAT` f�jlt
   - Be�ll�tja az `EnableAutoTray` �rt�ket 0-ra az alap�rtelmezett profilban
   - L�trehozza a sz�ks�ges registry kulcsokat
   - Kirakja a registry hive-ot

4. **Minden l�tez� felhaszn�l�i profil konfigur�l�sa**:
   - Megkeresi az �sszes felhaszn�l�i profilt a C:\Users mapp�ban
   - Minden felhaszn�l�n�l be�ll�tja az `EnableAutoTray` �rt�ket 0-ra
   - L�trehozza a sz�ks�ges registry kulcsokat

5. **�temezett feladat l�trehoz�sa �j felhaszn�l�knak**:
   - L�trehoz egy `Configure-MicLevelMonitor-ForNewUser` nev� �temezett feladatot
   - Az �temezett feladat bejelentkez�skor fut le minden felhaszn�l�n�l
   - A feladat ugyanazokat a registry be�ll�t�sokat alkalmazza, hogy az ikon l�that� legyen

6. **�tmutat� l�trehoz�sa**:
   - L�trehoz egy `ConfigureSystemTray.txt` nev� sz�veges f�jlt
   - A f�jl tartalmazza a manu�lis be�ll�t�si lehet�s�geket arra az esetre, ha az automatikus be�ll�t�s nem m�k�dne

## 5. Telep�t�s ut�ni ellen�rz�s

Az SCCM telep�t�s ut�n a k�vetkez�ket �rdemes ellen�rizni:

1. **Ind�t�si parancsikon**: A `C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp` mapp�ban tal�lhat�-e a MicLevelMonitor parancsikonja.

2. **Alkalmaz�s m�k�d�se**: A MicLevelMonitor alkalmaz�s megfelel�en elindul-e a rendszerind�t�skor.

3. **Rendszert�lca ikon**: A rendszert�lc�n l�that�-e az alkalmaz�s ikonja minden felhaszn�l� sz�m�ra.

4. **�temezett feladat**: L�trej�tt-e a "Configure-MicLevelMonitor-ForNewUser" nev� �temezett feladat a rendszeren.

## 6. Hibaelh�r�t�s

Ha probl�m�k mer�lnek fel a telep�t�s sor�n:

### 6.1. Alkalmaz�s nem indul el

- Ellen�rizze, hogy a parancsikon l�tezik-e a Startup mapp�ban.
- Ellen�rizze, hogy a hivatkoz�s helyes-e �s a c�lalkalmaz�s l�tezik.
- N�zze meg az esem�nynapl�kat a hib�k�rt.

### 6.2. Az ikon nem l�that� a rendszert�lc�n

- Pr�b�lja meg manu�lisan be�ll�tani a rendszert�lca ikont a `ConfigureSystemTray.txt` f�jlban le�rt m�don.
- Ellen�rizze, hogy az alkalmaz�s fut-e (Task Manager).
- Ind�tsa �jra a Windows Explorer folyamatot vagy a sz�m�t�g�pet.

### 6.3. SCCM telep�t�si hib�k

- Ellen�rizze az SCCM kliens napl�f�jlokat a `C:\Windows\CCM\Logs` mapp�ban.
- Futtassa manu�lisan a telep�t� scriptet a hiba�zenet megtekint�s�hez.
- Ellen�rizze a jogosults�gokat �s a h�l�zati kapcsolatot.

## 7. T�mogat�s

Technikai probl�m�k eset�n forduljon a rendszergazd�hoz vagy a fejleszt�h�z.