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
| D14 | Vorlagen entfernt | Mobile-Quality-Stufe samt `Mobile_RPAsset`/`Mobile_Renderer`, `SampleScene`, `SampleSceneProfile`, `TutorialInfo`, `Readme` gelöscht. `InputSystem_Actions.inputactions` bleibt bis Phase 3 liegen. |
