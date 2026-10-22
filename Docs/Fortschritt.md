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
| LD | Look-Development: Kammer B-12, Pult, Halle nach Bild 08 | fertig |
| 5 | Risiken, Warnzeichen, Explosion, Infektion | fertig |
| 6 | Tagesablauf, Bestellung, Direktor | fertig |
| 7 | Kommissar und Laken | fertig |
| 8 | Zentrifuge, Kryo, Ultimativ-Höhepunkt | fertig |
| 9 | Nebenstationen und Baumodus | fertig |
| 10 | Staats-TV, Logbuch, Rekorde, Fenster | fertig |
| 11 | Grafik und Animation (Art-Pass) | fertig |
| 12 | Ton, Feinschliff, Performance, Build | fertig |
| E1 | Erweiterung: Balancing, Bestellen für morgen, Einführung, Nebenzimmer, Direktor, Sendung, UI | fertig |

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

## Erweiterung E1 – Balancing, Nebenzimmer, Direktor, Sendung, UI

- [x] Balancing neu (D106): Schicht 5 Minuten, Überschuss zahlt doppelt, Wodka/Kaviar/Gegenmittel teurer, Klonen kostet (D109)
- [x] Bestellterminal als Panel, Bestellung für morgen, Standardlieferung (D107); Laken als Verbrauchsartikel (D108)
- [x] Geführte Einführung an Tag 1 mit Zielkasten und Pfeil (D110)
- [x] Halle tiefer, Anbau mit Lager, Umkleide, Schlafraum, Kantine, Direktorbüro (D111); begehbare Fläche `WalkArea`; Maschinen leicht verteilt (D112)
- [x] Direktor als animierter dicker Funktionär im Fernseher (D113)
- [x] Staats-TV: Sprecher jubelt, schwitzt oder weint je nach Tagesergebnis, wechselnde Kamerafahrten (D114)
- [x] UI-Stil: Knöpfe mit Fase, eingelassene Felder, Messingregler, Zierstreifen (D115)
- [x] EditMode-Tests 146 grün (neu: `EconomyTests`, `WalkAreaTests`, `NewsMoodTests`); Play-Test mit Screenshots (Zimmer, TV bei Wut, Sendung Jubel/Weinen, Panels)
- [x] Nachbesserung: Türrahmen ohne Flackern, Sockel nicht mehr quer durch die Türöffnung; Springen (D117); Bereich hinter dem Band begehbar (D118); Wachstumshormon stärker (D116). Tests 147 grün
- [x] UI-Runde: Knopf-Rückmeldung (D121), Planfenster kleiner, sanfterer Rand (D122), Taste F (D120), Einführung mit Bodenpfeilen (D123), Logbuch (D124), Bildformate (D125). Tests 151 grün
- [x] Nebenzimmer mit Funktion: Kantine-Schub (D126), Schutzkittel (D127), früher Feierabend (D128), Kaviar persönlich (D129); Platzierung rechts, Schilder, Lampen (D130); Taste am Prompt, „Wie gestern“, Klingelton, Formattest (D131). Tests 158 grün
- [x] Einarbeitung nach Spieltest: Einsatzbesprechung in sechs Seiten (D135), Auftragskarte mit Tagessoll zu Schichtbeginn, Zeit steht (D134), Hinweise bei erstem Auftreten für Kontrolle, Bandriss, Not-Stopp usw. (D133). Tests 164 grün; Play-Test mit Screenshots
- [x] Balancing-Serie über alle 27 Dosen, Melkpumpen, Kühe durchschleusen (Werkzeug `BalanceBatch`); danach jede Ware ihr Serum (D137), Legebatterie (D138), Kryo 8 s (D136). Tests 165 grün
- [x] Spielerführung nach Spieltest: Pflicht-Mischung und -Bestellung in der Einführung (D139), „Empfohlene Bestellung“ (D140), Hebel am Pult (D141), Warnmeldungen Wut/Zeit/Kommissar (D142), Kommissar erklärt (D143), Wut-Skala rechts am Fernseher (D144). Tests 172 grün
- [x] Qualität und Abgabe: Spielstand-Test für alle neueren Felder, Kontrolle endet mit der Schicht (D145), feste Tasten im Baumodus (D146), Springen abschaltbar und Tastenbelegung (D147), Projektdoku und itch.io-Entwurf. Tests 176 grün
- [x] Rückmeldung und Gefühl: Geld-Zahlen (D148), Konfetti und jubelnder Direktor bei erfülltem Plan (D149), wackelnder Fernseher bei Wut (D150), Angstlaute vor der Krise (D151). Tests 179 grün

## Phase 12 – Ton, Feinschliff, Performance, Build

