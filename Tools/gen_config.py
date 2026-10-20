# Erzeugt die serialisierbaren Config-Datenklassen (eine Datei pro Klasse).
import os, sys

OUT = sys.argv[1]

# (Klasse, Kommentar, [(Typ, Feld, Kommentar, Default, JsonName)])
F = lambda t, n, c="", d=None, j=None: (t, n, c, d, j)

CLASSES = [
    ("GoodsValues", "Werte je Ware: m = Milch, e = Eier, f = Fleisch.", [
        F("float", "m", "Milch"), F("float", "e", "Eier"), F("float", "f", "Fleisch")]),
    ("SubstanceAmounts", "Menge je Grundstoff A/B/C.", [
        F("int", "A", "Wachstum A-17"), F("int", "B", "Laktation B-08"), F("int", "C", "Stabilität C-04")]),
    ("LinearValue", "Wert, der mit dem Plan-Index wächst: base + perPlan * planIdx.", [
        F("float", "base", "Grundwert"), F("float", "perPlan", "Zuwachs pro Plan-Index")]),
    ("DayConfig", "Tagesablauf, Quoten und Bezahlung (GDD 7.1).", [
        F("int", "plans", "Anzahl Pläne bis Spielende"),
        F("float", "dayLengthSec", "Schichtlänge in Sekunden"),
        F("GoodsValues[]", "targets", "Tagesziel je Plan"),
        F("float[]", "prizes", "Prämie je Plan"),
        F("GoodsValues", "payPerTon", "SM pro Tonne"),
        F("float", "overfulfilPayBonus", "Aufschlag über Soll (0.5 = +50 %)"),
        F("float", "lateWarningSec", "Restzeit, ab der der Bildschirmrand pulsiert"),
        F("int[]", "standardOrder", "Lieferung ohne Bestellung (A, B, C, Gegenmittel, Wodka, Kaviar, Laken)")]),
    ("StartConfig", "Startwerte eines neuen Spiels (GDD 4.1).", [
        F("float", "money", "Start-Kasse"),
        F("SubstanceAmounts", "inventory", "Startlager A/B/C"),
        F("int", "antidote", "Gegenmittel"), F("int", "vodka", "Wodka"), F("int", "caviar", "Kaviar"),
        F("int", "cloths", "Laken")]),
    ("PricesConfig", "Einkaufspreise am Bestellterminal und Kosten fürs Klonen.", [
        F("float", "A"), F("float", "B"), F("float", "C"),
        F("float", "antidote"), F("float", "vodka"), F("float", "caviar"),
        F("float", "cloth", "Laken (wird bei jedem Einsatz verbraucht)"),
        F("float", "cloneCow", "Klonen einer Kuh"), F("float", "cloneHen", "Klonen eines Huhns")]),
    ("TutorialStepConfig", "Schritt der Einführung: Bedingung (id) und Ziel, auf das der Pfeil zeigt.", [
        F("string", "id", "walkMixing, mix, clone, spray, watch, process, order, cloth, finish"),
        F("string", "target", "Station-Id (mixing, pult, terminal …), chamber oder leer")]),
    ("TutorialConfig", "Geführte Einführung an Tag 1.", [
        F("float", "readSec", "Lese-Schritte gehen nach dieser Zeit weiter"),
        F("TutorialStepConfig[]", "steps", "Reihenfolge der Schritte (Texte in ui.tutorial.steps)")]),
    ("SubstanceConfig", "Ein Grundstoff (A/B/C) mit Anzeige-Daten.", [
        F("string", "id", "A, B oder C"), F("string", "code", "z. B. A-17"),
        F("string", "displayName", "Anzeigename", None, "name"), F("string", "color", "Farbe als Hex")]),
    ("MixingConfig", "Mischstation (GDD 4.2).", [
        F("int", "sliderMax", "Maximale Dosis pro Regler (auch: voller Tank in der Anzeige)"),
        F("float", "mixTimeSec", "Mischdauer"), F("int", "rackSize", "Plätze im Serum-Regal"),
        F("float", "stirSpeedDeg", "Rührer-Drehung pro s beim Mischen"), F("string", "centrifugedColor", "Farbe zentrifugierter Rohseren")]),
    ("AnimalBaseConfig", "Grundwerte einer Tierart.", [
        F("float", "baseRate", "Grundertrag Milch/Eier in t/s"),
        F("float", "baseMeat", "Grundfleisch in t bei Verarbeitung"),
        F("string", "produces", "Ware: m oder e")]),
    ("AnimalsConfig", "Kuh und Huhn.", [
        F("AnimalBaseConfig", "cow"), F("AnimalBaseConfig", "hen")]),
    ("StabilityCoeffs", "Koeffizienten der Stabilitätsformel (GDD 4.3).", [
        F("float", "base"), F("float", "perA"), F("float", "perB"), F("float", "perAB"), F("float", "perC"),
        F("int", "overloadFrom", "Ab dieser Gesamtdosis gibt es Überlast-Abzug"),
        F("float", "overloadPer", "Abzug pro Dosis über overloadFrom")]),
    ("QualityCoeffs", "Koeffizienten der Qualitätsformel (GDD 4.3).", [
        F("float", "base"), F("float", "perA"), F("float", "perB"), F("float", "perC")]),
    ("YieldCoeffs", "Koeffizienten für Ertrag bzw. Fleisch (pro gedämpfter Dosis).", [
        F("float", "perA"), F("float", "perB")]),
    ("AcidConfig", "Faktoren bei Säure-Milch.", [
        F("float", "quality"), F("float", "rate"), F("float", "meat")]),
    ("FormulaConfig", "Alle Zahlen der Serum-Formeln (GDD 4.3).", [
        F("float", "dampPerC", "Dämpfung e = 1/(1+dampPerC*C)"),
        F("StabilityCoeffs", "stability"), F("QualityCoeffs", "quality"),
        F("YieldCoeffs", "rate", "Milch/Eier"), F("YieldCoeffs", "meat", "Fleisch"),
        F("float", "statMax", "Obergrenze für Stabilität und Qualität"),
        F("float", "frozenStabBonus", "+Stabilität pro tiefgekühltem Serum"),
        F("float", "musicStabBonus", "+Stabilität während Musik"),
        F("float", "stampQualityBonus", "+Qualität durch Siegel"),
        F("AcidConfig", "acid")]),
    ("ComboConfig", "Schwellen für Kombinationsnamen und Vor-/Nachteile (GDD 4.4).", [
        F("int", "extremeFromAB", "Zusatz (extrem) ab A+B"),
        F("int", "stabilizedMinC", "Zusatz (stabilisiert) braucht mindestens so viel C"),
        F("float", "stabilizedRatio", "... und C >= ratio*(A+B)"),
        F("float", "showEffectAboveFactor", "Vorteil wird ab diesem Faktor angezeigt"),
        F("int", "heatWarnMinA"), F("int", "acidWarnMinB"), F("int", "floatWarnMinB"), F("int", "beltWarnMinA")]),
    ("RisksConfig", "Risiken am Tier (GDD 5.2).", [
        F("float", "activeBelowStab", "Risiken nur unter dieser Basis-Stabilität"),
        F("int", "heatMinA"), F("float", "heatPerA"), F("float", "heatDecay"), F("float", "heatCrisis"),
        F("float", "heatUnstable"), F("float", "heatStabPenalty"),
        F("int", "acidMinB"), F("float", "acidThreshold"),
        F("int", "floatMinB"), F("float", "floatPerB"), F("int", "floatFromB"), F("float", "floatDecay"),
        F("float", "floatCrisis"), F("float", "floatEscape"), F("float", "escapeCost"), F("float", "leashTo"),
        F("int", "beltBreakMinA"), F("float", "beltBreakChancePerAPerSec"),
        F("float", "cableTearBelowStab"), F("float", "cableTearChancePerSec"),
        F("float", "blackoutSec"), F("float", "blackoutProductionFactor"),
        F("float", "blackoutLightFactor", "Helligkeit während des Stromausfalls (0.2 = 20 %)"),
        F("float", "heatMusicDecay", "Zusätzliches Abkühlen pro s während Musik"),
        F("float", "unstableCountdownSec"),
        F("float", "infectionDistance"), F("float", "infectionChance"), F("int", "infectionFromDay"), F("int", "infectionDays")]),
    ("WarningsConfig", "Warnzeichen vor Krisen (GDD 5.3).", [
        F("float[]", "heat", "Hitze-Bereich [von, bis]"), F("float", "acidT", "Säure-Warnung ab"),
        F("float[]", "float", "Schwebe-Bereich [von, bis]", None, None), F("float", "stab", "Zittern unter")]),
    ("ProcessingConfig", "Verarbeitung und Mut-Bonus (GDD 5.6).", [
        F("float", "braveMinStab"), F("float", "braveBonus"), F("float", "braveExplodeChance"), F("float", "kolossBeltBreakChance")]),
    ("AngerConfig", "Direktor-Wut (GDD 7.2).", [
        F("float", "max"), F("float", "idleAfterSec"), F("float", "idlePerSec"), F("float", "explode"), F("float", "collapse"),
        F("float", "planFailed"), F("float", "planMet"), F("float", "inspectionPassed"), F("float", "caviar"),
        F("float", "wishDone"), F("float", "wishFailed"),
        F("float[]", "tiers", "Grenzen der Wut-Stufen"),
        F("float[]", "shoutIntervalSec", "Abstand zufälliger Rufe [min, max]"),
        F("float", "shoutJitterSec", "Zufälliger Zuschlag auf den Rufabstand"),
        F("float", "stopShoutChance", "Wahrscheinlichkeit eines Spruchs beim NOT-STOPP"),
        F("float", "startShoutDelaySec", "Erster zufälliger Ruf nach Spielstart")]),
    ("AuthorityConfig", "Parteikontrolle (GDD 9).", [
        F("float", "max"), F("float", "base"), F("float", "lowStab"), F("float", "floating"), F("float", "glowFactor"),
        F("float", "omega"), F("float", "koloss"), F("float", "clothFactor"), F("float", "musicFactor"),
        F("float", "singingCow"), F("float", "contaminated"), F("float", "decay"), F("float", "explodeAdd"), F("float", "afterInspection"),
        F("float", "floatingFrom", "Schwebehöhe, ab der „schwebend“ zählt")]),
    ("SuspicionConfig", "Verdacht pro Sekunde während der Kontrolle (GDD 9).", [
        F("float", "uncovered"), F("float", "uncoveredKoloss"), F("float", "coveredBase"), F("float", "coveredPerDose"),
        F("float", "coveredMaxDose"), F("float", "coveredUnstable"), F("float", "coveredOmega"), F("float", "coveredKoloss"),
        F("float", "contaminated"), F("float", "rack"), F("float", "rackRange"), F("int", "rackSuspiciousAB"),
        F("float", "ultLevel", "Mutationsgrad eines Ultimativs (Laken-Verdacht und Wackeln)")]),
    ("InspectionMultConfig", "Multiplikatoren für den Verdacht.", [
        F("float", "boss"), F("float", "music"), F("float", "singingCow")]),
    ("VodkaConfig", "Wirkung von Wodka am Kommissar.", [
        F("float", "pauseSec"), F("float", "suspReduce")]),
    ("InspectionConfig", "Kontrolle durch den Kommissar (GDD 9).", [
        F("int", "fromDay"), F("float", "triggerAuthority"), F("float", "firstAfterDay2StartSec"), F("float", "gapSec"),
        F("float", "minGapSec"), F("float", "speed"), F("float", "range"), F("float", "suspMax"),
        F("SuspicionConfig", "susp"), F("InspectionMultConfig", "mult"), F("VodkaConfig", "vodka"),
        F("float", "prize"), F("float", "bossPrizeMult"), F("float", "bossAfterUltSec")]),
    ("InspectorConfig", "Weg und Darstellung des Kommissars (Prototyp inspG, Prototyp-Koordinaten).", [
        F("float", "startX", "Start rechts"), F("float", "endX", "Ende links (Kontrolle bestanden)"),
        F("float", "pathZ", "Weg vor der Kammer"), F("float", "vodkaReach", "Abstand für Wodka anbieten"),
        F("float", "vodkaScoreOffset", "Vorrang vor anderen Zielen (Wertung = Abstand − Wert)"),
        F("float", "bossScale", "Größe des Oberkommissars"), F("float", "swayDeg", "Schwanken beim Trinken"),
        F("float", "swayFreq"), F("float", "stepFreq", "Schritte pro s beim Gehen"), F("float", "legSwingDeg"),
        F("float", "startShake", "Kamerawackeln beim Eintreffen")]),
    ("MachineFxConfig", "Darstellung von Zentrifuge und Kryokammer (Prototyp updatePlay/kryWin).", [
        F("float", "centrifugeRotorRadPerSec", "Rotordrehung bei voller Drehzahl"), F("float", "centrifugeShakeAmp", "Wackeln des Gehäuses in m bei voller Drehzahl"),
        F("float", "centrifugeBreakShake", "Kamerawackeln beim Zerspringen"),
        F("float", "cryoGlowIdle"), F("float", "cryoGlowFreezing"), F("float", "cryoGlowPulse"),
        F("float", "iceBlockShowSec", "Eisblock im Fenster sichtbar"),
        F("float", "recordRadPerSec", "Grammophon-Platte"), F("float", "crankRadPerSec"), F("float", "notesPerSec", "Noten am Grammophon"),
        F("float", "singNotesPerSec", "Noten über der Singenden Kuh"), F("float", "milkDropsPerRate", "Milchtropfen beim Milchhuhn"),
        F("float", "stampSec", "Stempelbewegung des Verkostungsautomaten"), F("float", "probeGlowPulse")]),
    ("CinematicConfig", "Ultimativ-Höhepunkt (GDD 10, Prototyp ultCinematic).", [
        F("float", "durationSec", "Licht gedimmt, Tier leuchtet"), F("float", "lightFactor", "Bildhelligkeit während der Sequenz"),
        F("float", "directorLineDelaySec", "„… … …“, dann der Spruch"), F("float", "animalGlowBoost", "Tier leuchtet heller, damit es im Dunkeln strahlt"),
        F("float", "fadeSpeed", "Abdunkeln/Aufhellen pro s")]),
    ("ClothFxConfig", "Laken über der Kammer: Animation und verräterische Bewegung (GDD 6.8, Prototyp clothK).", [
        F("float", "speed", "Annäherung pro s beim Werfen/Abnehmen"),
        F("float", "wobbleDegPerLevel", "Wackeln je Mutationsgrad"), F("float", "wobbleBaseFreq", "Frequenz = Basis + Grad"),
        F("float", "unstableDeg", "Zittern bei INSTABIL"), F("float", "unstableFreq"),
        F("string", "omegaColor", "Goldenes Leuchten bei Ω"), F("float", "omegaGlow"), F("float", "omegaPulse"), F("float", "omegaFreq")]),
    ("CentrifugeZones", "Zonen des Drehzahlbalkens (0..1).", [
        F("float", "tooEarly"), F("float[]", "yellow"), F("float[]", "green"), F("float[]", "red")]),
    ("CentrifugeConfig", "Zentrifuge (GDD 6.3).", [
        F("int", "unlockPlanIdx"), F("float", "rpmAccelBase"), F("float", "rpmAccelPerRpm"),
        F("CentrifugeZones", "zones"), F("float", "concYellow"), F("float", "concGreen"),
        F("float", "spinDown"), F("float", "shakeAbove")]),
    ("CryoConfig", "Kryokammer (GDD 6.4).", [
        F("int", "unlockPlanIdx"), F("float", "freezeSec"), F("float", "takeWindowSec")]),
    ("UnlockConfig", "Nur eine Freischaltung.", [F("int", "unlockPlanIdx")]),
    ("TastingConfig", "Verkostungsautomat (GDD 6.6).", [
        F("int", "unlockPlanIdx"), F("float", "cost"), F("float", "qualityBonus"),
        F("float", "hiFrom", "Qualität ab der ein Lob kommt"), F("float", "midFrom", "Qualität für den mittleren Spruch")]),
    ("GramophoneConfig", "Grammophon (GDD 6.5).", [
        F("float", "crankSec"), F("float", "musicSec"), F("float", "heatExtraDecay"), F("float", "directorCommentChance")]),
    ("RohrpostConfig", "Rohrpost (GDD 6.7).", [F("float", "angerReduce"), F("float", "capsuleSec", "Flugzeit der Kapsel im Rohr")]),
    ("MicroscopeConfig", "Probenmikroskop (GDD 6.2).", [
        F("float", "cost"), F("float", "analyseSec"),
        F("float[]", "factorLevels", "Schwellen für ↑ / ↑↑ / ↑↑↑"),
        F("float[]", "qualityLevels", "Schwellen für ↓ / – / ↑"),
        F("float[]", "stabLevels", "Schwellen für nervös / ruhig")]),
    ("MachinesFixedConfig", "Feste Stationen im Labor.", [
        F("CentrifugeConfig", "centrifuge"), F("CryoConfig", "cryo"), F("UnlockConfig", "ultimate"),
        F("TastingConfig", "tasting"), F("GramophoneConfig", "gramophone"), F("RohrpostConfig", "rohrpost"),
        F("MicroscopeConfig", "microscope")]),
    ("BuildGridConfig", "Raster des Baumodus (GDD 3).", [
        F("int", "cols"), F("int", "rows"), F("float", "cell"), F("float", "x0"), F("float", "z0"),
        F("int", "blockedRow"), F("int", "blockedFrom"), F("int", "blockedTo")]),
    ("BuildMachinesConfig", "Baumodus allgemein (GDD 6.10). Maschinentypen liegen als eigene Assets vor.", [
        F("float", "priceFactor", "Preisfaktor pro gleichem Typ"), F("BuildGridConfig", "grid"),
        F("float", "sellRefund", "Anteil des Kaufpreises beim Abriss"),
        F("float", "autoSprayRetrySec", "Auto-Sprüher: neuer Versuch nach, wenn nichts passt"),
        F("float", "autoSprayWarnGapSec", "Auto-Sprüher: Meldung „kein Serum“ höchstens so oft"),
        F("float", "pipeY", "Höhe der Rohre zur Kammer"), F("float", "pipeGap", "Rohrende vor dem Kammerrand")]),
    ("BuildViewConfig", "Draufsicht im Baumodus (Prototyp camTop, Prototyp-Koordinaten).", [
        F("float", "camX"), F("float", "camY"), F("float", "camZ"), F("float", "lookX"), F("float", "lookY"), F("float", "lookZ"),
        F("float", "fov"),
        F("float", "hideBehindZ", "Umgebung hinter dieser z-Linie (Rückwand) wird ausgeblendet"),
        F("float", "hideLeftX", "Umgebung links dieser x-Linie (linke Wand) wird ausgeblendet"),
        F("float", "hideAboveY", "Umgebung über dieser Höhe (Decke) wird ausgeblendet")]),
    ("WishTypeConfig", "Ein Sonderwunsch-Typ (GDD 7.3).", [
        F("string", "id"), F("LinearValue", "amount", "Menge in t (falls benötigt)"),
        F("float", "minQuality"), F("float", "durationSec")]),
    ("WishesConfig", "Sonderwünsche des Direktors (GDD 7.3).", [
        F("int", "fromDay"), F("float", "firstAfterSec"), F("float[]", "intervalSec"),
        F("LinearValue", "reward"), F("WishTypeConfig[]", "types")]),
    ("ContractConfig", "Ein Sonderauftrag (GDD 7.4).", [
        F("string", "k", "Ware m/e/f"), F("float", "minQ", "Mindestqualität"),
        F("string", "who", "Auftraggeber"), F("string", "what", "Warenname")]),
    ("ContractsConfig", "Sonderaufträge (GDD 7.4).", [
        F("int", "fromDay"), F("float", "payMult"), F("ContractConfig[]", "list")]),
    ("InfectionConfig", "Infektion des Spielers (GDD 8).", [
        F("float[]", "symptomIntervalSec"), F("float[]", "symptomDurationSec"),
        F("float", "fast"), F("float", "slow"), F("string[]", "symptoms"),
        F("float", "levelStart", "Infektionswert bei Ansteckung"), F("float", "levelMore", "Zuwachs bei erneuter Ansteckung"),
        F("float", "levelPerSec", "Langsamer Anstieg pro s"), F("float", "firstSymptomSec", "Erstes Symptom nach"),
        F("float", "veilBase", "Grüner Schleier: Grunddeckkraft"), F("float", "veilPerLevel", "Schleier pro Infektionswert"),
        F("float", "veilPulse", "Anteil des Pulsierens"), F("float", "greenVeil", "Deckkraft beim Symptom grün"),
        F("float", "wobbleDeg", "Schwanken der Kamera beim Symptom wobble"), F("float", "wobbleFreq"),
        F("float", "blurRadius", "Unschärfe beim Symptom blur")]),
    ("WindowHungerConfig", "Fenster zeigt Warteschlange, wenn Volk oder Nahrung darunter.", [
        F("float", "popBelow"), F("float", "foodBelow")]),
    ("WindowParadeConfig", "Fenster zeigt Festumzug, wenn beide Werte erreicht sind.", [
        F("float", "popMin"), F("float", "foodMin")]),
    ("WorldConfig", "Spielwerte der Welt (GDD 7.7).", [
        F("float", "avgQStart"), F("float", "famineQ"), F("float", "famineRange"), F("float", "popMax"),
        F("GoodsValues", "supplyWeights"), F("float", "supplyPerDay"),
        F("float", "foodBase"), F("float", "foodPerSupply"), F("float", "foodMax"), F("float", "foodFamineFactor"),
        F("float", "envMax"), F("float", "envPerDose"), F("float", "envPerExplosion"),
        F("WindowHungerConfig", "windowHunger"), F("WindowParadeConfig", "windowParade")]),
    ("NewsConfig", "Staats-TV (GDD 11).", [
        F("float", "durationSec"), F("float", "levelDead"), F("float", "levelCrisis"), F("float", "levelWarn"),
        F("float", "anchorGoneFrom", "Ab diesem Hunger ist der Sprecher verschwunden (Automatikbetrieb)"),
        F("float", "skipAfterSec", "Überspringen erst nach"), F("int", "factsMax", "Höchstens so viele Taten im Lauftext"),
        F("float", "prognosisYearsPerPlan", "„PROGNOSE JAHR +n“ = Plan-Index × Wert"),
        F("float", "meatBarBase"), F("float", "meatBarPerPct"), F("float", "meatBarMin"), F("float", "meatBarMax"),
        F("float", "qualityBarBase"), F("float", "qualityBarPerPct"), F("float", "qualityBarMax"),
        F("float", "tickerSpeed", "Lauftext in Pixeln pro Sekunde")]),
    ("RecordsConfig", "Rekorde (GDD 12).", [
        F("float", "epsilon", "Neuer Rekord erst ab altem Wert + epsilon"), F("float", "directorChance", "Direktor meldet den Rekord")]),
    ("NewsStudioConfig", "Kamerafahrt und Sprecher im Studio (Prototyp studio, Studio-Koordinaten).", [
        F("float[]", "widePos"), F("float[]", "wideTarget"), F("float[]", "closePos"), F("float[]", "closeTarget"),
        F("float[]", "closeDeadPos"), F("float[]", "closeDeadTarget"), F("float", "fov"),
        F("float", "slumpDelaySec", "Sprecher sackt nach dieser Zeit zusammen"), F("float", "slumpSpeed")]),
    ("ToastSound", "Klang zu einer Meldung (Prototyp: SFX-Aufruf neben toast()).", [
        F("string", "key", "Textpfad der Meldung, z. B. ui.toasts.planMet"), F("string", "sound", "Klang-Id (Dateiname ohne Endung)")]),
    ("AudioConfig", "Ton (GDD 14, Prototyp SFX/updateAudio): Pegel, Tierlaute, Musik, Schleifen, Zuordnung zu Meldungen.", [
        F("float", "master", "Gesamtpegel (Prototyp master 0.45)"), F("float", "music", "Standard Musik 0..1"), F("float", "sfx", "Standard Effekte 0..1"),
        F("int", "voices", "Gleichzeitige Effektquellen"),
        F("float", "mooEveryMin", "Tierlaut frühestens nach s"), F("float", "mooEveryRand", "plus Zufall bis s"),
        F("float", "cloneVoiceDelay", "Tierlaut nach dem Klonen in s"),
        F("float", "mooBaseHz", "Grundton der Datei moo"), F("float", "mooHzPerA", "tiefer je A"), F("int", "mooWobbleFromB", "wobbelig ab B"),
        F("float", "marchBeatSec", "Marsch: Notenabstand"), F("float", "marchBaseHz", "Grundton der Datei march_note"), F("float[]", "marchNotes", "Melodie in Hz"),
        F("float", "grumbleBaseHz", "Direktor: Grundton bei Wut 0"), F("float", "grumbleHzPerAnger"), F("float", "grumbleRandHz"),
        F("float", "grumbleGapSec"), F("int", "grumbleMax"), F("int", "grumbleCharsPerSyllable"), F("float", "grumbleVolPerAnger", "Lautstärke +Anteil je Wutpunkt"),
        F("float", "whirClipHz", "Grundton der Datei whir_loop"), F("float", "whirBaseHz"), F("float", "whirHzPerRpm"),
        F("float", "coinEggChance", "Münzhuhn: Münzklang je Ei"),
        F("ToastSound[]", "toastSounds", "Meldung → Klang")]),
    ("SettingsConfig", "Einstellungen: Vorgaben und Grenzen (Maus, Grafik).", [
        F("float", "mouseDefault", "Mausempfindlichkeit Faktor"), F("float", "mouseMin"), F("float", "mouseMax"),
        F("float", "lowRenderScale", "Grafik Niedrig: Auflösungsfaktor"), F("float", "lowShadowDistance", "Grafik Niedrig: Schattenweite m"),
        F("int", "lowShadowResolution", "Grafik Niedrig: Schattenkarte"), F("int", "lowShadowCascades", "Grafik Niedrig: Kaskaden"),
        F("int", "windowedWidth", "Fenstermodus: Breite"), F("int", "windowedHeight", "Fenstermodus: Höhe")]),
    ("EndingsConfig", "Schwellen für die Spielenden (GDD 13).", [
        F("float", "heroMinPop"), F("int", "heroMaxFailedDays"), F("float", "deadBelowPop")]),
    ("ComboNameEntry", "Fester Name einer Kombination, z. B. 1-1-1 = Planformel.", [
        F("string", "key", "Dosis als A-B-C"), F("string", "displayName", "Name")]),
    ("RoomLayout", "Raummaße in Prototyp-Koordinaten (GDD 3).", [
        F("float", "sideWallX", "Seitenwände bei ±x"), F("float", "frontWallZ", "Wand hinter der Kammer (Proto-z)"),
        F("float", "backWallZ", "Rückwand hinter dem Spieler (Proto-z)"), F("float", "wallThickness"), F("float", "height", "Deckenhöhe")]),
    ("AreaLayout", "Rechteckiger Bereich in Prototyp-Koordinaten.", [
        F("float", "xMin"), F("float", "xMax"), F("float", "zMin"), F("float", "zMax")]),
    ("PlayerStartLayout", "Startpunkt des Spielers in Prototyp-Koordinaten.", [
        F("float", "x"), F("float", "z"), F("float", "eyeHeight", "Augenhöhe")]),
    ("ChamberLayout", "Glaskammer B-12 (GDD 3).", [
        F("float", "x"), F("float", "z"), F("float", "radius"), F("float", "height"), F("float", "baseY"),
        F("float", "playerCollisionRadius", "Spieler kommt der Kammermitte nicht näher")]),
    ("BeltLayout", "Förderband quer durch die Kammer.", [
        F("float", "xMin"), F("float", "xMax"), F("float", "halfWidth"), F("float", "height")]),
    ("ZoneLayout", "Zone über |x| und z in Prototyp-Koordinaten (Pult, Laken).", [
        F("float", "absXMin"), F("float", "absXMax"), F("float", "zMin"), F("float", "zMax")]),
    ("WindowLayout", "Fenster in der rechten Seitenwand.", [
        F("float", "x"), F("float", "y"), F("float", "z"), F("float", "width"), F("float", "height")]),
    ("BobConfig", "Kopfwippen und Neigen der Kamera beim Laufen.", [
        F("float", "phaseSpeed", "Schrittphase pro s beim Laufen"), F("float", "ampY"), F("float", "ampX"),
        F("float", "breatheAmp"), F("float", "breatheFreq"), F("float", "rollPerSpeedDeg", "Neigung pro m/s Seitwärtslaufen"),
        F("float", "fadeSpeed", "Ein-/Ausblenden des Wippens"), F("float", "rollSmoothing")]),
    ("HandsConfig", "Animation der Ego-Hände mit Spritze.", [
        F("float", "smoothing"), F("float", "bob"), F("float", "sway"), F("float", "idleAmp"),
        F("float", "holdX"), F("float", "holdY"), F("float", "holdZ"), F("float", "holdTiltDeg"),
        F("float", "thrustZ"), F("float", "thrustTiltDeg")]),
    ("PlayerConfig", "Spielfigur in Ego-Sicht (GDD 2, Prototyp updatePlay).", [
        F("float", "walkSpeed"), F("float", "turnSpeedDeg", "Q/E und Pfeiltasten"), F("float", "mouseSensitivityDeg", "Grad pro Mauspixel"),
        F("float", "lookUpMaxDeg"), F("float", "lookDownMaxDeg"), F("float", "radius"), F("float", "height"),
        F("float", "fov"), F("float", "fovMax"), F("float", "fovAspectRef", "Unter diesem Seitenverhältnis wird das Sichtfeld weiter"),
        F("BobConfig", "bob"), F("HandsConfig", "hands"),
        F("float", "fastTurnBoost", "Drehfaktor, wenn die Spielfigur beschleunigt ist (Infektion)"),
        F("float", "sprintMultiplier", "Tempo-Faktor beim Sprinten (Shift)")]),
    ("HoldTimesConfig", "Haltezeiten der Interaktionen in Sekunden.", [
        F("float", "pult"), F("float", "mixing"), F("float", "rackHide"), F("float", "cloth"), F("float", "vodka"),
        F("float", "tasting"), F("float", "rohrpost"), F("float", "beltRepair"), F("float", "centrifugeLoad"),
        F("float", "centrifugeBrake"), F("float", "cryoLoad"), F("float", "cryoTake"), F("float", "terminal")]),
    ("InteractionConfig", "Zielwahl und Haltezeiten (GDD 2, Prototyp pickTarget).", [
        F("float", "pultScore", "Wertung, wenn der Spieler in der Pult-Zone steht"), F("float", "clothScore"),
        F("float", "disabledHoldSec", "Haltezeit bei nicht möglichen Aktionen (nur Anzeige)"),
        F("AreaLayout", "mixingZone", "Bereich vor der Mischstation (Prototyp-Koordinaten)"), F("HoldTimesConfig", "hold")]),
    ("UiConfig", "Allgemeine Anzeige-Werte (GDD 7.6).", [
        F("int", "toastMax", "Gleichzeitige Kurzmeldungen"), F("float", "toastSec", "Anzeigedauer"),
        F("float", "toastFadeFrom", "Ab diesem Anteil der Dauer ausblenden"),
        F("float", "subtitleMinSec", "Untertitel mindestens so lange"), F("float", "subtitlePerChar", "plus pro Zeichen"),
        F("float", "boardRefreshSec", "Plantafel und TV-Bild auffrischen alle"),
        F("float", "dayEndDelaySec", "Pause zwischen Schichtende und Bestellterminal"),
        F("float", "contractAnnounceDelaySec", "Sonderauftrag wird so spät nach Schichtbeginn gemeldet")]),
    ("PultConfig", "Pult: Reinigung, Hebelanimation, Statuslampen (GDD 6.1).", [
        F("float", "cleanSec", "Dauer der Kammerreinigung"), F("float", "leverPullSec"), F("float", "leverPullDeg"),
        F("float", "statusTempWarn", "Hitze, ab der die Temperaturlampe gelb wird"),
        F("float", "statusVitBad", "Stabilität, unter der die Vitallampe rot wird"),
        F("float", "panelRefreshSec", "Auffrischtakt offener Panels"),
        F("float", "leashMinFloat", "Anleinen erst ab dieser Schwebehöhe möglich")]),
    ("AnimalConfig", "Tier in der Kammer: Bewegung, Eier, Schild, Meter (Prototyp updateSubject).", [
        F("float", "standY"), F("float", "enterDropHeight"), F("float", "enterDropSpeed"), F("float", "enterSpeed"),
        F("float", "exitSpeed"), F("float", "procStopBefore"), F("float", "popInSpeed"), F("float", "popOutSpeed"),
        F("float", "destroySec"), F("float", "sinkSpeed"), F("float", "flyRiseSpeed"), F("float", "flyDriftSpeed"),
        F("float", "flyGoneHeight"), F("float", "eggInterval"), F("float", "labelHeightCow"), F("float", "labelHeightHen"),
        F("float", "meterRateMaxCow"), F("float", "meterRateMaxHen"), F("int", "meterSegments"),
        F("float", "meterStabGood"), F("float", "meterStabMid"), F("float", "sprayToastBad"), F("float", "sprayToastWarn"),
        F("float", "gasFlashSec")]),
    ("ChamberDoorsConfig", "Hubtüren links (Einlass) und rechts (Auslass) der Kammer.", [
        F("float", "openSec", "Dauer Öffnen"), F("float", "closeSec", "Dauer Schließen"),
        F("float", "width", "Lichte Breite der Schleuse"), F("float", "height", "Lichte Höhe ab Kammerboden"),
        F("float", "lift", "Hubweg der Tür nach oben"), F("float", "radialOffset", "Abstand der Tür vor dem Glas"),
        F("float", "waitGapCow", "Kuh wartet so weit vor der geschlossenen Tür (Mitte, mal Größe)"),
        F("float", "waitGapHen"), F("float", "clearGapCow", "Tür schließt, wenn die Kuh so weit hindurch ist (mal Größe)"),
        F("float", "clearGapHen")]),
    ("ChamberFireConfig", "Feuer bei der Vernichtung (Not-Stopp) statt Gas.", [
        F("float", "afterSec", "Nachbrennen, nachdem das Tier weg ist"),
        F("float", "lightIntensity"), F("float", "flickerAmp"), F("float", "flickerFreq"),
        F("string", "lightColor"), F("string", "nozzleColor", "Farbe von Düsenlicht und Kegel während des Feuers"),
        F("float", "fadeSec", "Ein- und Ausblenden von Licht und Düsenfarbe")]),
    ("FxConfig", "Kamerawackeln und Effektstärken (Prototyp shake).", [
        F("float", "shakeExplosion"), F("float", "shakeProcExplosion"), F("float", "shakeBelt"), F("float", "shakeCable"),
        F("float", "shakeDecay", "Abbau pro s"), F("float", "shakeScale", "Meter Kameraversatz pro Wackel-Einheit"),
        F("int", "slimeSplats", "Schleimflecken am Boden nach einer Explosion"),
        F("float", "splatRadiusMin"), F("float", "splatRadiusMax"), F("float", "alarmTurnDegPerSec", "Drehung der Warnleuchte")]),
    ("AnimalVisualsConfig", "Sichtbare Mutation am Tier (GDD 5.5).", [
        F("float", "growPerA"), F("float", "growCapA"), F("float", "kolossGrowExtra"), F("float", "kolossA"), F("float", "omegaB"),
        F("float", "inflPerA"), F("float", "inflPerBOver2"), F("float", "inflBCap"), F("float", "inflSmoothing"),
        F("float", "udderPerB"), F("float", "udderBCap"), F("float", "udderSmoothing"), F("float", "henScale"),
        F("float", "glowFromInstab"), F("float", "glowFactor"), F("float", "glowOmega"), F("float", "glowSmoothing"),
        F("float", "emissiveFactor"), F("float", "lightFactor"),
        F("float", "legSwingFloatDeg"), F("float", "legSwingWalkDeg"), F("float", "legWalkFreq"), F("float", "legFloatFreq"),
        F("float", "wobbleUnstableDeg"), F("float", "wobbleLowStabDeg"), F("float", "lowStabWobbleBelow"),
        F("float", "floatBobAmp"), F("float", "floatRollDeg"), F("float", "floatBobFrom"),
        F("float", "wingFlapFast"), F("float", "wingFlapSlow"), F("float", "wingFlapDeg"),
        F("float", "floatRollFreq"), F("float", "floatBobFreq"), F("float", "wobbleUnstableFreq"), F("float", "wobbleLowStabFreq"),
        F("float", "pulseBase"), F("float", "pulseAmp"), F("float", "pulseFreq"), F("float", "pulseHeatFreq"), F("float", "pulseUnstableFreq"),
        F("string", "glowColor"), F("string", "heatColor"), F("string", "goldColor"), F("string", "spotColor"), F("string", "spotGlowColor"),
        F("float", "spotGlowFactor"), F("float", "unstableHeatColorMix"), F("float", "lightOffsetY"), F("float", "lightOffsetZ"),
        F("string", "funLabelColor"), F("float", "gasFlashAlpha"), F("float", "gasIdleAlpha"),
        F("float", "growSmoothing", "Weiches Wachsen: Annäherung pro s"),
        F("float", "extraGrowSpeed", "Zusatz-Euter/Wucherungen: Ein-/Ausblenden pro s"),
        F("float", "breatheFreq", "Atmen: Kreisfrequenz"), F("float", "breatheAmp", "Atmen: Stärke 0..1 (Blendshape)"),
        F("float", "breatheUnstableMult", "Atmen schneller bei INSTABIL"),
        F("float", "headLookDeg", "Kopf: Umschauen in Grad"), F("float", "headLookFreq", "Kopf: Umschau-Frequenz"),
        F("float", "headNodDeg", "Kopf: Nicken in Grad"), F("float", "headWalkBobDeg", "Kopf: Nicken beim Laufen"),
        F("float", "henHeadBob", "Huhn: Kopf vor/zurück beim Laufen in m"),
        F("float", "earFlickDeg", "Ohrenzucken in Grad"), F("float", "earFlickEverySec", "Ohrenzucken alle n s"), F("float", "earFlickSec", "Dauer eines Zuckens"),
        F("float", "tailSwingDeg", "Schwanz: Pendeln in Grad"), F("float", "tailFreq", "Schwanz: Frequenz"),
        F("float", "kneeBendDeg", "Knie beim Laufen in Grad"),
        F("float", "shiverDeg", "Zittern des Rumpfs bei INSTABIL in Grad"), F("float", "shiverFreq", "Zittern: Frequenz")]),
]

