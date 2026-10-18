# Entscheidungen

Abweichungen von den Übergabedokumenten und eigene Festlegungen. Neueste unten.

## Vom Auftraggeber entschieden (Prompt 0, 2026-10-16)

| # | Thema | Entscheidung |
|---|---|---|
| E1 | Grafikstil | Es gelten die Art Direction (04) und die Bilder 07/08: stilisierter Realismus. „Low-Poly“ in GDD Abschnitt 3 ist ein Fehler. |
| E2 | Basis-Stabilität S0 | Wie im Prototyp (`stabOf`): Kryo-Bonus und Musik zählen zu S0 und senken damit auch die Risiken. |
| E3 | UI-Texte | Die Text-Arbeitskopie bekommt einen Abschnitt `ui` mit allen Texten aus dem Prototyp (Toasts, Prompts, Hinweise, Beschriftungen, Fehlermeldungen). |
| E4 | `prototypeTexts` | Nur `news` und `endings` werden genutzt, alles andere kommt aus `director`. Veraltete Teile sind in der Kopie gelöscht. |
| E5 | Ende „fail“ | Bleibt vorerst ungenutzt, die Wut beendet das Spiel. Text bleibt erhalten (spätere Grenze für verfehlte Tage möglich). |
| E6 | Unity-Version | Unity 6.6 (6000.6.6f1), kein Wechsel auf 6.0 LTS. |
| E7 | Assets und Ton | Zuerst freie Quellen (Kenney, Quaternius, Poly Haven), Ton prozedural oder freie SFX. Unity AI nur für Kuh, Huhn und Direktor-Bild, vorher Kosten nennen. Alle Quellen in Credits.md. |
| E8 | Schriften | Oswald, JetBrains Mono, Caveat (OFL) von Google Fonts, Lizenzen liegen bei den Schriftdateien. |
| E9 | Versionsverwaltung | Git im Projektordner, nach jeder Phase ein Commit mit Phasennummer. |
| E10 | Phase 4 | Darf in 4a (Panels) und 4b (Tier) geteilt werden. |

## Übernahmen aus dem Prototyp (bestätigt)

| # | Thema | Festlegung |
|---|---|---|
| P1 | Panels | Pult, Mischstation, Bestellterminal usw. werden mit der Maus bedient; dafür wird der Mauszeiger freigegeben. |
| P2 | Tag 1 | Der 180-s-Timer läuft auch während der Einführung. |
| P3 | Melkpumpe | Gepumpte Milch zählt mit Qualität 85 in die Durchschnittsqualität. |
| P4 | Sonderwunsch „Ruhe“ | „Krise“ heißt: ÜBERHITZT, SÄURE-MILCH oder SCHWEBT DAVON. |
| P5 | Endtexte | Es gelten die Texte aus der JSON, nicht die Ersatztexte im Prototyp-Code. |

## Eigene Festlegungen (Phase 1)