- [x] Ton (GDD 14): 24 Klänge prozedural nach der WebAudio-Synthese des Prototyps (`Tools/gen_audio.py`): Muhen (tiefer mit A, wobbelig ab B ≥ 2 / Ultimativ), Gackern, Singende Kuh, Zischen, Blubbern, Explosion, Sirene bei INSTABIL (Schleife), Zentrifugensurren (Tonhöhe nach Drehzahl), Glas, Eis, Kurbel, Marschmusik vom Grammophon, Münze, Fanfare, trauriges Motiv, Stempel, Rohrpost, Warnton, Klick, Direktor-Grummeln (Silben nach Textlänge, tiefer und lauter bei Wut)
- [x] `AudioService`: 16 Effektquellen im Ring, Kanäle Gesamt × Musik/Effekte, Klang zu Meldungen über `audio.toastSounds`, Pause hält den Ton an, **M** = Ton an/aus
- [x] Einstellungen (gespeichert): Lautstärke gesamt, Musik, Effekte, Mausempfindlichkeit, Kamerawackeln, Grafik Hoch/Niedrig (zweite URP-Einstellung), Ton an/aus; erreichbar aus Hauptmenü und Pause
- [x] Hauptmenü: Startbildschirm mit „Für den Plan!“, „Einstellungen“, „Beenden“; Pause mit „Ton an · M“ und „Einstellungen“
- [x] Performance-Pass (GDD 15): kleine Teile ohne Schatten, statisches Batching, Licht neu gebacken. Editor-Messung (Play Mode, Kuh in der Kammer, Game View 1400 × 828): **156 FPS** (vorher 111), CPU 7,9 ms, Render-Thread 4,2 ms, Draw Calls 1 318 (vorher 5 160, davon 1 253 statisch gebündelt), Schattenwerfer 722 (vorher 4 104), ca. 850 000 Dreiecke
- [x] Windows-Build `Builds/Windows/PlanErfuellt.exe` (230 MB, 0 Fehler), Probestart: Hauptmenü erscheint, Player.log ohne Ausnahmen. Startanleitung in `Docs/README.md`
- [x] Durchlauf im Zeitraffer (Autopilot, 10×, Log `Docs/Durchlauf_Plan2.log`): Plan 1 an Tag 1 erfüllt (+300 SM Prämie, Freischaltungen), Plan 2 an Tag 2–5 mit Sonderauftrag, Sonderwünschen, Kontrollen (Laken drauf/ab, bestanden, +400 SM), Sendungen, Bestellungen, Rekorden; 0 Fehler in der Konsole
- [x] Tests: 127 EditMode-Tests grün (5 neue)
- [x] Korrigiert im Test: URP entfernte im Build zwei Post-Processing-Shader und schaltete damit das ganze Post-Processing ab (Einstellung geändert, D99); Einstellungsfenster schien durch; gewählte Grafikstufe sah ausgegraut aus; Vollbild-Hintergründe bekamen das Schild-Sprite

### Nachtrag (auf Wunsch)
- [x] Sprinten mit Shift (gemessen: Gehen 5,19 m/s, Sprinten 8,28 m/s = ×1,6)
- [x] Einstellung „Anzeige“: Vollbild/Fenster, im Build geprüft (Start mit Fenster-Parametern wird auf Vollbild 1920 × 1080 umgestellt)
- [x] Tests: 130 grün (3 neue), Build neu erstellt, Player.log ohne Fehler

- [x] Klick-Bug behoben: nach Terminal, Panel, Pause oder Menü ist die Maus sofort wieder gefangen (geprüft: Schichtbeginn, Mischpanel auf/zu)
- [x] Speichern/Laden: automatisch zu Beginn jeder Schicht; geprüft mit einem gespielten Tag → Pause → Hauptmenü → Weiterspielen (Tag 2, Plan 2, Kasse 966, Rezepte wieder da), Schicht startet danach normal
- [x] Vollbild-Hauptmenü (Weiterspielen, Neues Spiel → Einsatzbesprechung, Einstellungen, Beenden); Pause mit „Hauptmenü“ und „Spiel beenden“, oberhalb der Untertitelleiste
- [x] Tests: 132 grün (2 neue: Spielstand übersteht Speichern/Laden, kaputte Datei wird ignoriert); Build neu, Probestart zeigt das Hauptmenü, Player.log ohne Fehler

### Befunde aus dem Durchlauf
- Kein Programmfehler. Der einfache Autopilot (immer 1-1-1, viele Hühner) schafft die 42 t Fleisch von Plan 2 nicht; nach mehreren verfehlten Tagen wird er entlassen (Wut 100). Das ist Spielregel, kein Fehler; ob Plan 2 für Menschen zu schwer ist, bitte selbst prüfen
- Ohne Bewegung steigt die Wut (Leerlauf, GDD 7.2); der Autopilot setzt den Leerlauf daher zurück wie der Prototyp-Testschritt
- Bei einem Kontrollbeginn direkt zum Schichtstart erscheint die Meldung schon vor dem Wechsel in die Schicht (unkritisch)

### Offen
- Ton nur technisch geprüft (Quellen, Tonhöhe, Lautstärke), nicht angehört
- Build-Warnungen (649) stammen aus den Shadern des Pakets `com.unity.ai.inference` (Unity-KI-Assistent), das mitgebaut wird; ohne den Assistenten wäre der Build kleiner
- Firmenname im Build ist noch „DefaultCompany“
- Aus früheren Phasen: Partikel laufen in Echtzeit statt Spielzeit, Schilder rechts überlappen teils, Baumodus-Kamera, Diagrammtitel im Studio, animiertes Direktor-Gesicht (D49)
- Nicht geprüft: Einstellungen und Pausemenü mit echter Maus, Grafik „Niedrig“ im Build

## Phase 11 – Grafik und Animation (Art-Pass)

