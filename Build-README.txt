Satisfactory Planer – Quellcode

Voraussetzungen: Windows, .NET 10 SDK. Node.js nur zum erneuten Datenimport.
Die enthaltenen game.json, languages.json und UiStrings.json erlauben einen Build
ohne Datenimport und ohne externe NuGet-Pakete.

Bauen im entpackten Quellcode-Ordner:
dotnet restore SatisfactoryPlanner.csproj --configfile NuGet.Config
dotnet publish SatisfactoryPlanner.csproj -c Release -o App --no-restore

Spieldaten aktualisieren:
node normalize.js "PFAD-ZU-SATISFACTORY/CommunityResources/Docs/de.json" game.json
node import-languages.js game.json languages.json
Danach neu bauen. Der Sprachimport verwendet en-US.json neben der deutschen Datei.

UiStrings.json enthält die Übersetzung der App-Oberfläche Deutsch → Englisch.
Die Namen für Produkte, Rezepte und Maschinen stammen aus den lokalen Sprachdateien.
Sprache und Dunkelmodus werden in %LOCALAPPDATA%/SatisfactoryPlaner/Einstellungen.json gespeichert.
Alte Darstellung.json-Dateien mit nur DarkMode bleiben kompatibel.

Prüfungen:
dotnet App/SatisfactoryPlaner.dll --selftest
dotnet App/SatisfactoryPlaner.dll --settings-test "ABSOLUTER-PFAD/settings-test.json"
dotnet App/SatisfactoryPlaner.dll --window-test
dotnet App/SatisfactoryPlaner.dll --material-test
dotnet App/SatisfactoryPlaner.dll --map-test

--settings-test überschreibt die angegebene Testdatei; keine echte Darstellung.json verwenden.

Einzelne EXE mit integrierter Laufzeit (NuGet-Internetzugriff erforderlich):
dotnet publish SatisfactoryPlanner.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o Desktop-App --source https://api.nuget.org/v3/index.json

