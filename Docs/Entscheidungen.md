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

## Phase 9 (2026-10-18)

| Nr. | Thema | Entscheidung |
|---|---|---|
| D64 | Neue Werte | `machinesFixed.tasting.hiFrom` 75 / `midFrom` 50 und `rohrpost.capsuleSec` 1,2 (Prototyp), `buildMachines.sellRefund` 0,6, `autoSprayRetrySec` 5, `autoSprayWarnGapSec` 40, `pipeGap` 0,25 (Prototyp), Abschnitt `buildView` (Kamera Prototyp `camTop`; Sichtfeld 42 und Ausblendgrenzen eigene Werte), in `machineFx` Werte für Grammophon, Noten, Milchtropfen, Stempel und Mikroskop. |
| D65 | Doppelte Werte | Hitzeabbau durch Musik kommt aus `risks.heatMusicDecay` (gleicher Wert wie `gramophone.heatExtraDecay`), Kaviar-Wirkung aus `anger.caviar` (gleich `rohrpost.angerReduce`). Die zweiten Felder bleiben in der JSON, werden aber nicht gelesen. |
| D66 | Raster-Treffer | Feld unter der Maus über die Feldmitte gerundet. Der Prototyp rechnet in z mit `(z − 4) / 2` und trifft dadurch teils das Nachbarfeld. |
| D67 | Rohre | Rohre zur Kammer als Bodenleitungen (`pipeY` 0,12 statt 1,1 m), weil sie in Hüfthöhe wie Absperrstangen quer durch die Halle liefen. Sie enden 0,25 m vor dem Kammerrand auf der Seite der Maschine (der Prototyp zielt auf die gegenüberliegende Seite). |
| D68 | Ausblenden | Im Baumodus werden Rückwand, linke Wand und Decke über ihre Lage (`buildView.hide*`) beim Bauen des Labors gesammelt und zur Laufzeit ausgeblendet; Stationen bleiben sichtbar. |
| D69 | Esc im Baumodus | Esc gehört zur Pause; im Baumodus meldet das Pause-Menü „Panel schließen“, darauf beendet sich der Baumodus. Eigene Tasten liegen in der neuen Eingabegruppe „Build“. |
| D70 | Text | `ui.build.remove` ohne „✕“, weil die Schrift das Zeichen nicht enthält; neuer Text `ui.build.refund` („{n} % zurück“). |
| D71 | Maschinen-Look | Bedienseite (Kreuz, Bildschirm, Lüfter, Manometer) zeigt zur Kammer, eigener sauberer Lack statt der rostigen Look-Dev-Emaille. |

## Phase 10 (2026-10-18)

| Nr. | Thema | Entscheidung |
|---|---|---|
| D72 | Neue Werte | In `news`: `anchorGoneFrom` 0,5 (Prototyp `G.newsDead`), `skipAfterSec` 1,5, `factsMax` 4, `prognosisYearsPerPlan` 3, Balkenmaße wie Prototyp-Diagramm, `tickerSpeed` 140 (eigener Wert). Abschnitt `records` (epsilon 0,05, Direktor 50 %) und `newsStudio` (Kamerafahrt, Sichtfeld 38, Zusammensacken nach 1,8 s). `cinematic.specialNewsSec` entfällt; die Sondersendung dauert wie jede Sendung `news.durationSec`. |
| D73 | Studio als Szene | Das Studio ist eine eigene Szene `NewsStudio`, die das Labor beim Start additiv lädt, und liegt 200 m unter dem Labor, damit sich Lichter nicht beeinflussen. Während der Sendung rendert die Studiokamera, die Spielerkamera ist aus. |
| D74 | Spiegelung | Die Studio-Koordinaten des Prototyps (three.js, rechtshändig) werden in x gespiegelt, sonst stünden Schild, Diagramm und Pflanze auf der falschen Seite. |
| D75 | Taten im Laufband | `GameState.newsFacts` enthält Textpfade unter `newsFacts.*` (Liste → Zufallseintrag) oder fertige Texte (Sonderauftrag mit Platzhaltern). Neueste zuerst, höchstens 4, danach die Tageszeile. |
| D76 | Neue Texte | `ui.news.chartTitleAlive`, `chartMeat`, `chartQuality`, `planBroadcast`, `signName` (im Prototyp fest im Bild) und `ui.logbook.funLine`. |
| D77 | TV-Knopf | „sonst über Knopf“ (GDD 11) als Knopf im Pausemenü, sichtbar während der Schicht nach der ersten Mutation; daneben ein Logbuch-Knopf. |
| D78 | Rekord-Meldung | Die Meldung nutzt `ui.records.*` (wie Prototyp), die Rekordtafel `ui.logbook.record*`. |
| D79 | Logbuch-Skizzen | Eigene Strichzeichnungen (Kuh, Huhn, Euter, gestrichelter Rahmen) als Sprites; Größe nach `animalVisuals.growPerA`/`growCapA` wie am echten Tier. |
| D80 | Fenster | Einöde als eigenes Bild, Volk und Schilder als Ebenen davor (Texte aus der JSON), statt alles in eine Textur zu malen. |