| # | Thema | Festlegung und Grund |
|---|---|---|
| D1 | Ablage der JSON | `Docs/` enthält die unveränderten Originale. Die Arbeitskopien liegen in `Assets/_Project/Data/Json/Balancing.json` und `Texte_de.json` und werden vom Spiel genutzt. |
| D2 | Formel-Strings | In der Balancing-JSON standen manche Werte nur als Formeltext (z. B. `"stability": "clamp(100-14A…)"`, `"reward": "150+50*planIdx"`, Weltwerte, Zentrifugen-Drehzahl, gesperrte Rasterzellen, Nachrichtenstufen). In der Arbeitskopie sind daraus Zahlenfelder geworden; der Originaltext steht als `_doc` daneben. So steht keine Zahl im Code. |
| D3 | Neue Zahlenfelder | Werte, die nur im GDD oder Prototyp standen, sind in die Balancing-Kopie gewandert: Schwellen für Kombinationsnamen und Warnungen (`combo`), Mindestdosen der Risiken (`heatMinA` usw.), `infectionFromDay`, Mikroskop-Schwellen, Rufabstand des Direktors, Maxima (`max`, `suspMax`, `statMax`), Werte der Baumaschinen (`milkPerSec`, `intervalSec`, `heatPerSec`), Spielend-Schwellen (`endings`). Spätere Phasen ergänzen weitere Felder nach demselben Muster. |
| D4 | Textaufbau | `prototypeTexts.news` und `.endings` liegen in der Kopie als oberste Schlüssel `news` und `endings`. |
| D5 | JSON-Bibliothek | Newtonsoft JSON (Unity-Paket `com.unity.nuget.newtonsoft-json`), weil `JsonUtility` keine verschachtelten Listen und Objekte mit Schlüsseln lesen kann (Direktor-Listen je Wut-Stufe, `ultimates`, `comboNames`). |
| D6 | Importer | Menü *Plan erfüllt > Balancing importieren*; läuft zusätzlich automatisch, wenn `Balancing.json` gespeichert wird. Ultimative Seren, Kuriositäten und Baumaschinen werden eigene Assets (`UltimateSerum_*`, `FunRecipe_*`, `BuildMachineType_*`). |
| D7 | Editor-Code | Editor-Scripts liegen in `Scripts/Editor/` (eigene Assembly `PlanErfuellt.Editor`, Namespace `PlanErfuellt.EditorTools`) statt in `Scripts/Config/`, damit sie nicht in den Build gelangen. |
| D8 | Assemblies | `PlanErfuellt` (Laufzeit), `PlanErfuellt.Editor`, `PlanErfuellt.Tests.EditMode` (Tests unter `Assets/_Project/Tests/EditMode`). |
| D9 | GameManager | Liegt als Prefab in Lab und TestRange (Architektur 05: „Systems: GameManager“). Die Boot-Szene lädt nur Lab. Kein DontDestroyOnLoad. Zugriff über `GameManager.Instance`. |
| D10 | Debug-Menü | IMGUI-Fenster (F1), aktiv nur bei `Debug.isDebugBuild` (Editor und Development Build). Beschriftungen stehen im Code, weil es ein Entwicklerwerkzeug ist und keine Spieler-UI. |
| D11 | Runden | Anzeigewerte werden wie im Prototyp (`Math.round`) kaufmännisch gerundet (`Formula.RoundHalfUp`). |
| D12 | Formeln am Tier | `Formula.AnimalRate` enthält Ultimativ und Konzentration; Säure, Stromausfall und Milchhuhn-Faktor wendet später das Tier an (wie Prototyp `updateSubject`). |
| D13 | Zahlenformat in Texten | Platzhalter werden mit deutscher Formatierung gefüllt (Komma als Dezimaltrennzeichen). |
| D15 | Run In Background | `PlayerSettings.runInBackground = true`, damit der Play Mode über die Unity-Verbindung getestet werden kann, auch wenn der Editor keinen Fokus hat. Pause bei Fokusverlust wird in Phase 3 mit dem Pausenmenü entschieden. |
| D16 | Zeitsteuerung | Spielsysteme nutzen `GameManager.Instance.Clock.DeltaTime` (nicht `Time.deltaTime`). Ein Frame-Schritt wird auf 0.1 s begrenzt (Feld am GameManager), damit Ruckler keine Sprünge erzeugen. |
| D17 | Layout als Daten (Phase 2) | Koordinaten aus GDD 3 stehen in `Balancing.json → layout` (Prototyp-Koordinaten, Umrechnung `LayoutConfig.ToUnity`: x bleibt, z → −z). Dazu Raummaße, Spielerbereich, Startpunkt, Kammer (inkl. Kollisionsradius 3.1), Band, Pult- und Laken-Zone, Fenster und alle Stationen mit Kollidergröße und Schild. `facing` ist in Unity-Grad; die Vorderseite jeder Station zeigt bei 0° zum Spieler (−Z). |
| D18 | Blockout-Werkzeug | Menü *Plan erfüllt > Labor-Blockout bauen* erzeugt Halle, Licht und Stations-Prefabs aus `layout`. Vorhandene Prefabs bleiben erhalten (für spätere Handarbeit), *(Prefabs neu)* erzeugt sie neu. Die Maße der Platzhalter-Modelle stehen im Werkzeug (Modellierdaten wie im Prototyp, keine Spielwerte). |
| D19 | ProBuilder | Für den Blockout nicht nötig; Ringe und Kegel erzeugt `MeshFactory` als Mesh-Assets mit Lightmap-UVs. ProBuilder bleibt installiert für spätere Handarbeit. |
| D20 | Schilder | `StationSign` holt den Text zur Laufzeit aus der Text-JSON (`ui.signs.*`) und dreht sich wie im Prototyp zur Kamera; feste Wandschilder drehen sich nicht. Neue Schlüssel in `ui.signs` (Kammerschild, Wandslogan, Banner, Tank-Etiketten). |
| D21 | Licht | Warmes Hauptlicht (Directional, Mixed, weiche Schatten) wie im Prototyp von oben-vorne; die Decke wirft keinen Schatten, damit es hereinfällt. 6 gebackene Lampen, gebackenes Fensterlicht, Echtzeit nur Kammerlicht (grün) und TV-Licht (rot), beide ohne Schatten. Lightmapper GPU, Baked Indirect, Light Probes und eine gebackene Reflexionssonde. Nebel exponentiell, Post-Processing: ACES, Bloom, Vignette, Farbkorrektur warm, Split Toning grün in den Lichtern, leichtes Filmkorn. |
| D22 | TextMeshPro | TMP Essential Resources importiert (`Assets/TextMesh Pro`). Dynamische Font-Assets aus Oswald, JetBrains Mono und Caveat (Menü *Plan erfüllt > Schriften für TextMeshPro erzeugen*). |
| D23 | Config-Generator | Die einfachen Datenklassen in `Scripts/Config/` werden mit `Tools/gen_config.py` erzeugt (eine Datei pro Klasse). Neue Abschnitte dort ergänzen und das Skript erneut ausführen. |
| D24 | Eingabe (Phase 3) | Eigenes, projektweites Input-Actions-Asset `Settings/PlanErfuellt_Input.inputactions` statt der Vorlage. Pfeiltasten hoch/runter laufen, links/rechts drehen (wie Prototyp). Zugriff nur über `InputReader`. |
| D25 | Halten | Eine Aktion startet, solange die Leertaste gehalten ist (nicht nur im Frame des Drückens); nach dem Ausführen erst wieder nach Loslassen. Wechselt das Ziel, beginnt der Balken neu. Entspricht dem Prototyp (`act.down`/`act.consumed`). |
| D26 | Zielwahl | Wie Prototyp `pickTarget`: nur Position, keine Blickrichtung. Pult und Laken über Zonen mit fester Wertung (0,4 / 0,5), Mischstation über einen Bereich davor, sonst Kreis mit `reach` je Station. Werte in `interaction` und `layout.stations[].reach`. |
| D27 | Kollision | CharacterController (Radius 0,3 m) gegen Kollider der Stationen, Kisten und Fässer; Spielerbereich und Kammer-Mindestabstand (3,1 m, Mitte zu Mitte) werden wie im Prototyp zusätzlich exakt erzwungen. Wände brauchen keine Kollider, weil der Spielerbereich innerhalb liegt. |
| D28 | Laken und Bandmotor | Laken werfen/abnehmen und Band reparieren funktionieren bereits (nur Zustand und Optik); Wirkungen auf Kontrolle und Risiken folgen in Phase 5/7. Alle anderen Stationen zeigen eine Platzhalter-Meldung mit der Phase, in der sie kommen. |
| D29 | Test-Übersteuerung | `InputReader.TestMove` / `TestInteract` ersetzen Eingaben nur für automatisierte Tests, weil im pausierten Einzelschritt-Modus simulierte Tastendrücke nicht bei den Actions ankommen. |
| D30 | Animationswerte | Feinwerte der Hand-Animation (Atemfrequenzen, Kolbenweg) stehen als Animationsdaten im `ViewModelHands`; Amplituden und Geschwindigkeiten stehen in `player.hands`. |
| D31 | Panels (Phase 4a) | uGUI-Panels unten mittig wie im Prototyp, im HUD-Prefab. Öffnen über `EventBus.PanelRequested("pult"/"mixing")`, damit Stationen keine Szenenverweise brauchen. Panels laufen in Echtzeit weiter (Spiel pausiert nicht), wie im Prototyp. Auffrischen alle `pult.panelRefreshSec`. |
| D32 | Rückmeldungen | Aktionen liefern `Feedback` (Textpfad + Platzhalter) statt direkt Toasts; der Aufrufer zeigt sie als Warn-Meldung (Prototyp `toastR`). So bleibt die Logik testbar. |
| D33 | Kammer-Schnittstelle | `Chamber` ist ab 4a die Schnittstelle des Pults; in 4b kommt das Tier dazu, ohne dass sich das Panel ändern muss. |
| D34 | Neue Werte | `mixing.stirSpeedDeg`, `mixing.centrifugedColor`, Abschnitt `pult` (Reinigungsdauer, Hebelanimation, Lampenschwellen, Anleinen ab Schwebehöhe 0,5, Auffrischtakt). `interaction.hold.clean` entfernt, weil die 3 s eine Reinigungsdauer sind, keine Haltezeit. |
| D35 | Probenmikroskop | Wie im Prompt-Plan erst in Phase 9, obwohl GDD 6.2 es im Misch-Panel beschreibt. `Formula.Probe` und Texte sind schon vorhanden. |
| D36 | Tier (Phase 4b) | Eine Kammer mit wiederverwendeten Kuh- und Huhn-Modellen (wie Prototyp), keine Instanzen pro Klon. Bewegungs- und Optikwerte in `animal` und `animalVisuals` der Balancing-JSON. |
| D37 | Bezahlung | Produktion bezahlt sofort pro Tonne (`day.payPerTon`). Übererfüllung, Quote, Aufträge und Wünsche kommen in Phase 6 in `Production.Deliver` dazu. |
| D38 | Rezept-Ids | Sonderfälle werden über die Ids aus der JSON erkannt (`omega`, `koloss`, `milkhen`) wie im Prototyp. |
| D39 | Instabil in 4b | Ein Tier wird INSTABIL, sobald die Formel-Stabilität 0 erreicht (z. B. 2-3-1); nach 8 s explodiert es vereinfacht (Kammer verseucht). Die volle Explosion folgt in Phase 5. |
| D14 | Vorlagen entfernt | Mobile-Quality-Stufe samt `Mobile_RPAsset`/`Mobile_Renderer`, `SampleScene`, `SampleSceneProfile`, `TutorialInfo`, `Readme` gelöscht. `InputSystem_Actions.inputactions` bleibt bis Phase 3 liegen. |