- [x] Kuh und Huhn neu mit Skelett: eine durchgehende Haut aus Abstandsfeldern (Kopf, Hals, Beine und Schwanz verschmolzen), gehäutet auf Knochen (Kuh 14: Rumpf, Hals, Kopf, 3 Schwanzglieder, 4 Beine mit Knie; Huhn 4 plus Flügel und Beine), Gewichte weich über die Körperteile; Fleckenschale mit denselben Gewichten
- [x] Mutationen: Blendshape „Inflate“ (Rumpf bläht, Hals quer zur Achse; Kopf, Schwanz, Euter, Flügel, Zusatz-Euter und Halsband wandern mit), Euter wächst über den Knochen, Zusatz-Euter und Wucherungen wachsen weich heraus, Wachsen mit A weich statt sprunghaft
- [x] Animationen: Laufen mit Kniebeuge, Atmen (Blendshape „Breathe“, schneller bei INSTABIL), Umschauen und Nicken, Ohrenzucken, Schwanz pendelt als Kette, Huhn ruckt beim Laufen den Kopf, Beine baumeln beim Schweben, Rumpf zittert bei Instabilität; Glühen und Flecken-Tönung wie bisher. Neue Werte in `animalVisuals`
- [x] Deko nach Bild 07 und Prototyp (alles eigene Meshes): 5 Plakate und 2 Warnschilder mit Texten aus `posters`, 2 Glasregale mit 36 leuchtenden Gläsern, 4 Spinde, Büroecke unter dem Fenster (Schreibtisch, Röhrenmonitor, Schreibmaschine, Bücherstapel mit Titeln aus der JSON, Emaille-Tasse, Papiere, Reagenzglas-Ständer, Lampe mit gebackenem Licht, Stuhl), roter Teppich mit Emblem, 2 Aktenschränke, Feuerlöscher, Topfpflanze, Wanduhr (Zeiger = Schichtfortschritt wie Prototyp), Rohre an den Seitenwänden; Kisten mit Leisten und Fässer mit Sicken in abgenutzten Texturen
- [x] HUD und Panels im Industrie-Stil: dunkle Flächen als Metallschild (9-Slice mit Lichtkante, Messingnieten, Schmutz); Tier-Meter mit Flasche, Stern und Sanduhr
- [x] Konfetti im Fenster als deckende Papierschnipsel (vorher kaum sichtbar)
- [x] Tests: 122 EditMode-Tests grün (4 neue)
- [x] FPS im Editor (Play Mode, Kuh in der Kammer, Game View 1400 × 828): 111 FPS, CPU 8,4 ms, Render-Thread 4,2 ms, ca. 490 000 Dreiecke, 5 100 Draw Calls (SRP Batcher), 2 gehäutete Meshes
- [x] Vorher/Nachher: `Phase11_Vorher_*.png` (Kuh, Huhn, Labor) und `Phase11_Nachher_*.png` (Kuh, Kuh mutiert, Huhn, Huhn mutiert, Labor, Plakate, Büroecke, Schreibtisch, Rückwand, Warnschild)
- [x] Korrigiert im Test: Hals verschluckte beim Aufblähen Kopf, Ohren und Halsband; Wucherungen des Huhns schwebten neben dem Körper; Plakate trennen Schlagzeile und Unterzeile mit „–“, nicht „·“; Warnschild an der Mischstation war hinter den Tanks verdeckt

### Offen / für spätere Phasen
- Statisches Batching ist in den Player-Einstellungen aus; viele Deko-Teile sind einzelne Draw Calls (Phase 12, Performance)
- Licht der neuen Schreibtischlampe ist „Baked“ und wirkt erst nach erneutem Lichtbacken
- Aus früheren Phasen weiter offen: Partikel laufen in Echtzeit statt Spielzeit, Schilder rechts überlappen sich teils, Baumodus-Kamera, Diagrammtitel im Studio, animiertes Direktor-Gesicht (D49)
- Nicht geprüft: Tier-Animationen über einen ganzen Spieltag mit echter Eingabe (Posen über `Apply` im Play Mode geprüft)

## Phase 10 – Staats-TV, Logbuch, Rekorde, Fenster

