# Plan erfüllt – Startanleitung

Unity-Umsetzung des Prototyps „Plan erfüllt“ (Satire auf Planwirtschaft: Kühe und Hühner mit Seren mutieren, Pläne erfüllen, Kommissar täuschen).

## Spiel starten (Windows-Build)

1. Ordner `Builds/Windows/` öffnen (wird beim Build erzeugt, liegt nicht im Git).
2. `PlanErfuellt.exe` starten. Das Spiel startet im randlosen Vollbild in Bildschirmauflösung (umstellbar in den Einstellungen).
3. Im Hauptmenü „Neues Spiel“ → Einsatzbesprechung (sechs Seiten, „Weiter →“) → auf der letzten Seite „Für den Plan!“ (mit Einführung und Hinweisen) oder jederzeit „Ohne Einführung“. Jede Schicht beginnt mit der Auftragskarte (Tagessoll); die Zeit läuft erst nach „Schicht beginnen“.
   Bestellt wird jederzeit am Bestellterminal im Lager (Tür ganz links in der Rückwand) – für den nächsten Morgen. Wer nichts bestellt, bekommt die Standardlieferung. Der Knopf „Empfohlene Bestellung“ trägt passende Mengen für den Plan von morgen ein.
4. Gespeichert wird automatisch zu Beginn jeder Schicht. „Weiterspielen“ im Hauptmenü setzt dort fort. Der Spielstand liegt unter `%USERPROFILE%\AppData\LocalLow\DefaultCompany\Plan erfüllt\spielstand.json`.
5. Nebenzimmer hinter der Rückwand: Lager (Bestellen), Umkleide (Schutzkittel), Schlafraum (mit erfülltem Plan früher Feierabend), Kantine (Essen verstärkt die Seren bis Schichtende), Direktorbüro (Kaviar persönlich).

Beenden: Hauptmenü „Beenden“, im Spiel Esc → „Spiel beenden“, oder Alt+F4. Esc → „Hauptmenü“ führt zurück ins Hauptmenü (Fortschritt seit Schichtbeginn verfällt).

## Steuerung

| Taste | Aktion |
|---|---|
| W A S D | laufen |
| Shift halten | sprinten |
| Leertaste | springen (schließt auch Panels) |
| Maus oder Q / E | umsehen bzw. drehen |
| F halten | Station benutzen (Mischstation, Pult, Laken, Zentrifuge …) |
| G | Gegenmittel selbst spritzen (nach Infektion) |
| L | Forschungslogbuch |
| B | Baumodus (Maschinen, ab Plan 2) |
| M | Ton an/aus |
| P oder Esc | Pause (dort: Logbuch, Staats-TV, Ton, Einstellungen, Hauptmenü, Spiel beenden) |
| Mausklick ins Bild | Maus fangen (nur nach Alt+Tab nötig) |

Einstellungen (Hauptmenü oder Pause → „Einstellungen“): Lautstärke gesamt, Musik, Effekte, Mausempfindlichkeit, Kamerawackeln, Grafik Hoch/Niedrig, Anzeige Vollbild/Fenster (Fenster 1600 × 900), Ton an/aus. Sie werden gespeichert. Alt+Enter schaltet ebenfalls zwischen Vollbild und Fenster um.

## Im Unity-Editor öffnen

- Unity **6000.6.6f1** (URP). Projektordner im Unity Hub hinzufügen.
- Szene `Assets/_Project/Scenes/Lab.unity` öffnen und Play drücken.
- Menü **Plan erfüllt**:
  - *Labor-Blockout bauen* – baut Halle, Stationen, Tiere, HUD und Systeme aus den Editor-Skripten neu
  - *Balancing importieren* – überträgt `Assets/_Project/Data/Json/Balancing.json` in das GameConfig-Asset
  - *EditMode-Tests ausführen* – Ergebnis in `Temp/editmode_result.txt`
  - *Windows-Build erstellen* – Ausgabe nach `Builds/Windows/`
- F1 im Play Mode: Debug-Menü (Zeitraffer, Geld, Lager …), nur im Editor und in Development-Builds.

## Daten

- Spielwerte: `Assets/_Project/Data/Json/Balancing.json` (Arbeitskopie von `Docs/02_Balancing.json`)
- Texte: `Assets/_Project/Data/Json/Texte_de.json` (Arbeitskopie von `Docs/03_Texte_de.json`)
- Klänge: selbst erzeugt mit `python Tools/gen_audio.py Assets/_Project/Audio/Generated`
- Texturen: selbst erzeugt mit `Tools/gen_lookdev_textures.py`, dazu Poly-Haven-Texturen (CC0, siehe `Docs/Credits.md`)

## Weitere Dokumente

- `Docs/Fortschritt.md` – Stand jeder Phase, offene Punkte
- `Docs/Entscheidungen.md` – eigene Entscheidungen (D1 …)
- `Docs/Screenshots/` – Aufnahmen je Phase