## Phase 11 (2026-10-18)

| Nr. | Thema | Entscheidung |
|---|---|---|
| D81 | Quellen | Nur eigene Modelle (Rückfrage beantwortet): alles per Editor-Skript aus Abstandsfeldern und Drehkörpern, Texturen selbst erzeugt bzw. Poly Haven (CC0, schon in Credits). |
| D82 | Tierskelett | Neu mit Skelett (Rückfrage beantwortet). Eine Haut statt Einzelteile; Knochengewichte aus dem Abstand zu den Körperteilen (weicher Übergang 3,5 cm, höchstens 4 Knochen). Augen, Hörner, Hufe, Ohren, Kamm, Schnabel, Euter und Flügel sind starre Teile an Knochen. |
| D83 | Mutation | Aufblähen als Blendshape: Rumpf längs nur 60 % (sonst schiebt er sich über Hals und Kopf), Hals quer zur Achse 80 %; angehängte Teile werden mit derselben Formel verschoben (`AnimalVisuals.Swell`). Euter wächst über die Knochenskalierung. |
| D84 | Neue Werte | In `animalVisuals`: `growSmoothing`, `extraGrowSpeed`, Atmen, Kopf, Ohren, Schwanz, Knie, Zittern (eigene Werte; im Prototyp gab es diese Bewegungen nicht). |
| D85 | Plakattexte | `posters` in der JSON: „|“ und „ · “ brechen die Parole um, „ – “ trennt die kleine Unterzeile. Die letzte Zeile (ACHTUNG …) ist das Warnschild aus dem Prototyp (`warnTex`). Plakatfarben wie im Prototyp. |
| D86 | Neuer Text | `ui.signs.books` (GENETIK, NÄHRSTOFFE, WACHSTUM, NEBENWIRKUNGEN?) aus der Art Direction für die Buchrücken. |
| D87 | Deko-Lage | Prototyp-Lagen an unseren Raum angepasst: Büroecke unter dem Fenster mit Teppich, rechte Fässer an die Rückwand gerückt, Rückwand-Plakate bei x ±5,9 (bei ±8,5 hängen die Banner). |
| D88 | HUD-Stil | Industrieschild als 9-Slice-Sprite für alle großen dunklen HUD- und Panelflächen; Logbuch (Papier), Sendung und Pause bleiben, wie sie sind. |
| D89 | Testlauf | Die EditMode-Tests starten über den Menüpunkt *Plan erfüllt > EditMode-Tests ausführen* (die Verbindung lässt den Test-Runner nicht mehr direkt starten). |

## Phase 12 (2026-10-18)