- [x] `NewsLogic` (rein, getestet): Stufe nach Hungersnot (boom/warn/crisis/dead), Schlagzeile und Laufband aus `news.*` mit {year} {meat} {quality} {pop} {earned}, davor die verdrehten Taten (Explosion, Verarbeitungsexplosion, davongeschwebte Kuh, Infektion, Kuriositäten, Kontrollen, gelobter Sonderauftrag) und die Tageszeile; Sondersendung nach dem ersten Ultimativ; Fleisch- und Qualitätsprozent wie Prototyp `W.meat`/`W.quality`
- [x] Staats-TV als eigene Szene `NewsStudio` (additiv geladen, in den Build-Einstellungen): Studiowand mit Raster, Senderschild „ZF Staatsfernsehen“ mit LIVE bzw. PLANSENDUNG, Diagramm (Planerfüllung: Fleisch/Qualität bzw. „Bevölkerung (offiziell)“: Plan/Ist), Pult mit Logo und Leiste, Sprecher mit Atmen und Kopfbewegung, Pflanze, Tasse, Papier, Führungslicht; Kamerafahrt von der Totalen zum Sprecher; im Automatikbetrieb sackt der Sprecher zusammen (Kreuzaugen, grau-grün), Pflanze welkt, Fliegen kreisen, Licht flackert grünlich
- [x] Bauchbinde: Kennung, Zeitzeile, Schlagzeile, laufendes Band, „Weiter“ ab 1,5 s (auch Leertaste); das Labor-HUD ist während der Sendung ausgeblendet
- [x] Ablauf: nach jedem Tag automatisch (danach Bestellterminal bzw. Spielende), Sondersendung nach dem ersten Ultimativ, sonst über den Knopf „Staats-TV“ im Pausemenü (erst nach der ersten Mutation); dort auch „Logbuch“
- [x] Rekorde (`RecordLogic`, getestet): höchster Ertrag und meistes Fleisch bei der Verarbeitung, Tagesmengen je Ware beim Schichtende; erster Wert still, danach „NEUER REKORD: …“ und zu 50 % der Direktor
- [x] Forschungslogbuch (L): Notizbuch mit Ledereinband, Papier, Randlinie; Karten mit Bleistiftskizze (Kuh/Huhn, Größe nach A, Euter nach B, gestrichelter Rahmen bei C), Name, Dosis, Vor- und Nachteile; Abschnitte Geheimakte (gold), Kuriositäten (rosa), Rekordtafel; nur Entdecktes, sortiert nach Gesamtdosis
- [x] Fenster: Einöde als eigenes Bild (Hügel, Fabrik, Laternen, Stacheldraht), bei Hunger Warteschlange vor „MILCH · LEER“, bei satten Bürgern Festumzug mit Fahnen, Banner „DANKE, GENOSSE BIOCHEMIKER!“ und Konfetti; Figuren wippen
- [x] Tests: 118 EditMode-Tests grün (9 neue)
- [x] Spieltest: Schichtende → Sendung automatisch („SONDERMELDUNG … Fleisch +33 %“, Taten im Laufband) → Bestellterminal; Hungersnot → Automatikbetrieb mit zusammengesacktem Sprecher; Logbuch mit 7 Kombinationen, Ω in der Geheimakte, Milchhuhn und Singende Kuh, Rekordtafel mit den echten Tageswerten; Fenster mit Schlange und Festumzug. Konsole ohne Fehler. Screenshots `Phase10_*.png`
- [x] Korrigiert im Test: Studio war seitenverkehrt (x gespiegelt); Werte über den Balken saßen zu hoch; Labor-HUD lag über dem Fernsehbild; Karten im Logbuch etwas höher

### Offen / für spätere Phasen
- Ton (Jingle, Sprecher, Fanfare beim Rekord): Phase 12
- Diagrammtitel und Fleischwert berühren sich bei hohen Werten knapp; Studio-Feinschliff im Art-Pass
- Konfetti im Fenster ist schwach sichtbar
- Nicht geprüft: Knöpfe „Logbuch“ und „Staats-TV“ im Pausemenü und die Rekord-Meldung im laufenden Spiel (Logik getestet); erhöhte Logbuchkarten nach der Korrektur nicht erneut im Bild
- Zum Testen: Die Sendung läuft in unskalierter Zeit; für Aufnahmen über die Verbindung `Time.captureDeltaTime` setzen

## Phase 9 – Nebenstationen und Baumodus

- [x] `StationLogic` (rein, getestet): Grammophon (Kurbel 1,5 s → 30 s Volksmusik: Stabilität +15, Hitze −3/s, Parteikontrolle ×0,6, Verdacht ×0,7; 50 % Direktor-Spruch), Verkostungsautomat ab Plan 2 (30 SM, Siegel „STAATLICH GEPRÜFT“, Qualität +15, Spruch nach Qualität), Rohrpost (Kaviar: Wut −25, Direktor-Spruch), Probenmikroskop (20 SM, 2 s, nur für unbekannte Mischungen)
- [x] Darstellung: Platte und Kurbel drehen sich, Noten steigen aus dem Horn; Stempelarm schlägt beim Siegel, Lampe pulsiert bei ungeprüftem Tier; Kapsel saust das Rohr hoch; Mikroskopschale leuchtet während der Probe
- [x] Misch-Panel: Knopf „PROBE UNTERS MIKROSKOP · 20 SM“ und Ergebniszeile („Fleisch ↑↑↑ · Milch ↑↑↑ · Geschmack ↓ · Probe ruhig“)
- [x] Kuriositäten: erste Entdeckung mit Direktor-Spruch und Nachrichtenmeldung; Noten über der Singenden Kuh, Milchtropfen beim Milchhuhn, Münze pro Ei beim Münzhuhn; Wirkungen (Parteikontrolle −0,8/s, Verdacht ×0,5, Milch ×1,6, Münzen, Qualitätsabzug) aus Phase 4b/7 bleiben
- [x] `BuildLogic` (rein, getestet): Raster 14 × 3 mit gesperrtem Gang, Preis ×1,5 pro gleichem Typ, Freischaltung nach Plan, Bauen, Abriss mit 60 % Erstattung, Wirkung der Maschinen
- [x] Baumodus (B): Draufsicht wie Prototyp, Rückwand, linke Wand und Decke ausgeblendet, Raster mit gesperrten Feldern, Feldmarkierung grün/rot, halbdurchsichtige Vorschau, Leiste mit Preisen bzw. „GESPERRT · ab Plan n“, Tasten 1–4, X = Abriss, Rechtsklick = abwählen, B/Esc = zurück; das Spiel läuft weiter
- [x] Maschinen im Look-Pass: Melkpumpe (+1,4 t Milch/s bei Kuh, Qualität 85, Kolben arbeitet), Auto-Sprüher (alle 15 s das zuletzt benutzte Serum, sonst neuer Versuch nach 5 s), Kühlaggregat (Hitze −8/s, Lüfter dreht), Notfall-Automat (bei INSTABIL, Säure oder Überhitzung Gegenmittel, alle 3 s möglich); Rohre als Bodenleitungen zur Kammer; Stromausfall halbiert die Pumpenleistung
- [x] Neue Eingabegruppe „Build“ im Input-Asset (Mausposition, Platzieren, Abwählen, Werkzeuge 1–4, Abriss)
- [x] Tests: 109 EditMode-Tests grün (9 neue)
- [x] Spieltest: Grammophon mit Noten, Singende Kuh (3-0-3) mit Noten und Nachricht, Siegel (Qualität 80 → 95), Kaviar mit Spruch und Kapsel, Mikroskop-Ergebnis im Panel. Baumodus: 5 Maschinen gebaut (zweite Pumpe 600 SM, gesperrtes Feld abgelehnt, Kasse 6.000 → 3.550); Ertrag 2,0 → 4,8 t/s mit zwei Pumpen; Hitze 50 → 0 durch das Kühlaggregat; Auto-Sprüher hat nachgesprüht; Notfall-Automat hat bei Überhitzung Gegenmittel gegeben. Konsole ohne Fehler. Screenshots `Phase9_*.png`
- [x] Korrigiert im Test: Noten kamen neben dem Horn heraus; Rohrpost-Rohr schnitt das Schild; Bedienseite der Maschinen zeigte von der Kammer weg; Emaille zu rostig; „✕“ fehlt in der Schrift

