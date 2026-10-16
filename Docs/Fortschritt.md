# Fortschritt

Stand der Umsetzung nach `09_Prompts.md`. Pro Phase ein Git-Commit „Phase N: …“.

| Phase | Inhalt | Status |
|---|---|---|
| 0 | Einlesen, Plan, Fragen | fertig |
| 1 | Setup und Grundgerüst | fertig |
| 2 | Labor-Blockout und Licht | offen |
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

### Offen / Hinweise für spätere Phasen
- TextMeshPro-Font-Assets aus den Schriften werden in der ersten UI-Phase (4a) erzeugt.
- `InputSystem_Actions.inputactions` liegt noch im Assets-Hauptordner; in Phase 3 wird ein eigenes Action-Asset unter `_Project` angelegt.
- Das Lab ist noch leer (Kamera an der Spielerstartposition, Licht, GameManager).
- Leere Ordner aus der Architektur (z. B. `Audio/`, `Scripts/Player/`) werden erst angelegt, wenn Inhalt dazukommt, weil Git leere Ordner nicht speichert.
- Play-Mode-Tests über die Unity-Verbindung laufen mit Einzelschritten (`EditorApplication.Step`), weil der Editor ohne Fokus keine Frames rechnet.