## Look-Development (2026-10-17)

| Nr. | Thema | Entscheidung |
|---|---|---|
| L1 | Quellen | Texturen kostenlos von Poly Haven (CC0), Emblem, Direktor, Plantafel, Etiketten und Verläufe selbst erzeugt (`Tools/gen_lookdev_textures.py`). Modelle aus eigenen Meshes per Editor-Skript. Keine Unity-AI-Generierung (kostet Credits). |
| L2 | Kammerhöhe | `layout.chamber.height` von 8,3 m auf 4,5 m gesenkt (GDD: 8,3), damit Kopfring und „B-12“ wie in Bild 08 im Bild sind. Vom Auftraggeber freigegeben. |
| L3 | Pult-Position | Pult näher an der Kammer (`z` 6,2), `pultZone` angepasst (z 5,0–8,4, |x| bis 2,6). |
| L4 | Wandtafeln | TV (x 7,4, y 5,9), Plantafel (x 7,4, y 3,35) und Wutskala (x 3,95, y 5,9) an die Wand hinter der Kammer rechts versetzt, wie in Bild 08. Alte kleine Banner und der Wand-Slogan hinter der Kammer entfallen zugunsten hoher Banner. |
| L5 | Sichtfeld | `player.fov` 60 (vorher 66, kurz 50). Kommt Bild 08 am nächsten. |
| L6 | Volumetrik | Das MIT-Paket „URP Volumetric Fog“ wurde getestet und wieder entfernt: Mit URP 17.6 streuen nur das Hauptlicht (in der geschlossenen Halle im Schatten), nicht die Düsen-Spots. Stattdessen eigener Lichtkegel-Shader (`LightShaft`) plus Nebel- und Dunstpartikel. Billiger und für 60 FPS sicher. |
| L7 | Echtzeitlichter | In der Kammer 3 grüne Düsen-Spots ohne Schatten, dazu Kammerlicht, Tierglühen und TV-Licht. Das sind mehr als „max. 4“ aus der Art Direction, aber ohne Schatten und mit kleiner Reichweite (Forward+). Wird in Phase 12 gemessen. |
| L8 | Wandlampen | 5 zusätzliche gebackene Punktlichter an den Wandlampen; kosten zur Laufzeit nichts. |
| L9 | Augenhöhe | Spieler-Augenhöhe bleibt 1,7 m. Die Vergleichsaufnahme mit Bild 08 entsteht mit 2,0 m, weil Bild 08 von etwas höher blickt. |
| L10 | Kollider | Kammersockel endet bei 2,8 m (= `playerCollisionRadius` 3,1 minus Spielerradius 0,3), damit der Spieler nicht auf dem Ring steht. Über dem Schlauchbündel zwischen Pult und Kammer liegt ein eigener Box-Kollider (aus den Schlauch-Bounds berechnet). |
| L11 | Layout-Test | `ConfigImportTests.Layout` erwartet jetzt Pult z −6,2 und `pultZone.absXMax` 2,6 (Werte aus L3). |
| L12 | Tiermodelle | Kuh und Huhn werden per Editor-Skript aus Abstandsfeldern erzeugt (Surface Nets, weiche Vereinigung), kostenlos und ohne Fremd-Asset. Bewegliche Teile bleiben getrennt, damit `AnimalVisuals` unverändert bleibt. Kuh ca. 75 000, Huhn ca. 35 000 Vertices (sichtbar). |
| L13 | Hubtüren | Neue Abschnitte `chamberDoors` (Zeiten, Maße, Wartabstände) in der Balancing-JSON. Gas (Sprühen) nur bei geschlossenen Türen; neuer Fehlertext `ui.errors.doorsMoving`. Ohne Türen-Komponente läuft die Kammer wie im Prototyp. |
| L14 | Feuer | Neuer Abschnitt `chamberFire`. Das Feuer ist rein visuell und dauert `animal.destroySec` + `chamberFire.afterSec`; Ablauf und Werte der Vernichtung bleiben wie im Prototyp. |
| L15 | Laken | Laken jetzt so hoch wie Kammer + Kopfring (statt fest 8,4 m) und mit Radius 3,05 m innerhalb des Spielerabstands; die Schleusen ragen heraus. |
| L16 | Texturen | Zwei weitere CC0-Texturen von Poly Haven (`dark_wood`, `old_planks_02`) für Grammophon und Kisten. Kistenaufschriften als Texte (`ui.signs.crateA` usw.). |
| L17 | Mesh-Überschreiben | Vorhandene Mesh-Assets werden über die Mesh-API überschrieben (`SdfMesher.CopyInto`), nicht mit `CopySerialized`, weil sonst die GPU-Daten veraltet bleiben (zerknitterte Tiere beim ersten Versuch). |