### Offen / für spätere Phasen
- Kurbel-, Stempel-, Rohrpost- und Musikton: Phase 12
- Nachrichten zu Kuriositäten, Kontrollen und Ultimativ im Staats-TV: Phase 10
- Baumodus-Ansicht: Maschinen in der vorderen Reihe liegen teils hinter der Bauleiste; Kameraposition im Art-Pass nachjustieren
- Hinweis zum Testen: Dateien nicht ändern, während der Play Mode läuft (Unity kompiliert neu und verliert den Spielstand)
- Nicht geprüft: Baumodus mit echter Tastatur und Maus (getestet über die Methoden Enter/Select/Click); Lage der Noten am Grammophon nach der Korrektur und Rohrpost-Schild nach dem Versatz nicht erneut im Bild kontrolliert

## Phase 8 – Zentrifuge, Kryokammer, Ultimativ-Höhepunkt

- [x] `MachineLogic` (rein, getestet): Zentrifuge lädt das erste passende Serum (Rohserum oder normales, nicht konzentriertes), Drehzahl rpm += dt·(0,16 + 0,12·rpm); Bremsen unter 60 % ohne Wirkung, gelb/rot ×1,25, grün ×1,5 (Rohserum → zentrifugiert); ab 100 % Überdrehzahl, Serum verloren; Auslauf, dann zurück ins Regal (wartet bei vollem Regal)
- [x] Kryokammer ab Plan 2: zentrifugiertes oder normales, nicht tiefgekühltes Serum, 16 s einfrieren, 12 s Zeitfenster, sonst Eisblock; Ergebnis tiefgekühlt (+20 Stabilität) bzw. ab Plan 3 ultimativ, vorher „Die Probe reagiert nicht richtig …“
- [x] Drehzahlbalken im Prompt mit Zonen grau/gelb/grün/rot aus der Config und weißer Nadel; Hinweise im Plan-Panel
- [x] Zentrifuge sichtbar: Rotor dreht mit der Drehzahl, Gehäuse wackelt über 85 %, Lampe gelb/grün/rot, bei Überdrehzahl Glassplitter, Säurespritzer, Kamerawackeln
- [x] Kryokammer sichtbar: Fenster leuchtet und pulsiert beim Einfrieren, Kältenebel quillt aus der Tür, Lampe blinkt im Entnahmefenster (rot = gesperrt), vereistes Fenster nach Eisblock. Behoben: Die Leuchtscheibe saß hinter der Türplatte und war nie zu sehen
- [x] Ultimativ-Höhepunkt beim ersten Ω/Koloss: Bild auf 10 % (eigenes Volume), Tier leuchtet verstärkt, Direktor „… … …“, nach 2,5 s sein Spruch; nach 4,5 s Sondersendung (Kennung, Schlagzeile, Laufband, Spielzeit steht), danach weiter; Oberkommissar 25 s nach dem ersten Ultimativ. Weitere Ultimative: nur ein Spruch
- [x] Debug-Menü: Plan ±, Rohserum 2-3-5, Kryo sofort fertig, Anzeige von Drehzahl und Kryo-Zeit
- [x] Tests: 100 EditMode-Tests grün (10 neue, darunter die ganze Kette 2-3-5 → Zentrifuge grün → Kryo → Ω)
- [x] Spieltest nach Prompt: Plan 3, 2-3-5 gemischt (brodelndes Rohserum) → Zentrifuge bei 77 % (grün) gebremst → Kryokammer → „ULTIMATIVES SERUM: Planerfüller Ω“ → auf Kuh gesprüht → Sequenz, Sondersendung, Oberkommissar mit dunkelrotem Schild; Laken leuchtet golden. Unterwegs auch Überdrehzahl und Eisblock im Spiel ausgelöst. Konsole ohne Fehler. Screenshots `Phase8_*.png`
- Hinweis zum Test: Die Unity-Verbindung braucht mehrere Sekunden pro Befehl. Für das Timing an Zentrifuge und Kryo wurde die Spielzeit mit `GameClock.SetTimeScale` verlangsamt und das Hochdrehen bis 72 % bzw. der Rest des Einfrierens per Befehl übersprungen.

