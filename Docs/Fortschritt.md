# Fortschritt

Stand der Umsetzung nach `09_Prompts.md`. Pro Phase ein Git-Commit „Phase N: …“.

| Phase | Inhalt | Status |
|---|---|---|
| 0 | Einlesen, Plan, Fragen | fertig |
| 1 | Setup und Grundgerüst | fertig |
| 2 | Labor-Blockout und Licht | fertig |
| 3 | Spieler und Interaktion | fertig |
| 4a | Kernkreislauf: Panels (Mischstation, Pult) | fertig |
| 4b | Kernkreislauf: Tier, Produktion, Verarbeitung, HUD | fertig |
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

## Phase 3 – Spieler und Interaktion

- [x] Eigenes Input-Actions-Asset `Settings/PlanErfuellt_Input` (projektweit), Map „Player“: Move, Turn, Look, Interact, Pause, Antidote, Logbook, Build, Mute, Confirm, CaptureMouse; Vorlagen-Asset entfernt
- [x] `FirstPersonController` (CharacterController): WASD/Pfeile laufen 5,2 m/s, Maus (Klick fängt) oder Q/E/Pfeile drehen, Blick hoch/runter begrenzt, Kopfwippen, Neigen, Sichtfeld wie Prototyp; Spielerbereich aus `layout.playerArea`, Kammer-Mindestabstand 3,1 m; Haken für Infektion (Tempo, Invertieren)
- [x] Ego-Hände mit Spritze (`ViewModelHands`): Wippen, Atmen, Vorbeugen beim Halten, Spritzstoß vorbereitet
- [x] `IInteractable` + `Interactable` (Liste statt Find) + `InteractionSystem` (kleinste Wertung gewinnt, Haltebalken, nach Ausführen erst wieder nach Loslassen)
- [x] Stationen mit Komponente, Prompt aus JSON und Haltezeit aus `interaction.hold`: Pult (Zone), Mischstation (Zone), Laken (Zone, funktioniert schon), Grammophon, Verkostung (gesperrt bis Plan 2), Rohrpost, Bandmotor (Reparatur funktioniert), Zentrifuge, Kryo (gesperrt bis Plan 2), Bestellterminal, Lager
- [x] HUD-Prefab: Fadenkreuz, Maus-Hinweis, Prompt unten mittig (ausgegraut mit Begründung), Kurzmeldungen rechts (max. 4), Pause-Overlay (P/Esc)
- [x] Player- und HUD-Prefab werden vom Blockout-Werkzeug erzeugt und eingesetzt
- [x] Play-Test (Einzelschritte): alle 11 Stations-Prompts korrekt, Pult 0,12 s → Platzhalter-Meldung, Laken 0,8 s an/aus, gesperrte Ziele ohne Aktion, Bandreparatur 1,6 s, Pult-Kollision, Raumgrenzen, Tempo 5,2 m/s, Kammerabstand 3,1 m, Pause stoppt Bewegung
- [x] Tests 48 grün, Konsole ohne Fehler; Screenshot `Docs/Screenshots/Phase3_Pult_Prompt.png`

### Bitte selbst prüfen (Play Mode, Game View anklicken)
- Maus fangen per Klick, Umsehen, WASD, Q/E, Kopfwippen – fühlt sich das Tempo und die Mausempfindlichkeit richtig an?
- Leertaste halten an Pult, Mischstation, Laken, Grammophon; P/Esc für Pause.
- Hinweis: Echte Tastatur und Maus konnte ich über die Verbindung nicht drücken (Einzelschritt-Modus); die Logik ist über eine Test-Übersteuerung geprüft.

## Phase 4a – Panels (Mischstation, Pult)