## Phase 5 (2026-10-17)

| Nr. | Thema | Entscheidung |
|---|---|---|
| D40 | Risiko-Logik | Risiken laufen in `RiskLogic.Step` (rein, mit Zufallsfunktion) und melden `RiskEvent`s. Die Kammer macht daraus Meldungen und Zustände, Effekte und Kamerawackeln hören auf `EventBus.Risk`. |
| D41 | Neue Werte | `risks.blackoutLightFactor` (GDD: Licht 20 %), `risks.heatMusicDecay` (GDD: Musik −3/s), Abschnitt `fx` (Wackelstärken und Abbau wie Prototyp `shake`, Anzahl und Abstand der Schleimflecken, Drehtempo der Warnleuchte) und in `infection` die Prototyp-Werte für Infektionswert, Schleier, erstes Symptom; `wobbleDeg`/`blurRadius` sind eigene Festlegungen. |
| D42 | Kamerawackeln | Stärke ×0,6 gegenüber dem Prototyp (`fx.shakeScale`), weil ±0,45 m Versatz in Ego-Sicht zu heftig wirkt. |
| D43 | Sirene | Vorerst nur eine drehende rote Warnleuchte auf dem Kopfring; der Sirenenton kommt mit dem Ton in Phase 12. |
| D44 | Stromausfall-Licht | Abdunkeln über ein eigenes Post-Processing-Volume (Belichtung log2(0,2)), weil die Hallenlampen gebacken sind und sich nicht dimmen lassen. |
| D45 | Infektions-Bild | Grün und Unschärfe über zwei globale Volumes (Farbfilter bzw. Gauß-Tiefenunschärfe), der Schleier als HUD-Bild. Schnell, bleischwer, invertiert und schwankend wirken auf den Spieler-Controller. |
| D46 | Zusammenbruch | Die Frist-Prüfung ist fertig (`InfectionLogic.TryCollapse`), wird aber erst vom Tagesablauf in Phase 6 aufgerufen. |