### Offen / für spätere Phasen
- Fanfare, Surren der Zentrifuge, Glasklirren, Warnton der Kryokammer: Ton in Phase 12
- Sondersendung als Einblendung; das volle Staats-TV-Studio mit Nachrichten zwischen den Tagen folgt in Phase 10 (dort auch die Meldung zum Ultimativ im Laufband)
- Laufband der Sondersendung steht still (wird abgeschnitten); Lauftext in Phase 10
- Nach dem Neubau des Labors markiert Unity die Szene einen Frame später erneut als geändert; vor Tests über die Verbindung deshalb in einem eigenen Befehl speichern

## Phase 7 – Parteikontrolle, Kommissar, Laken

- [x] `InspectionLogic` (rein, getestet): Parteikontrolle pro s (mutiertes Tier, niedrige Stabilität, Schweben, Glühen, Ω/Koloss, ×0,4 Laken, Singende Kuh, verseuchte Kammer, Abbau 0,6/s), Start ab Tag 2 bei Kontrolle ≥ 75, nach Zeitplan (erste 70 s nach Beginn von Tag 2, dann alle 100 s) oder Oberkommissar, mindestens 45 s Abstand
- [x] Verdacht pro s in 4,2 m um die Kammer: ohne Laken 22 (Koloss 40), mit Laken 2 + 0,6 × Mutationsgrad (+6 instabil, Ω 9, Koloss 12), verseuchte Kammer +6; Multiplikatoren Oberkommissar/Musik/Singende Kuh; Verdacht 100 → Skandal (Spielende)
- [x] Serum-Regal: verdächtig bei Ultimativ, Roh-/Zentrifugenserum oder A+B ≥ 5; +12/s, wenn der Kommissar an der Mischstation vorbeigeht; Warnung „beäugt das Regal“; an der Mischstation wird während der Kontrolle aus „Mischstation benutzen“ „Serum-Regal abdecken!“ (0,5 s), eine Plane senkt sich über das Regal
- [x] Wodka: am Kommissar 0,5 s halten → 10 s Pause (er hebt die Flasche und schwankt), Verdacht −40
- [x] Kontrolle bestanden: +400 SM (Oberkommissar ×2), Wut −10, Parteikontrolle auf 15, nächste Kontrolle in 100 s, Nachrichtenmeldung vorgemerkt (für Phase 10); Direktor kommentiert Beginn und Ende
- [x] Oberkommissar: wird beim ersten Ultimativ für 25 s später angekündigt (Ultimativ selbst folgt in Phase 8)
- [x] Kommissar-Figur: graue Uniform mit Mantel, Schirmmütze mit rotem Band, Schnurrbart, Klemmbrett, Stiefel; geht mit Bein- und Armschwung, dreht den Kopf zur Kammer; Schild KOMMISSAR/OBERKOMMISSAR und Verdachtsbalken drehen sich zur Kamera
- [x] Laken neu: Leinen mit Falten, gewölbter Haube und rotem Saum; fällt von oben herab und wird hochgezogen; verrät das Tier (Wackeln je Mutationsgrad, Zittern bei INSTABIL, goldenes Leuchten bei Ω)
- [x] Hinweise im Plan-Panel („Kommissar! … abdecken“, „… Laken überwerfen“), Debug-Menü: Kontrolle/Oberkommissar starten, Kontrolle +25, Verdacht +25, Kommissar zur Kammer
- [x] Tests: 90 EditMode-Tests grün (10 neue für die Kontrolle, darunter die beiden Prompt-Fälle)
- [x] Spieltest: Tag 2, Kuh mit 1-1-0 ohne Laken → Kontrolle → Verdacht 100 → „Skandal in der Säule“. Neues Spiel, Kuh mit 1-1-0 unter dem Laken, verdächtiges Serum (3-3-0) im Regal abgedeckt, Wodka angeboten → Verdacht 18 → „Kontrolle bestanden. Verschwiegenheitsprämie +400 SM“. Konsole ohne Fehler. Screenshots `Phase7_*.png`

### Offen / für spätere Phasen
- ~~Musik als Multiplikator für Parteikontrolle und Verdacht~~ in Phase 9 angebunden
- ~~Ω-Leuchten des Lakens und Oberkommissar~~ in Phase 8 im Spiel geprüft
- Nachrichtenmeldung nach der Kontrolle: Phase 10
- Hinweis: Tests über die Unity-Verbindung erst nach dem Speichern der Szene starten, sonst blockiert der Dialog „Scene(s) Have Been Modified“ den Editor

## Phase 6 – Tagesablauf, Bestellung, Direktor