| Nr. | Thema | Entscheidung |
|---|---|---|
| D90 | Klänge | Alle Klänge prozedural als WAV, Nachbau der WebAudio-Synthese des Prototyps (`Tools/gen_audio.py`): Oszillatoren mit denselben Frequenzen, Rampen und Lautstärken, gefiltertes Rauschen. Keine Fremdquelle, daher keine Rückfrage. Tonhöhe (Muhen nach A, Grummeln nach Wut, Zentrifuge nach Drehzahl, Marschmelodie) zur Laufzeit über die Tonhöhe der Quelle. |
| D91 | Mixer | Kanäle Gesamt × Musik bzw. Effekte im Code (`AudioLogic.Channel`) statt eines AudioMixer-Assets: Unity bietet keine öffentliche Schnittstelle, um Mixergruppen per Skript anzulegen. Grundpegel `audio.master` 0,45 wie Prototyp. Pause hält den Ton an (`AudioListener.pause`). |
| D92 | Klang zur Meldung | Wie im Prototyp (SFX neben `toast()`): Tabelle `audio.toastSounds` (Textpfad → Klang). Dafür meldet `GameManager.Toast` zusätzlich den Textpfad (`EventBus.ToastKey`). Dazu Mischen, Sprühen, Hebel, Direktor-Grummeln, Tierlaute, Sirene, Zentrifuge, Marsch und Münzhuhn direkt. |
| D93 | Neue Werte | Abschnitt `audio` (Prototyp-Werte; `voices` 16 eigener Wert) und `settings` (Mausfaktor 0,3–3; Grafik Niedrig: Renderskala 0,75, Schatten 25 m, 1024er Karte, 2 Kaskaden – eigene Werte). |
| D94 | Grafik Hoch/Niedrig | Zweite URP-Einstellung `PC_RPAsset_Low` (Kopie mit den Werten aus `settings`, ohne weiche Schatten), zur Laufzeit über `QualitySettings.renderPipeline`; das Projekt hatte nur eine Qualitätsstufe. |
| D95 | Hauptmenü | Der Startbildschirm ist das Hauptmenü (Für den Plan!, Einstellungen, Beenden). Einstellungen auch in der Pause, dazu dort „Ton an · M“. Zusätzlicher Regler „Kamerawackeln“ (0 = aus) als Dosierung für empfindliche Spieler. |
| D96 | Neue Texte | `ui.settings.*`, `ui.intro.settings`, `ui.intro.quit`, `ui.hud.btnSettings`. |
| D97 | Performance | Teile unter 0,6 m Diagonale werfen keinen Schatten (GDD 15 „Schatten nur für große Objekte“), statisches Batching für Windows an. Licht nach dem Neubau neu gebacken. |
| D98 | Autopilot | Für den Durchlauf ein Debug-Autopilot (`DebugAutoPlay`, nur Editor/Development), der über die echten Systeme spielt. Er setzt den Leerlauf zurück wie der Testschritt im Prototyp (`G.idleT = 0`), sonst steigt die Wut, weil er die Figur nicht bewegt. |
| D99 | Shader im Build | „Unbenutzte Post-Processing-Varianten entfernen“ ist aus: sonst entfernt der Build Motion-Blur- und Panini-Shader und URP schaltet das ganze Post-Processing ab (Meldung im Player.log). |
| D101 | Sprinten | Auf Wunsch: Shift (links oder rechts) halten = Tempo × 1,6 (`player.sprintMultiplier`, eigener Wert; der Prototyp hat keinen Sprint). Wirkt zusätzlich zu Infektionseffekten. Hinweistexte nennen „Shift sprinten“. |
| D102 | Vollbild | Auf Wunsch: Einstellung „Anzeige“ Vollbild (randlos, Bildschirmauflösung) oder Fenster (`settings.windowedWidth/Height` 1600 × 900, eigene Werte); gespeichert, beim Start angewendet; im Editor ohne Wirkung. Neue Texte `ui.settings.display`, `fullscreen`, `windowed`. |
| D103 | Maus fangen | Gibt das Spiel die Maus selbst frei (Panel, Terminal, Pause, Menü, Sendung), fängt es sie beim Zurückkehren automatisch wieder; ein Klick ist nur nach Alt+Tab nötig (vorher immer). |
| D104 | Speichern | Automatisch zu Beginn jeder Schicht (Bestellterminal), als JSON des Spielstands in `persistentDataPath/spielstand.json`. Mitten in der Schicht wird nicht gespeichert, weil Tier, Maschinenläufe und Kommissar in der Szene laufen. Gespeichert werden nur Daten (Felder), berechnete Eigenschaften nicht. Auch nach einem Spielende bleibt der letzte Stand ladbar. |
| D105 | Hauptmenü | Eigener Vollbild-Bildschirm statt Popup: rotes Band mit Emblem, Titel und Knöpfen (Weiterspielen nur mit Spielstand, Neues Spiel, Einstellungen, Beenden), rechts Direktor-Plakat mit Parole (`ui.signs.wallSlogan`, `directorCaption`). „Neues Spiel“ öffnet die Einsatzbesprechung (bisheriger Starttext) mit „Für den Plan!“ und „Zurück“. Pause: „Hauptmenü“ lädt das Labor neu, „Spiel beenden“. Neue Texte `ui.menu.*`, `ui.intro.continue`, `ui.hud.btnMainMenu`, `btnQuit`, `saveHint`, `saveHintNone`. |
| D100 | Build | Produktname „Plan erfüllt“, randloses Vollbild 1920 × 1080, Ausgabe `Builds/Windows/` (nicht im Git). Firmenname unverändert (DefaultCompany), bitte bei Bedarf selbst setzen. |
| D106 | Balancing neu | Auf Wunsch „mittelschwer“: Schicht 5 Minuten (`day.dayLengthSec` 300), Soll je Plan 300/90/45 bis 900/255/140 t, Überschuss zahlt doppelt (`overfulfilPayBonus` 1,0), Start 900 SM. Teurer sind vor allem Wodka (120), Kaviar (220) und Gegenmittel (150); Grundstoffe nur leicht angehoben (A 45, B 65, C 55). Eigene Werte, im Spiel nachjustierbar. |
| D107 | Bestellen für morgen | Das Bestellterminal ist ein Panel, das jederzeit offen ist; bestellt wird für den nächsten Morgen und sofort bezahlt (mehrere Bestellungen addieren sich). Wer nichts bestellt, bekommt die Standardlieferung (`day.standardOrder`: 2× A, 3× B, 2× C), gekürzt auf die Kasse. Der Bildschirm „Bestellung“ vor der Schicht entfällt. |
| D108 | Laken verbraucht sich | Laken sind ein Verbrauchsartikel (Preis 90, Start 1): Überwerfen verbraucht ein Laken, beim Abnehmen ist es „von den Mutagenen zerfressen“. Ohne Laken im Lager keine Tarnung. |
| D109 | Klonen kostet | Kuh 40 SM, Huhn 20 SM (`prices.cloneCow/cloneHen`); ohne Geld kein Klon. |
| D110 | Einführung | Geführte Einführung an Tag 1 (9 Schritte, `tutorial`), mit Zielkasten oben und Pfeil zur Station; Lese-Schritte gehen nach `readSec` weiter, überspringbar in der Pause, abschaltbar mit „Ohne Einführung“. |
| D111 | Halle und Nebenzimmer | Halle 4 m tiefer (`room.backWallZ` 15,9). Hinter der Rückwand ein Anbau (`layout.wing`, 7 m tief, 4,2 m hoch) mit fünf Zimmern (`layout.rooms`): Lager (Bestellterminal und Lagerausgabe), Umkleide, Schlafraum, Kantine, Direktorbüro, je mit offener Doppeltür und Schild. Nur das Lager hat Stationen, die anderen Zimmer sind Kulisse. Begehbar sind Halle, Türen und Zimmer (`WalkArea`, Vereinigung von Rechtecken), weil die Wände keine Kollider haben. Spinde, Kisten und Fässer sind aus der Halle in die Zimmer umgezogen, Plakate 3 und 4 an die Seitenwände. |
| D112 | Maschinen verteilt | Leicht auseinandergezogen: Mischstation x −5,7, Mikroskop −7,0, Kryo (8,2 / 2,4), Verkostung (11,4 / 3,7); Grammophon und Rohrpost an die neue Rückwand (z 15,0 / 15,25). Kammer, Band, Klonanlage, Verarbeitung und Baufeld unverändert. |
| D113 | Direktor neu | Erfundener dicker Funktionär (Glatze mit Haarkranz, runde Brille, Doppelkinn, buschige Brauen, Fliege, viele Orden), bewusst ohne Schnurrbart und Mütze, keiner echten Person nachempfunden. Im Fernseher aus Ebenen (`Tools/gen_director.py`, `DirectorRig`): Mund bewegt sich beim Sprechen (wütend: brüllt), Blinzeln, Brauen senken sich und das Gesicht läuft mit der Wut rot an, ab Wut-Stufe 1 Schweiß, ab Stufe 2 Zornesader und Kopfzittern. Dasselbe Bild auf Plakat, Porträt und im Hauptmenü. |
| D114 | Sendung mit Stimmung | Sprecher-Stimmung aus dem letzten Tagesergebnis (`NewsLogic.Mood`): Plan erfüllt → Jubel (Winken, lachende Augen, Wangen, Konfetti), in der Krise nur gefasst; verfehlt → Schwitzen (Tropfen, Stirn wischen, Blick wandert, Handkamera wackelt), ab zwei verfehlten Tagen oder in der Krise → Weinen (Taschentuch, Tränen, Schluchzen). Kamerafahrt wechselt zufällig (Zufahrt, Schwenk, Kran). Zusammensacken im Automatikbetrieb wie bisher. |
| D115 | UI-Stil | Knöpfe mit Fase und Hover-/Druck-Abstufung, Infofelder als eingelassene dunkle Felder, Schieberegler mit Messingknopf, große Panels mit rotem Zierstreifen und Goldkante (`HudStyle`, eigene Texturen). Einführungskasten unter die Lager-Chips verschoben. |
| D116 | Wachstumshormon stärker | Auf Wunsch lohnt sich A-17 mehr: je Dosis Fleisch +120 % (statt +70 %), Ertrag +35 % (statt +15 %), Stabilität −11 (statt −14), Qualität −6 (statt −8). Die Referenztests aus Prompt 4/GDD prüfen die Formeln weiter mit den Prototypwerten (`TestData.PrototypeConfig`). |
| D117 | Springen | Leertaste springt (Höhe `player.jumpHeight` 0,9 m, `gravity` 22, eigene Werte), aber nur wenn keine Station im Ziel ist – dann bleibt die Leertaste „Benutzen“. Die Leertaste, die ein Panel schließt, löst keinen Sprung aus. Möbel, Band und Maschinen tragen über ihre Kollider. |
| D118 | Hinter dem Band | Der Spielerbereich reicht bis vor die Wand hinter der Kammer (`playerArea.zMin` −2,2). Band und Laufsteg-Stützen haben Kollider; über das Band springt man (0,45 m hoch). Die Kammer bleibt über ihren Mindestabstand gesperrt. |
| D119 | Klonkosten im Pult | Die Klon-Hebel zeigen die Kosten („KLON · Kuh · 40 SM“, Texte `ui.pult.leverCloneCow/Hen` mit `{cost}`) und sind gesperrt, solange die Kasse nicht reicht. |
| D120 | Taste F | Benutzen (halten) liegt auf F, die Leertaste springt immer (D117 damit vereinfacht). Panels und die Sendung schließen mit F oder Leertaste. Texte mit „Leertaste halten“ auf „F halten“ umgestellt. |
| D121 | Knopf-Rückmeldung | Alle Knöpfe (Hauptmenü, Panels, Pause, Einstellungen) leuchten beim Überfahren warm auf und wachsen leicht, beim Drücken werden sie dunkler und kleiner (`ButtonHoverFx`, eigene Leuchtebene, unabhängig von den Farben im Code). |
| D122 | HUD-Feinschliff | Planfenster oben links auf 80 %. Roter Rand am Schichtende sanfter (Deckkraft 0,22 statt 0,55, 0,8 statt 2,2 Pulse/s); bei cholerischem Direktor unverändert. |
| D123 | Einführung deutlicher | Größerer Kasten mit grünem Rahmen und weißer Schrift, liegt über den Panels, blinkt bei jedem neuen Schritt golden auf; vor Bestellung und Logbuch tritt er zurück. Dazu leuchtende Pfeile am Boden vom Spieler zum Ziel (`TutorialPath`, Weg durch die Tür bei Nebenzimmern, `GuidePath`). Der Randpfeil hält den oberen Bereich frei. |
| D124 | Logbuch | Breiter (1720 × 960), Karten wachsen mit ihrem Text, Texte nutzen die volle Kartenbreite – nichts überlappt mehr. |
| D125 | Bildformate | UI skaliert je Bildformat: schmaler als 16:9 (Laptop 16:10, 4:3) nach der Breite, breiter (21:9) nach der Höhe (`CanvasAspectFit`). |
| D126 | Kantine | Auf Wunsch: drei Gerichte (200 / 350 / 500 SM) verstärken bis Schichtende die Serenwirkung um ×1,1 / ×1,3 / ×1,5 – der Zugewinn gegenüber dem unbehandelten Tier wird multipliziert (wie die Konzentration der Zentrifuge, beides zusammen multipliziert sich). Ein besseres Gericht ersetzt ein schlechteres. `sideRooms.canteen`. |
| D127 | Umkleide | Schutzkittel für eine Schicht (kostenlos, F halten): hält 60 % der Ansteckungen bei Explosionen ab (`sideRooms.locker.protectChance`). |
| D128 | Schlafraum | Mit erfülltem Plan kann man sich hinlegen und die Schicht sofort beenden (kein Warten bis Schichtende). |
| D129 | Direktorbüro | Kaviar persönlich am Schreibtisch: Wut −40 statt −25 (Rohrpost), aber 30 % Chance, dass er gestört ist und brüllt (Wut +12). `sideRooms.desk`. |
| D130 | Platzierung | Zentrifuge (12,2 / 1,6) und Kryokammer (15,1 / 2,6, Schild höher) in die frei gewordene rechte Ecke, Verkostung an die alte Zentrifugenstelle (6,0 / 2,8). Hintere Lampenreihe mit fünf Lampen. Stationsschilder werden aus der Nähe kleiner (ab 7 m bis 40 %). Das Mikroskop bleibt auf der Mischstation, es hat kein eigenes Ziel (Probe über das Mischpanel). |
| D131 | Bedienung | Prompt zeigt die Taste („[F] Pult benutzen“). Bestellterminal: „Wie gestern“ übernimmt die letzte eigene Lieferung (gekürzt auf die Kasse). Erledigter Einführungsschritt klingelt. Startoption `-screen-width/-screen-height` geht beim Start vor die gespeicherte Anzeige (für Tests); 4:3 im Build geprüft. |
| D132 | Kommissar und Prämie | Nach den Balancing-Läufen und auf Wunsch: Kontrollen seltener (`inspection.gapSec` 100 → 150, `minGapSec` 45 → 70), Verschwiegenheitsprämie 400 → 150 SM. Vorher war jede Kontrolle eine Einnahmequelle (häufigerer Kommissar = mehr Geld) und Geld im Überfluss. |
| D133 | Hinweise bei erstem Auftreten | Nach Spieltest („was tun, wenn Kontrolle kommt“, „Band geht kaputt“): Mit Einführung erklärt ein grüner HINWEIS-Kasten jede Lage beim ersten Mal – Kontrolle (Laken), instabiles Tier (Not-Stopp), Bandriss (Bandmotor), verseuchte Kammer, Infektion (G), keine Laken, wütender Direktor, dazu Zentrifuge, Kryo, goldene Seren, Nebenzimmer. Pfeil zum Ziel wie in der Einführung. Jeder Hinweis einmal pro Spiel; dringende verdrängen Info-Hinweise. Liste und Zeiten in `tutorial.hints`, `hintInfoSec` 12, `hintMaxSec` 30. „Einführung überspringen“ schaltet sie ab. |
| D134 | Auftragskarte zu Schichtbeginn | Jede Schicht beginnt mit einer Karte: Tagessoll groß (Milch, Eier, Fleisch), Schichtlänge, Ablauf in vier Schritten, Hinweise des Tages (Kommissar ab Tag 2 bzw. Laken im Lager, neue Maschinen, Wiederholung, Bestellen). Die Zeit steht, bis man „Schicht beginnen“ drückt (F, Leertaste, Esc oder Knopf). Ohne Wertänderung. |
| D135 | Einsatzbesprechung in Seiten | Der Textblock zu Spielbeginn ist in sechs Seiten geteilt (Auftrag, Ware, Gefahren, Kommissar, Geld/Direktor, Steuerung) mit „Weiter →“ und „Zurück“; größere Schrift. Texte unter `ui.intro.pages`. |