## Phase 6 (2026-10-18)

| Nr. | Thema | Entscheidung |
|---|---|---|
| D47 | Neue Werte | `ui.subtitleMinSec` 3,4 und `ui.subtitlePerChar` 0,08 (Anzeigedauer der Sprüche), `ui.boardRefreshSec` 10 (träge Plantafel), `ui.dayEndDelaySec` 1,4 und `ui.contractAnnounceDelaySec` 2,5; `anger.shoutJitterSec` 5, `anger.stopShoutChance` 0,4, `anger.startShoutDelaySec` 16 (alle Werte aus dem Prototyp-Code: `shoutT: 16`, `+ Math.random() * 5`, `Math.random() < 0.4` beim Not-Stopp; Untertitel `max(3.4, Länge × 0.08)`). |
| D48 | Spieluhr | Neuer Zustand `GameClock.Held`: Während Start-, Bestell- und Endbildschirm steht die Spielzeit, ohne die Pause (P) zu belegen. |
| D49 | Fernseher | Der Direktor-Spruch erscheint auf dem Parolenfeld neben dem Direktor-Bild statt über eine Render-Textur mit animiertem Gesicht. Billiger; ein animiertes Gesicht kann im Art-Pass (Phase 11) folgen. |
| D50 | Plantafel | Die Plantafel zeigt Ist/Soll als echte 3D-Balken und Zahlen unter den gemalten Diagrammen. |
| D51 | Endbildschirm | Feste gemeinsame Zeilenhöhe für Beschriftung und Werte (`lineHeightFactor` 1,45 × Schriftgröße), weil Oswald und die Mono-Schrift unterschiedliche Zeilenabstände haben. |