- [x] `DayLogic` (rein, getestet): Schichtbeginn mit Soll aus `day.targets`, Prämie, Sonderauftrag ab Tag 2, Einführung an Tag 1; Tageszeit, Plan erfüllt (Prämie, Wut −20), Zeit um, Tagesende mit Wiederholung bei Verfehlen (Wut +34), Entlassung bei Wut 100, Ende nach dem letzten Plan
- [x] Bezahlung über `DayLogic.Pay`: Übererfüllung (+Bonus, einmalige Meldung), Sonderauftrag ab Mindestqualität, Sonderwünsche (Fleisch, Eier, Qualität, Ruhe …) mit Frist, Belohnung bzw. Wut
- [x] Einführung in 4 Schritten (Mischen → Klonen → Sprühen → Verarbeiten), Hinweis im Plan-Panel, Direktor spricht jeden Schritt
- [x] Leerlauf-Wut wie im Prototyp (nach 6 s ohne Bewegung, Halten oder offenes Panel)
- [x] `Director`: Sprüche je Wut-Stufe, zufällige Zurufe, Reaktion auf Risiken und Vernichtung; Untertitel unten (`SubtitleUI`), Text auf dem Parolenfeld des Fernsehers (`TvDirectorFx`, cholerisch: Flackern und rotes Licht), roter Bildrand ab Stufe 3 (`RageBorder`)
- [x] `DayCycle`: Startbildschirm → Bestellterminal → Schicht → Tagesende → nächste Bestellung bzw. Endbildschirm; Spieluhr steht während der Overlays (`GameClock.Held`)
- [x] Bestellterminal (`OrderTerminalUI`): A/B/C, Gegenmittel, Wodka, Kaviar mit Preis, Lagerstand und Summe; eine Bestellung pro Tag
- [x] HUD: Plan-Panel (Ist/Soll mit Balken, Restzeit, Wunsch, Hinweis), Spielwerte-Panel (Nahrung, Volk, Umwelt, Wut, Parteikontrolle über `WorldValues`), Uhrzeit in der oberen Leiste
- [x] Welt: Plantafel mit Balken und Zahlen (träge, alle 10 s), Wut-Skala grün/gelb/rot
- [x] Endbildschirm mit Endung aus `endings.*`, Statistik und „Neuer Fünfjahresplan“; Infektions-Zusammenbruch am Tagesende eingebunden
- [x] Tests: 80 EditMode-Tests grün (12 neue für den Tagesablauf)
- [x] Spieltest Tag 1: Startbildschirm → Bestellung (1× B, Kasse 700 → 640) → Einführung komplett (1× B gemischt, Kuh geklont, gesprüht, verarbeitet: 60 t Milch, 6,6 t Fleisch, Schritt −1) → Zeit abgelaufen → Plan verfehlt, Wut +34, Bestellung Tag 2 mit Sonderauftrag → Wut 100 → „Fristlos entlassen“ → Neustart. Konsole ohne Fehler. Screenshots `Phase6_*.png`
- [x] Behoben: Statistik-Spalten im Endbildschirm liefen wegen unterschiedlicher Schriften auseinander (feste Zeilenhöhe)

### Offen / für spätere Phasen
- Staats-TV und Nachrichten zwischen den Tagen: Phase 10
- Kommissar-Besuch, Wodka anbieten: Phase 7; Kaviar per Rohrpost: Phase 9
- Direktor-Stimme bzw. Ton: Phase 12

## Phase 5 – Risiken, Warnzeichen, Explosion, Infektion

- [x] `RiskLogic` (rein, getestet): Hitze, Säure-Milch, Schweben bis Davonfliegen, Bandriss, Kabelriss/Stromausfall, aktuelle Stabilität, Krisen, INSTABIL mit 8-s-Countdown – alle Werte aus `risks` und `warnings`
- [x] Warnzeichen je einmal pro Tier mit Ursachen-Toast (Dampf, Blasen, Abheben, Zittern) und sichtbar am Tier (`AnimalCrisisFx`: Dampf, Flammen, Blasen, Säurespritzer, Schleim)
- [x] Rote Rundum-Warnleuchte auf dem Kopfring bei INSTABIL (`ChamberAlarm`)
- [x] Explosion: Schleim- und Feuerwolke, Kamerawackeln, Schleimflecken am Boden und Brandfleck (bis zur Reinigung), Kammer verseucht, Laken zerfetzt, Parteikontrolle +20, Wut +10
- [x] Stromausfall: Hauptkabel reißt an zufälliger Stelle mit Funken, Bild auf 20 % (eigenes Volume), 12 s, Produktion ×0,5, Klonen/Sprühen/Band gesperrt; „Strom ist wieder da“
- [x] Bandriss: Lampe am Bandmotor blinkt rot, Reparatur am Bandmotor (1,6 s halten)
- [x] Verarbeitung mit Mut-Bonus: 30 % Explosion (kein Fleisch, Wut +10), sonst +150 SM
- [x] Infektion (`InfectionLogic` + `InfectionSystem`): ab Tag 2 bei Explosion (näher als 7 m oder 50 %), Frist bis Ende des nächsten Tages, steigender Infektionswert, Symptome alle 10–22 s für 7–12 s (grün, schnell ×2,4, bleischwer ×0,45, verschwommen, schwankend, invertiert), grüner Schleier und Anzeige „INFEKTION · Frist Tag n“ im HUD, Heilung mit G
- [x] Debug-Menü (F1): Tag ±, Kuh klonen, Serum 2-2-0, Sprühen, NOT-STOPP, Explosion, Stromausfall, Band reißen, Infizieren; Anzeige von Wut, Kontrolle, Hitze, Säure, Schweben
- [x] Tests: 68 EditMode-Tests grün (9 neue für Risiken und Infektion)
- [x] Spieltest im Zeitraffer: 2-2-0 auf Kuh → Warnungen → INSTABIL → NOT-STOPP vernichtet ohne Explosion; zweite Kuh ohne NOT-STOPP → Explosion (verseucht, Wut 10, Kontrolle 20, Flecken). Stromausfall, Bandriss mit Reparatur, Infektion mit Symptom „schnell“, grüne und verschwommene Sicht geprüft. Konsole ohne Fehler. Screenshots `Phase5_*.png`

