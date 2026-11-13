# Installationsanleitung für KeePassOTP

Diese Anleitung beschreibt, wie das selbst kompilierte **KeePassOTP**-Plugin in KeePass 2.x installiert und eingerichtet wird.

---

## 1. Voraussetzungen

- **KeePass Password Safe 2.x** (bereits installiert unter z. B. `C:\Program Files\KeePass Password Safe 2`)
- **.NET Framework 4.0** oder höher (unter Windows 10/11 standardmäßig integriert)
- **Administratorrechte** (für das Kopieren in den Ordner `Program Files`)

---

## 2. Speicherort der kompilierten Dateien

Nach dem Build-Vorgang befinden sich die fertigen Dateien in folgenden Verzeichnissen:

- **Plugin-Datei (`.plgx`)**:
  `c:\VisualCode\_Releases\KeePassOTP.plgx`
- **Deutsche Übersetzung**:
  `c:\VisualCode\_Releases\Translations\KeePassOTP.de.language.xml`

---

## 3. Automatische Installation (per PowerShell als Administrator)

Öffnen Sie eine PowerShell mit Administratorrechten und führen Sie folgende Befehle aus:

```powershell
# 1. KeePass vor der Installation beenden
Stop-Process -Name "KeePass" -ErrorAction SilentlyContinue

# 2. Zielverzeichnisse definieren
$pluginDir = "C:\Program Files\KeePass Password Safe 2\Plugins"
$transDir = Join-Path $pluginDir "Translations"

# 3. Unterordner für Übersetzungen erstellen, falls nicht vorhanden
if (-not (Test-Path $transDir)) {
    New-Item -ItemType Directory -Path $transDir -Force
}

# 4. Plugin und deutsche Sprachdatei kopieren
Copy-Item "c:\VisualCode\_Releases\KeePassOTP.plgx" -Destination $pluginDir -Force
Copy-Item "c:\VisualCode\_Releases\Translations\KeePassOTP.de.language.xml" -Destination $transDir -Force

# 5. KeePass-Plugin-Cache leeren (empfohlen bei Aktualisierungen)
Remove-Item "$env:LOCALAPPDATA\KeePass\PluginCache\*" -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "KeePassOTP wurde erfolgreich installiert!" -ForegroundColor Green
```

---

## 4. Manuelle Installation (Schritt für Schritt)

1. **KeePass schließen**:
   - Stellen Sie sicher, dass KeePass vollständig beendet ist (auch im System-Tray / Infobereich neben der Uhr).

2. **Plugin kopieren**:
   - Kopieren Sie die Datei `KeePassOTP.plgx` aus:
     `c:\VisualCode\_Releases\`
   - Fügen Sie die Datei in den KeePass-Plugin-Ordner ein:
     `C:\Program Files\KeePass Password Safe 2\Plugins\`
   - *Hinweis: Bestätigen Sie die Windows-Abfrage nach Administratorrechten.*

3. **Übersetzung kopieren (optional)**:
   - Erstellen Sie im Plugin-Ordner (sofern nicht vorhanden) einen Unterordner namens `Translations`:
     `C:\Program Files\KeePass Password Safe 2\Plugins\Translations\`
   - Kopieren Sie die Datei `KeePassOTP.de.language.xml` aus:
     `c:\VisualCode\_Releases\Translations\`
     in diesen Ordner.

4. **KeePass starten**:
   - Starten Sie KeePass ganz normal.

---

## 5. Überprüfung & Funktionsprüfung

1. **Plugin-Liste prüfen**:
   - Wählen Sie in KeePass im Menü: **Tools (Werkzeuge) -> Plugins...**
   - In der Liste muss **KeePassOTP** aufgeführt sein.
2. **Optionen aufrufen**:
   - Gehen Sie auf **Tools -> Optionen** (bzw. **Options**).
   - Dort finden Sie die neuen Tabs für **KeePassOTP**.
3. **OTP für einen Eintrag einrichten**:
   - Klicken Sie mit der rechten Maustaste auf einen beliebigen Eintrag.
   - Wählen Sie im Kontextmenü **KeePassOTP -> OTP-Einstellungen...** (oder QR-Code scannen).

---

## 6. Problembehebung (Troubleshooting)

- **Plugin wird nach Aktualisierung nicht geladen oder zeigt alte Version:**
  - Schließen Sie KeePass.
  - Löschen Sie den Ordner `%LocalAppData%\KeePass\PluginCache`. KeePass kompiliert die `.plgx`-Datei beim nächsten Start frisch.
- **Fehlende Berechtigungen beim Kopieren:**
  - Nutzen Sie eine administrative Eingabeaufforderung / PowerShell oder bestätigen Sie die UAC-Abfrage im Datei-Explorer.
- **Portables KeePass:**
  - Falls eine portable KeePass-Installation genutzt wird, kopieren Sie die `.plgx` einfach in das `Plugins`-Verzeichnis innerhalb des portablen Ordners (dort sind keine Administratorrechte nötig).
