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
        F("float", "lateWarningSec", "Restzeit, ab der der Bildschirmrand pulsiert")]),
    ("StartConfig", "Startwerte eines neuen Spiels (GDD 4.1).", [
        F("float", "money", "Start-Kasse"),
        F("SubstanceAmounts", "inventory", "Startlager A/B/C"),
        F("int", "antidote", "Gegenmittel"), F("int", "vodka", "Wodka"), F("int", "caviar", "Kaviar")]),
    ("PricesConfig", "Einkaufspreise am Bestellterminal.", [
        F("float", "A"), F("float", "B"), F("float", "C"),
        F("float", "antidote"), F("float", "vodka"), F("float", "caviar")]),
    ("SubstanceConfig", "Ein Grundstoff (A/B/C) mit Anzeige-Daten.", [
        F("string", "id", "A, B oder C"), F("string", "code", "z. B. A-17"),
        F("string", "displayName", "Anzeigename", None, "name"), F("string", "color", "Farbe als Hex")]),
    ("MixingConfig", "Mischstation (GDD 4.2).", [
        F("int", "sliderMax", "Maximale Dosis pro Regler"),
        F("float", "mixTimeSec", "Mischdauer"), F("int", "rackSize", "Plätze im Serum-Regal")]),
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
        F("float[]", "shoutIntervalSec", "Abstand zufälliger Rufe [min, max]")]),
    ("AuthorityConfig", "Parteikontrolle (GDD 9).", [
        F("float", "max"), F("float", "base"), F("float", "lowStab"), F("float", "floating"), F("float", "glowFactor"),
        F("float", "omega"), F("float", "koloss"), F("float", "clothFactor"), F("float", "musicFactor"),
        F("float", "singingCow"), F("float", "contaminated"), F("float", "decay"), F("float", "explodeAdd"), F("float", "afterInspection")]),
    ("SuspicionConfig", "Verdacht pro Sekunde während der Kontrolle (GDD 9).", [
        F("float", "uncovered"), F("float", "uncoveredKoloss"), F("float", "coveredBase"), F("float", "coveredPerDose"),
        F("float", "coveredMaxDose"), F("float", "coveredUnstable"), F("float", "coveredOmega"), F("float", "coveredKoloss"),
        F("float", "contaminated"), F("float", "rack"), F("float", "rackRange"), F("int", "rackSuspiciousAB")]),
    ("InspectionMultConfig", "Multiplikatoren für den Verdacht.", [
        F("float", "boss"), F("float", "music"), F("float", "singingCow")]),
    ("VodkaConfig", "Wirkung von Wodka am Kommissar.", [
        F("float", "pauseSec"), F("float", "suspReduce")]),
    ("InspectionConfig", "Kontrolle durch den Kommissar (GDD 9).", [
        F("int", "fromDay"), F("float", "triggerAuthority"), F("float", "firstAfterDay2StartSec"), F("float", "gapSec"),
        F("float", "minGapSec"), F("float", "speed"), F("float", "range"), F("float", "suspMax"),
        F("SuspicionConfig", "susp"), F("InspectionMultConfig", "mult"), F("VodkaConfig", "vodka"),
        F("float", "prize"), F("float", "bossPrizeMult"), F("float", "bossAfterUltSec")]),
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
        F("int", "unlockPlanIdx"), F("float", "cost"), F("float", "qualityBonus")]),
    ("GramophoneConfig", "Grammophon (GDD 6.5).", [
        F("float", "crankSec"), F("float", "musicSec"), F("float", "heatExtraDecay"), F("float", "directorCommentChance")]),
    ("RohrpostConfig", "Rohrpost (GDD 6.7).", [F("float", "angerReduce")]),
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
        F("float", "priceFactor", "Preisfaktor pro gleichem Typ"), F("BuildGridConfig", "grid")]),
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
        F("float", "fast"), F("float", "slow"), F("string[]", "symptoms")]),
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
        F("float", "durationSec"), F("float", "levelDead"), F("float", "levelCrisis"), F("float", "levelWarn")]),
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