### Offen / für spätere Phasen
- Sirene, Zischen, Blubbern und Explosionsknall als Ton: Phase 12 (jetzt nur sichtbar)
- ~~Zusammenbruch bei abgelaufener Frist und Hinweis „LETZTER TAG“~~ in Phase 6 erledigt
- Anstieg der Parteikontrolle pro Sekunde bei mutiertem Tier: Phase 7 (jetzt nur +20 bei Explosion)
- Kühlaggregat (Phase 9) und Musik (Phase 9) für schnelleres Abkühlen; Nachrichtenmeldungen zu Explosionen: Phase 10
- Partikel laufen in Echtzeit statt in Spielzeit (Pause hält sie nicht an); im Art-Pass angleichen

## Look-Development – Kammer B-12 und Pult aus Ego-Sicht (Bild 08)

- [x] Editor-Werkzeuge `Scripts/Editor/LookDev/`: `LookDevKit` (Materialien, Meshes mit Welt-UVs, Schrauben, Schläuche als Bézier-Rohre), `LookDevMaterials`, `LookDevChamber`, `LookDevPult`, `LookDevHall`, `LookDevCapture` (Menü „Plan erfüllt/Look-Dev: Aufnahme Ego-Sicht“)
- [x] Kammer: Sockel und Kopfring aus Stahl mit Flanschen und Schraubenkränzen, Messingbänder, Streben, doppelwandiges Glas, drei Düsen mit grünem Spotlicht und weichem Lichtkegel, Nebelpartikel, „B-12“ und Plakette
- [x] Pult: drei Glastanks mit Etiketten und Schläuchen zur Kammer, Mischanzeige, Statustafel, NOT-STOPP mit Klappe und Warnstreifen, drei Kipphebel (bewegt von `PultLevers`)
- [x] Halle: Boden und Wände mit CC0-Texturen, Laufstege mit gelbem Geländer, Rohrbündel, hohe Banner mit Emblem, Wandschild, Wandlampen, warmer Dunst; TV (Direktor + Parolen), Plantafel (Milch/Eier/Fleisch), Wutskala, Klonanlage im neuen Material
- [x] Eigener Shader `Art/Shaders/LightShaft.shader` für die Lichtkegel
- [x] Screenshots `Docs/Screenshots/LookDev_00` bis `LookDev_03`
- [x] Abnahme des Looks durch den Auftraggeber
- [x] Spieltest im Play Mode mit echtem Laufen: Mischzone → 1-1-0 gemischt → zum Pult gelaufen (Pult-Kollider stoppt bei z −7,17) → Kuh geklont → gesprüht (Stabilität 62, 3,9 t/s) → verarbeitet (10,8 t Fleisch), Werte wie im 4b-Referenztest
- [x] Kollider: Pult-Kollider passt auf 5 cm zum Modell; neuer Kollider über dem Schlauchbündel; Kammersockel auf 2,8 m verkleinert (Spieler bleibt 3,1 m entfernt); Gang zur linken Lakenzone frei (Prompt „Laken über die Kammer werfen“ erreicht)
- [x] EditMode-Tests 59 grün (Layout-Test auf neue Pult-Werte angepasst); Screenshot `LookDev_04_Spieltest.png`
- [x] Kuh und Huhn als glatte organische Modelle (eigener SDF-Mesher, `SdfMesher` + `AnimalBuilder`): Kuh mit Holstein-Flecken, Euter, Hufen, Halsband, Ohrmarke; Huhn mit Kamm, Kehllappen, Flügeln, Schwanzfächer. Wachsen, Aufblähen, Zusatz-Euter, Glühen, Laufen funktionieren unverändert
- [x] Hubtüren: Schleusen links (Einlass) und rechts (Auslass) mit Führungsschienen, Antrieb und Warnlampe. Ablauf: linke Tür auf → Tier läuft ein (wartet, bis die Tür offen ist) → Tür zu → Gas erst bei geschlossenen Türen → zum Verarbeiten rechte Tür auf → Tier läuft aus → Tür zu. Glas und Sockel haben Öffnungen
- [x] Vernichtung (Not-Stopp): Feuer statt Gas (Flammenteppich, Stichflammen aus den Düsen, Rauch, flackerndes Feuerlicht, Düsen und Kegel orange, grüner Nebel wird Qualm)
- [x] Maschinen neu im Look-Pass (`LookDevMachines`): Verarbeitung, Bandmotor, Mikroskop, Zentrifuge, Kryokammer, Verkostungsautomat, Grammophon, Rohrpost, Bestellterminal, Lager; Mischstation im Pult-Look. Rückwand mit Bannern, Parolentafel und Lampen
- [x] Spieltest: Türablauf geprüft (Kuh wartet bei x −5,5, Sprühen bei Türstand 0,02 abgelehnt, bei 0 erfolgreich, Auslass mit Warten, 10,8 t Fleisch), Mutation 2-3-1 mit Glühen, Not-Stopp mit Feuer. Konsole ohne Fehler, 59 EditMode-Tests grün. Screenshots `LookDev_05` bis `LookDev_09`

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