CS_KEYWORDS = {"base", "float", "string", "int"}

def field_line(t, n, c, d, j):
    out = []
    if c:
        out.append(f"        /// <summary>{c}</summary>")
    ident = ("@" + n) if n in CS_KEYWORDS else n
    if j:
        out.append(f"        [JsonProperty(\"{j}\")]")
    init = ""
    if t.endswith("[]"):
        init = f" = new {t[:-2]}[0]"
    elif t not in ("float", "int", "string", "bool"):
        init = f" = new {t}()"
    elif d is not None:
        init = f" = {d}"
    out.append(f"        public {t} {ident}{init};")
    return out

for cls, comment, fields in CLASSES:
    uses_json = any(f[4] for f in fields)
    lines = []
    lines.append("using System;")
    if uses_json:
        lines.append("using Newtonsoft.Json;")
    lines.append("")
    lines.append("namespace PlanErfuellt")
    lines.append("{")
    lines.append(f"    /// <summary>{comment}</summary>")
    lines.append("    [Serializable]")
    lines.append(f"    public class {cls}")
    lines.append("    {")
    body = []
    for f in fields:
        body.extend(field_line(*f))
    lines.extend(body)
    lines.append("    }")
    lines.append("}")
    with open(os.path.join(OUT, cls + ".cs"), "w", encoding="utf-8", newline="\n") as fh:
        fh.write("\n".join(lines) + "\n")
print(len(CLASSES), "Klassen geschrieben")
