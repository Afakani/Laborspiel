# Plan erfüllt – Unity-Architektur (Vorgabe)

Das ist ein Vorschlag. Abweichungen sind erlaubt, wenn sie begründet und im `Docs/Entscheidungen.md` festgehalten werden.

## Technik
- Unity 6 LTS (Fallback 2022.3 LTS), Universal Render Pipeline
- Pakete: Input System, TextMeshPro, ProBuilder, Cinemachine (optional), Post Processing über URP Volume
- C#, Namespace `PlanErfuellt`, ein Script pro Klasse, englische Bezeichner, deutsche UI-Texte nur aus JSON

## Ordnerstruktur
```
Assets/_Project/
  Art/        Materials, Models, Textures, Shaders, VFX, UI
  Audio/      SFX, Music, Mixer
  Data/       ScriptableObjects (Config, Serums, Recipes, Texts)
  Prefabs/    Stations, Animals, UI, Props
  Scenes/     Boot, Lab, NewsStudio (additiv), TestRange
  Scripts/
    Core/        GameManager, GameState, EventBus, Services, SaveData (optional)
    Config/      GameConfig, BalancingImporter, TextDatabase
    Player/      FirstPersonController, PlayerLook, InteractionSystem, Interactable
    Lab/         Chamber, Animal, AnimalStats, AnimalVisuals, SerumRack, Serum, Formula
    Stations/    MixingStation, ControlPanel, Centrifuge, CryoChamber, Gramophone, TastingMachine, Rohrpost, Cloth, BeltMotor, OrderTerminal, Storage, Microscope
    Build/       BuildMode, BuildGrid, BuildMachine (Pump, AutoSprayer, Cooler, EmergencyDoc)
    Day/         DayCycle, Quota, Wishes, Contracts, Tutorial, Records
    Characters/  Director (TV), DirectorScreen, Inspector, InspectionSystem
    World/       WorldStats, WindowView, Blackout, Infection, Explosion
    News/        NewsDirector, NewsStudio, NewsFacts
    UI/          HUD, Toasts, PromptUI, CentrifugeBar, PanelControl, PanelMixing, OrderUI, LogbookUI, EndScreen, PauseMenu, Subtitle
    Audio/       AudioService, SfxLibrary
    Debug/       DebugMenu
Docs/  (Kopie dieser Übergabedateien + Entscheidungen.md + Fortschritt.md + Credits.md)
```

## Kernprinzipien
1. **Datengetrieben:** `02_Balancing.json` wird per Editor-Importer in ScriptableObjects übertragen (`GameConfig.asset`, `UltimateSerum_*.asset`, `FunRecipe_*.asset`, `BuildMachineType_*.asset`). `03_Texte_de.json` wird als TextAsset geladen (`TextDatabase`). Kein Spielwert und kein UI-Text hart im Code.
2. **Ein zentraler Zustand:** `GameState` (Geld, Lager, Seren, Tag, Plan, Wut, Parteikontrolle, Infektion, Rekorde, Entdeckungen …) als reine C#-Klasse, gehalten vom `GameManager`. Systeme lesen und schreiben darüber, UI hört auf Events.
3. **Events statt Querverweisen:** `EventBus` (statische C#-Events oder ScriptableObject-Events), z. B. `OnSerumMixed`, `OnAnimalStateChanged`, `OnExplosion`, `OnQuotaMet`, `OnDayEnded`, `OnInspectionStarted`, `OnToast`, `OnDirectorSpeak`.
4. **Reine Formeln testbar:** `Formula.cs` als statische Klasse (Stabilität, Qualität, Rate, Fleisch, ComboInfo, ComboName, ProbeHints). Dazu EditMode-Unit-Tests mit Werten aus dem Prototyp (siehe unten).
5. **Interaktion einheitlich:** `IInteractable` mit `GetPrompt(out text, out canUse, out holdTime)` und `Execute()`. Das `InteractionSystem` wählt das nächste Ziel (Distanz/Blick), zeigt Prompt und Haltebalken.
6. **Zeit:** alles über einen Spielzeit-Takt (`GameClock`, pausierbar, Zeitraffer im Debug-Menü).
7. **Performance:** siehe GDD 15. Static Batching und GPU Instancing, Licht backen, Lichtanzahl begrenzen, keine `Find`-Aufrufe im Update, UI nur bei Änderungen aktualisieren.

## Testwerte für Formula-Tests (aus dem Prototyp)
| Dosis A-B-C | Stabilität | Qualität | Rate Kuh (t/s) | Fleisch Kuh (t) |
|---|---|---|---|---|
| 0-0-0 | 100 | 80 | 2.00 | 6.00 |
| 0-1-0 | 82 | 68 | 3.60 | 6.60 |
| 1-1-0 | 62 | 60 | 3.90 | 10.80 |
| 2-2-0 | 12 | 40 | 5.80 | 15.60 |
| 1-1-1 | 82 | 68 | 3.65 | 10.17 |
| 2-3-5 (roh, ohne Ultimativ) | 32 | 68 | 5.09 | 11.83 |
| 0-4-2 | 58 | 48 | 6.92 | 7.85 |
| 0-4-2 Huhn | 58 | 48 | 2.77 | 2.62 |
(Rate und Fleisch auf 2 Nachkommastellen gerundet; Formeln siehe GDD 4.3.)

## Szenenaufbau Lab (Hierarchie, grob)
```
Lab
  --- Environment (static): Floor, Walls, Ceiling, Window, Decor, Lights(baked)
  --- Chamber: Glass, Frame, Nozzles(3), GasVFX, ChamberLight, Belt, Cloth
  --- Stations: Panel, MixingStation(+Microscope, Rack), Centrifuge, Cryo, Tasting, Gramophone, Rohrpost, BeltMotor, CloneUnit, Processing, OrderTerminal, Storage, TV+RageScale, PlanBoard
  --- BuildGrid
  --- Characters: Player(FPS), Inspector(pooled)
  --- Systems: GameManager, DayCycle, InspectionSystem, WorldStats, AudioService, NewsDirector
  --- UI (Canvas Screen Space Overlay)
```
