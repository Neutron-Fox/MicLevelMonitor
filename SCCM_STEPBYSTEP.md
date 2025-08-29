# MicLevelMonitor SCCM Telep�t�si L�p�sek

## �sszefoglal�

Ez a dokumentum egy egyszer�, l�p�senk�nti �tmutat�t tartalmaz a MicLevelMonitor alkalmaz�s SCCM-en kereszt�li telep�t�s�hez, k�l�n�s tekintettel arra, hogy a rendszert�lca ikon minden felhaszn�l� sz�m�ra l�that� legyen.

## 1. SCCM Alkalmaz�s l�trehoz�sa

SCCM-ben **Application** t�pus� telep�t�st kell haszn�lni **Package** helyett, mivel az Application modell t�bb el�nnyel rendelkezik:
- Jobb �llapotjelent�sek �s k�vethet�s�g
- Egyszer�bb telep�t�si f�gg�s�gek kezel�se
- Fejlettebb felhaszn�l�i �lm�ny szab�lyoz�s

## 2. Telep�t�si f�jlok el�k�sz�t�se

1. A telep�t� csomag tartalma:
   - MicLevelMonitor alkalmaz�s mappa (exe �s f�gg�s�gek)
   - Deploy-MicLevelMonitor.ps1
   - Setup-MicLevelMonitor.bat
   - Telep�t�si dokument�ci�

2. Telep�t�s el�tt ellen�rizze, hogy minden f�jl megfelel�en m�k�dik.

## 3. SCCM Alkalmaz�s konfigur�ci�ja

### 3.1 Alkalmaz�s l�trehoz�sa
1. SCCM konzol > Software Library > Applications
2. Create Application > Manually specify the application information
3. Adja meg: MicLevelMonitor, verzi�sz�m, kiad�, stb.

### 3.2 Telep�t�si t�pus hozz�ad�sa
1. Az �j alkalmaz�sn�l > Deployment Types > Add > Script Installer
2. Konfigur�ci�:
   - **Installation program**: `Setup-MicLevelMonitor.bat`
   - **Installation start in**: `%CONTENTLOCATIONPATH%`
   - **Content location**: [H�l�zati megoszt�s �tvonala]
   - **Detection method**: `C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp\MicLevelMonitor.lnk` f�jl l�tez�se

### 3.3 Telep�t�si k�vetelm�nyek
- Windows 10/11 oper�ci�s rendszer
- 64-bites architekt�ra

## 4. Terjeszt�s konfigur�l�sa

1. �j Deployment l�trehoz�sa a MicLevelMonitor alkalmaz�shoz
2. C�lgy�jtem�ny kiv�laszt�sa
3. Be�ll�t�sok:
   - **Purpose**: Required
   - **Deployment settings**: Install for system if resource is device; Install for user if resource is user
   - **Schedule**: Megfelel� telep�t�si id�pont kiv�laszt�sa
   - **User experience**: Hidden / Normal
   - **Allow end users to interact with the program installation**: No

## 5. A telep�t�si folyamat

### Mi t�rt�nik a kliens sz�m�t�g�peken:

1. **Tartalom let�lt�se**: Az SCCM let�lti az alkalmaz�sf�jlokat a megosztott h�l�zati helyr�l
2. **Telep�t�si hely l�trehoz�sa**: `C:\Ny�lt dokumentumok\MicLevelMonitor`
3. **Script futtat�sa**: `Setup-MicLevelMonitor.bat` -> `Deploy-MicLevelMonitor.ps1`

### A PowerShell script v�grehajtja:

1. **Ind�t�si parancsikon l�trehoz�sa**:
   - L�trehoz egy parancsikont a `C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp` mapp�ban
   - Ez minden felhaszn�l� sz�m�ra elind�tja az alkalmaz�st rendszerind�t�skor

2. **Rendszert�lca be�ll�t�sok konfigur�l�sa**:
   - Az aktu�lis felhaszn�l� be�ll�t�sai
   - Az alap�rtelmezett felhaszn�l�i profil be�ll�t�sai (j�v�beli �j felhaszn�l�knak)
   - Az �sszes l�tez� felhaszn�l�i profil be�ll�t�sai
   - �temezett feladat l�trehoz�sa, amely a bejelentkez�skor friss�ti a be�ll�t�sokat

3. **Visszajelz�s**:
   - Log f�jlt hoz l�tre a telep�t�si mapp�ban
   - Sikeres telep�t�s eset�n l�trehoz egy DEPLOYMENT_SUCCESS.txt f�jlt
   - Hiba eset�n hibak�dot ad vissza az SCCM-nek

## 6. Ellen�rz�si pontok

A sikeres telep�t�s ut�n ellen�rizze:

1. ? A `C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp` mapp�ban tal�lhat� MicLevelMonitor parancsikon
2. ? Az alkalmaz�s automatikusan elindul a rendszerind�t�skor
3. ? A rendszert�lc�n megjelenik az alkalmaz�s ikonja minden felhaszn�l� sz�m�ra
4. ? L�trej�tt a "Configure-MicLevelMonitor-ForNewUser" �temezett feladat

## 7. Hibaelh�r�t�s

Probl�m�k eset�n:
- Ellen�rizze a log f�jlt: `C:\Ny�lt dokumentumok\MicLevelMonitor\deployment_log.txt`
- Pr�b�lja manu�lisan futtatni a telep�t� scripteket
- Manu�lis be�ll�t�shoz haszn�lja a `ConfigureSystemTray.txt` f�jlban le�rt l�p�seket