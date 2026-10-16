# Fortschritt

Stand der Umsetzung nach `09_Prompts.md`. Pro Phase ein Git-Commit „Phase N: …“.

| Phase | Inhalt | Status |
|---|---|---|
| 0 | Einlesen, Plan, Fragen | fertig |
| 1 | Setup und Grundgerüst | fertig |
| 2 | Labor-Blockout und Licht | fertig |
| 3 | Spieler und Interaktion | offen |
| 4a | Kernkreislauf: Panels (Mischstation, Pult) | offen |
| 4b | Kernkreislauf: Tier, Produktion, Verarbeitung, HUD | offen |
| 5 | Risiken, Warnzeichen, Explosion, Infektion | offen |
| 6 | Tagesablauf, Bestellung, Direktor | offen |
| 7 | Kommissar und Laken | offen |
| 8 | Zentrifuge, Kryo, Ultimativ-Höhepunkt | offen |
| 9 | Nebenstationen und Baumodus | offen |
| 10 | Staats-TV, Logbuch, Rekorde, Fenster | offen |
| 11 | Grafik und Animation (Art-Pass) | offen |
| 12 | Ton, Feinschliff, Performance, Build | offen |

## Phase 1 – Setup und Grundgerüst

- [x] Git-Repository mit Unity-`.gitignore`, Ausgangsstand als Commit „Phase 0“
- [x] Pakete: Test Framework 1.8.0, ProBuilder 6.1.3, Newtonsoft JSON 3.2.2 (dazu vorhanden: URP 17.6, Input System 1.20, uGUI/TextMeshPro)
- [x] Mobile-Quality-Stufe und Vorlagen entfernt
- [x] Ordnerstruktur `Assets/_Project/` nach Architektur 05
- [x] `Docs/` mit Originalen, `Fortschritt.md`, `Entscheidungen.md`, `Credits.md`
- [x] Schriften Oswald, JetBrains Mono, Caveat mit OFL-Lizenzen
- [x] Szenen Boot, Lab, NewsStudio, TestRange in den Build Settings (Boot lädt Lab)
- [x] Core: `GameManager` (Prefab), `GameState`, `EventBus`, `GameClock` (Pause, Zeitraffer), `Bootstrap`
- [x] Config: `GameConfig` + 47 Datenklassen, `UltimateSerum`, `FunRecipe`, `BuildMachineType`
- [x] `BalancingImporter` (Menü + automatisch bei JSON-Änderung)
- [x] `TextDatabase` (Pfadzugriff, Zufallsauswahl, Wut-Stufen, Platzhalter)
- [x] Text-Arbeitskopie mit Abschnitt `ui` (alle Prototyp-Texte)
- [x] `Formula` (GDD 4.3, 4.4, 6.2 inkl. Modifikatoren am Tier) + `ComboTexts`
- [x] EditMode-Tests (Formel-Tabelle, Kombinationen, Texte, Import)
- [x] Debug-Menü F1: Zeitraffer, Pause, Geld, Lager

## Phase 2 – Labor-Blockout und Licht

- [x] `layout` in der Balancing-Kopie (GDD 3, Prototyp-Koordinaten) + `LayoutConfig`/`StationLayout`
- [x] Blockout-Werkzeug (Menü *Plan erfüllt > Labor-Blockout bauen*): Halle 34 × 14,9 m, Decke 8,7 m, Fensteröffnung rechts, Tür, Träger, Rohre, Lampen, Banner, Kisten, Fässer
- [x] Glaskammer B-12 als Prefab: Glas, Rahmen, Messingringe, 3 Düsen mit Leuchtring, Gas-Zylinder, Kammerschilder, grünes Kammerlicht, Förderband, Laken (aus), Kapsel-Kollider r 3.1
- [x] 16 Stations-Prefabs mit Kollider und Schild (Text aus JSON): Klonanlage, Verarbeitung, Pult, Bandmotor, Mischstation, Mikroskop, Zentrifuge, Kryo, Verkostung, Grammophon, Rohrpost, Bestellterminal, Lager, TV + Wut-Skala, Plantafel
- [x] Pult-Zone und Laken-Zonen als Trigger, Baugitter als Gizmo
- [x] Licht: warmes Hauptlicht (Mixed, Schatten), 6 gebackene Lampen, Fensterlicht, Echtzeit nur Kammer (grün) und TV (rot); Light Probes, Reflexionssonde; Nebel; Post-Processing
- [x] Licht gebacken (GPU, ca. 10–35 s, 2 Lightmaps à 512)
- [x] TMP Essentials, Font-Assets Oswald/JetBrains Mono/Caveat
- [x] Aufnahmen in `Docs/Screenshots/Phase2_*.png`, Tests 48 grün, Konsole ohne Fehler

### Vergleich mit 07/08 – größte Unterschiede (für Phase 11)
1. **Materialien:** nur Flächenfarben; Referenz hat abgenutzten Stahl, Betonstruktur, Bodenplatten und Gitterroste.
2. **Kammer:** Gas kaum sichtbar, keine grünen Lichtkegel unter den Düsen und kein dichter Nebel; Rahmen noch schlicht.
3. **Pult:** Referenz 08 hat drei Glastanks, Schieberegler-Feld „MISCHUNG“, Statustafel mit Häkchen, NOT-STOPP mit Klappdeckel und Warnstreifen, Kipphebel; hier nur einfache Formen.
4. **Dichte:** Referenz 07 ist viel voller (Rohrleitungen an allen Wänden, Laufstege mit gelbem Geländer, Schreibtisch mit Lampe, roter Teppich mit Emblem, Aktenschränke, Bücher, Mikroskop, Reagenzgläser).
5. **TV und Plantafel:** noch leere leuchtende Flächen (Direktor, Balkendiagramme Milch/Eier/Fleisch folgen in Phase 6).
6. **Fenster:** graue Leuchtfläche statt Einöde mit Wachtürmen, Fabrik und Mond (Phase 10).
7. **Licht:** kein volumetrischer Dunst (URP hat keinen eingebauten); später über Partikel/Lichtkegel-Meshes.

### Offen / Hinweise für spätere Phasen
- Rechts überlappen sich aus manchen Blickwinkeln die Schilder von Verarbeitung, Zentrifuge und Kryokammer; Höhen in Phase 11 nachjustieren.
- Async-Backen kommt ohne Editor-Fokus nicht voran; über die Unity-Verbindung wird synchron gebacken.
- TextMeshPro-Font-Assets aus den Schriften werden in der ersten UI-Phase (4a) erzeugt.
- `InputSystem_Actions.inputactions` liegt noch im Assets-Hauptordner; in Phase 3 wird ein eigenes Action-Asset unter `_Project` angelegt.
- Das Lab ist noch leer (Kamera an der Spielerstartposition, Licht, GameManager).
- Leere Ordner aus der Architektur (z. B. `Audio/`, `Scripts/Player/`) werden erst angelegt, wenn Inhalt dazukommt, weil Git leere Ordner nicht speichert.
- Play-Mode-Tests über die Unity-Verbindung laufen mit Einzelschritten (`EditorApplication.Step`), weil der Editor ohne Fokus keine Frames rechnet.