- [x] `MixingLogic` (rein, getestet): Regler 0–8 begrenzt durch Lager, Mischen verbraucht Stoffe, 2,5 s, Regal max. 6, Rohserum-Erkennung (Ultimativ-Rezept), Fehlermeldungen wie Prototyp
- [x] `MixingSystem`: Mischvorgang in Spielzeit, Meldung „Serum gemischt … (brodelt seltsam)“, Events
- [x] `SerumTexts`: Name (unbekannt „Mischung A#·B#·C#“ bis zum Test am Tier, Zusätze zentrifugiert/konzentriert/tiefgekühlt), Farbe (stärkster Stoff, Zentrifuge, Ultimativ)
- [x] `Chamber` (Schnittstelle fürs Pult): alle Aktionen mit Prüfungen und Meldungen wie Prototyp, Reinigung 3 s funktioniert, Statuslampen Gas/Temperatur/Förderband/Vitalwerte; Klonen/Sprühen/Verarbeiten/Not-Stopp/Gegenmittel/Anleinen zeigen „folgt in Phase 4b“
- [x] Panels (uGUI, Maus): `PanelMixing` (3 Regler mit Lager, Infobox bekannt/unbekannt, MISCHEN mit Restzeit, Regalzeile), `PanelControl` (Regal mit Auswahl und Infobox, Hebel GAS/KLON Kuh/KLON Huhn/FÖRDERBAND, Statuslampen, Aktionen, NOT-STOPP); Knöpfe nur aktiv, wenn möglich; schließen mit Leertaste, P/Esc oder Knopf; immer nur ein Panel offen; Spieler steht, Maus frei
- [x] 3D: Rührer dreht beim Mischen, Schüssel in Mischfarbe, Tankfüllstände zeigen Lager, Fläschchen im Regal in Serumfarbe, Pult-Hebel klappen beim Ziehen um
- [x] EventSystem mit Input-System-UI-Modul
- [x] Tests 54 grün (6 neue zur Mischstation); Play-Test über Interaktion und Knöpfe; Screenshots `Docs/Screenshots/Phase4a_*.png`

### Bitte selbst prüfen
- Mischstation: Regler ziehen, MISCHEN, Fläschchen erscheinen im 3D-Regal. Pult: Fläschchen wählen, KLON (Hebel klappt). Leertaste/Esc schließen.
- Das Probenmikroskop im Misch-Panel kommt laut Prompt in Phase 9.
- Hinweis: Wird während des Play Mode neu kompiliert, wirft Unity NullReference-Fehler (Domain-Reload); einfach Play neu starten. Empfehlung: in den Unity-Einstellungen „Script Changes While Playing“ auf „Stop Playing and Recompile“ stellen.

## Phase 4b – Tier, Produktion, Verarbeitung, HUD

- [x] `AnimalStats` (Dosen, Ultimativ, Konzentration, Kryo-Bonus, Kuriosität, Siegel, Säure, Hitze, Schweben …) mit Werten über `Formula`
- [x] `Chamber`: Zustände Empty → Enter (fällt aus der Klonanlage, läuft übers Band) → Ok ↔ Unstable (8-s-Countdown) → Exit (Verarbeitung) | Destroy (Not-Stopp) | Flying; Klonen, Sprühen (Dosen addieren, Entdeckung erst nach dem Sprühen, Kuriositäten, Ultimativ ersetzt alle Dosen), Verarbeitung → Fleisch, Gegenmittel ins Tier, Anleinen, Reinigen
- [x] `Production.Deliver`: Bezahlung pro Tonne, Gesamtmengen, Durchschnittsqualität; Münzhuhn zahlt pro Ei; Milchhuhn liefert Milch
- [x] Kuh- und Huhn-Prefabs (Platzhalter aus Grundkörpern, Maße wie Prototyp) mit `AnimalVisuals`: Wachsen, Aufblähen, Euter/Zusatz-Euter, Glühen (grün/rot/golden, pulsierend), Laufen, Schweben, Zittern, Flügelschlag; Glühlicht folgt dem Tier
- [x] Schild über dem Tier (`AnimalLabel`): farbige Dosen bzw. Name, Status, GEPRÜFT-Stempel
- [x] Gas in der Kammer leuchtet beim Sprühen in Serumfarbe auf (`ChamberGas`)
- [x] HUD: obere Leiste (Marke, Kasse, Rate t/s, Tag, Plan), Lager-Chips (A/B/C/Gegenmittel/Seren), Tier-Meter Menge/Qualität/Stabilität mit 12 Segmenten
- [x] Prompt-Test: Kuh mit 1-1-0 → Stabilität 62, Qualität 60, Rate 3,9 t/s; Verarbeitung 10,8 t Fleisch
- [x] Tests 59 grün; Play-Test Kuh/Huhn/Not-Stopp/Gegenmittel/Instabil; Screenshots `Docs/Screenshots/Phase4b_*.png`

### Bewusst auf Phase 5 verschoben
- Risiken (Hitze, Säure, Schweben, Bandriss, Stromausfall), Warnzeichen, volle Explosion (VFX, Wut, Parteikontrolle, Infektion), Mut-Bonus/Explosion in der Verarbeitung. Die Explosion nach dem Countdown verseucht derzeit nur die Kammer.

### Offen / Hinweise für spätere Phasen
- Rechts überlappen sich aus manchen Blickwinkeln die Schilder von Verarbeitung, Zentrifuge und Kryokammer; Höhen in Phase 11 nachjustieren.
- Async-Backen kommt ohne Editor-Fokus nicht voran; über die Unity-Verbindung wird synchron gebacken.
- TextMeshPro-Font-Assets aus den Schriften werden in der ersten UI-Phase (4a) erzeugt.
- `InputSystem_Actions.inputactions` liegt noch im Assets-Hauptordner; in Phase 3 wird ein eigenes Action-Asset unter `_Project` angelegt.
- Das Lab ist noch leer (Kamera an der Spielerstartposition, Licht, GameManager).
- Leere Ordner aus der Architektur (z. B. `Audio/`, `Scripts/Player/`) werden erst angelegt, wenn Inhalt dazukommt, weil Git leere Ordner nicht speichert.
- Play-Mode-Tests über die Unity-Verbindung laufen mit Einzelschritten (`EditorApplication.Step`), weil der Editor ohne Fokus keine Frames rechnet.