## Phase 7 (2026-10-18)

| Nr. | Thema | Entscheidung |
|---|---|---|
| D52 | Neue Werte | Abschnitt `inspector` (Weg x 16 → −16 bei z 3,0, Wodka-Reichweite 2,4 und Vorrang 0,6, Oberkommissar ×1,25, Schwanken, Wackeln 0,2 beim Eintreffen – alles Prototyp `inspG`/`pickTarget`; Schrittfrequenz und Beinschwung eigene Werte), Abschnitt `clothFx` (Prototyp `clothK`: Fallgeschwindigkeit, Wackeln, Zittern, Ω-Leuchten), `inspection.susp.ultLevel` 6 (Prototyp `c.ult ? 6`) und `authority.floatingFrom` 1 (Prototyp `floatH > 1`). Für „Stabilität < 61“ wird `risks.activeBelowStab` verwendet. |
| D53 | Zeitbasis | Kontrollen laufen auf `GameState.playTime` (steht bei Pause und Overlays), wie `G.t` im Prototyp. Eine laufende Kontrolle bleibt über das Tagesende hinaus bestehen und geht am nächsten Tag weiter (wie im Prototyp). |
| D54 | Kopfdrehung | Der Kommissar dreht den Kopf zur Kammer, solange sie in seiner Reichweite ist (eigene Ergänzung, nur optisch). Beim Wodka hebt er die Flasche. |
| D55 | Regal-Plane | Abgedecktes Regal wird als Leinenplane über dem Regal gezeigt (der Prototyp zeigt nur den Zustand im Text). |
| D56 | Laken-Look | Neues Laken-Mesh mit Falten (wie die Sinus-Falten im Prototyp) und eigener Leinen-Textur `Canvas.png` (selbst erzeugt, Längsstreifen und roter Saum wie Prototyp `clothTex`). |
| D57 | Ids | Sonderregeln für Ω, Koloss und Singende Kuh prüfen die Ids aus der JSON über `UltimateIds` (eine Stelle statt verstreuter Zeichenketten). |

