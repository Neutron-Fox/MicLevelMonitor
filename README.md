---- Mikrofon hang érzékelő .NET8 integrálással ----


** Win-x64 verzió, tálca ikonos program **


// "Telepítés": //

	- A .NET8 verziója integrálva lett a programba, az exe fájlba magába, így telepíteni nem kell semmit pluszba,
	  hogy működjön a program.

	- A mappában található a .exe fájl, melyet kirakva parancsikonként bárhova, el lehet indítani a programot,
	  vagy a mappából a .exe-vel.


// A program használata: //

	A tálcán alapból, mint minden újonnan telepített program, aminek van tálcás ikonja és megjelenik a háttérben
	futó alkalmazásoknál jobb alul a tálcán, elrejtve a ^ jel felett fog megjelenni. De ezt a kis ikont, ki lehet 
	húzni a tálcára, hogy mindig látható legyen.

	A program elindulása után, folyamatos, reszponzív módon fog működni és amíg nem állítják le a programot,
	mutatni és érzékelni fogja az alapértelmezetten használt mikrofonba bejövő hang, hangerejét.

	A program bezárása, úgy lehetséges, ha jobb egérgombbal rákattintva az ikonra, megjelenik egy kis menü,
	kilépés gombbal. Erre a kilépésre rákattintva, teljesen bezárja és leállítja a programot. Nem fogja innentől
	kezdve érzékelni a mikrofon hangerejét.

// Automatikus indítás a Windows-al //

	Rendszergazdai jogosultság kell hozzá:

		- Minden felhasználónak fog automatikusan indulni a Windows indítása és a felhasználóba bejelentkezés után.

		- Magát az exe fájlt érdemes beraknia, vagy az exe fájlra mutató parancsikont a "C://ProgramData/Microsoft/Windows/Start Menu/Indítópult/" mappába, és le kell
		  ellenőrizni a Gépházban az alkalmazások, indítópultnál, hogy a MicLevelVisualizer be van-e kapcsolva, vagy a feladatkezelőben az indítási alkalmazásoknál
		  engedélyezve van-e a program. Ha igen, akkor mindig el fog indulni a program a windows indulásánál.

	Nem kell rendszergazdai jogosultság:

		- Csak az adott felhasználónak fog indulni a program

		- Az exe fájlt, vagy az exe fájlra mutató parancsikont a "C://%felhasználó%/Appdata/Microsoft/Windows/Start Menu/Programs/Indítópult/" mappába kell belerakni
		  és ezáltal az adott felhasználónak el fog indulni a Windows indítása után és az adott felhasználó bejelntkezése után.

---- Mikrofon hang érzékelő .NET8 integrálás nélküli ----


** Win-x64 verzió, tálca ikonos program **


// "Telepítés": //

	- A NET8 integrálása nélkül, a mappában megtalálható a NET8-nak a legfrissebb verziója 2025.06-ban, melyet
	  fel kell telepíteni mindenek előtt, hogy működni tudjon a program a számítógépen, ha nincsen telepítve
	  semmilyen .NET verzió az eszközre. .NET8 vagy magasabb verziójú .NET szükséges a működéshez

	- A .NET telepítése után, már elindítható a program a .exe fájl segítségével.

	- A mappában található a .exe fájl, melyet kirakva parancsikonként bárhova, el lehet indítani a programot,
	  vagy a mappából a .exe fájlal.


// A program használata: //

	A tálcán alapból, mint minden újonnan telepített program, aminek van tálcás ikonja és megjelenik a háttérben
	futó alkalmazásoknál jobb alul a tálcán, elrejtve a ^ jel felett fog megjelenni. De ezt a kis ikont, ki lehet 
	húzni a tálcára, hogy mindig látható legyen.

	A program elindulása után, folyamatos, reszponzív módon fog működni és amíg nem állítják le a programot,
	mutatni és érzékelni fogja az alapértelmezetten használt mikrofonba bejövő hang, hangerejét.

	A program bezárása, úgy lehetséges, ha jobb egérgombbal rákattintva az ikonra, megjelenik egy kis menü,
	kilépés gombbal. Erre a kilépésre rákattintva, teljesen bezárja és leállítja a programot. Nem fogja innentől
	kezdve érzékelni a mikrofon hangerejét.

// Automatikus indítás a Windows-al //

	Rendszergazdai jogosultság kell hozzá:

		- Minden felhasználónak fog automatikusan indulni a Windows indítása és a felhasználóba bejelentkezés után.

		- Például a nyilvános dokumentumok mappába ha a programot mappástul behelyezzük és csinálunk egy parancsikont, ami abba a mappába fog mutatni az exe fájlra,
		  akkor ->

		- A "C://ProgramData/Microsoft/Windows/Start Menu/Indítópult/" mappába átrakva a parancsikont, majd leellenőrizve a Gépházban az alkalmazások, indítópultnál,
		  hogy a MicLevelVisualizer be van-e kapcsolva, vagy a feladatkezelőben az indítási alkalmazásoknál engedélyezve van-e a program, akkor ha igen, mindig el fog
		  indulni a program a windows indulásánál.

	Nem kell rendszergazdai jogosultság:

		- Csak az adott feéhasználónak fog indulni a program

		- A program mappáját a felhasználó mappájába kell behelyezni, a "C://%felhasználó%/" helyre, majd egy parancsikont kell létrehozni, ezek után ->

		- A parancsikont, a "C://%felhasználó%/Appdata/Microsoft/Windows/Start Menu/Programs/Indítópult/" mappába kell belerakni és ezáltal az adott felhasználónak el
		  fog indulni a Windows indítása után és az adott felhasználó bejelntkezése után.