## Phase 8 (2026-10-18)

| Nr. | Thema | Entscheidung |
|---|---|---|
| D58 | Neue Werte | Abschnitt `machineFx` (Prototyp: Rotor 40 rad/s bei voller Drehzahl, Gehäusewackeln 0,05, Kamerawackeln 0,25 beim Zerspringen, Fenster 0,15 / 0,8 + 0,3·sin) und `iceBlockShowSec` 6 (eigener Wert). Abschnitt `cinematic` (Prototyp: 4,5 s, Licht 0,1, Direktor nach 2,5 s, Abdunkeln mit dt·4) und eigene Werte `animalGlowBoost` 6 und `specialNewsSec` 8. |
| D59 | Sondersendung | Bis Phase 10 als Vollbild-Einblendung im Staats-TV-Stil (Kennung SONDERSENDUNG, Schlagzeile `newsFacts.specialHead`, Laufband `newsFacts.specialTicker`). Neuer Abschnitt `DayMode.News`: Spielzeit steht, danach geht die Schicht weiter. |
| D60 | Neuer Text | `director.speechless` („… … …“), im Prototyp fest im Code. |
| D61 | Abdunkeln | Eigenes Volume `Lab_Cinematic` (Belichtung log2(0,1)) wie beim Stromausfall, weil die Hallenlampen gebacken sind. Damit das Tier im Dunkeln strahlt, werden Leuchten und Glühlicht des Tiers während der Sequenz verstärkt (`AnimalVisuals.GlowBoost`). |
| D62 | Zeitfenster | Zentrifuge, Kryokammer und Zwischensequenz laufen nur während der Schicht (`DayMode.Play`) und in Spielzeit, wie `updateMachines`/`G.cine` im Prototyp. |
| D63 | Eisblock | Der Eisblock wird als vereistes Kryo-Fenster gezeigt (Prototyp: nur Meldung). |
