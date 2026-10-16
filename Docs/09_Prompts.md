# Prompts für Claude Desktop (Unity-Umbau)

Reihenfolge einhalten. Immer **einen** Prompt schicken, warten bis die Phase fertig ist, im Unity-Editor selbst kurz testen, dann den nächsten. Wenn etwas nicht passt: im selben Chat korrigieren lassen, bevor es weitergeht. Pro Phase möglichst einen neuen Chat im selben Claude-Projekt beginnen (spart Kontext); `Docs/Fortschritt.md` hält den Stand fest.

---

## Prompt 0 – Einlesen und Plan (noch nichts bauen)

```
Lies alle Projektdateien: 01_GDD_Plan_erfuellt.md, 02_Balancing.json, 03_Texte_de.json, 04_Art_Direction.md, 05_Unity_Architektur.md, die Referenzbilder 07 und 08 und schau dir die Struktur von 06_Prototyp_Referenz.html an (das ist der funktionierende Browser-Prototyp, Three.js).

Prüfe dann mit deinen Unity-Tools:
- welche Unity-Version und Render-Pipeline das offene Projekt hat
- welche Pakete installiert sind
- welche Tools du über die Unity-Verbindung hast (Szenen bearbeiten, Scripts anlegen, Play Mode, Konsole lesen, Screenshots usw.)

Baue noch NICHTS. Antworte mit:
1. Was du verstanden hast (Kernkreislauf in 5 Sätzen)
2. Was dir im Dokument unklar oder widersprüchlich vorkommt (Fragen statt Annahmen)
3. Deinem Umsetzungsplan in Phasen passend zu den Prompts 1–12, mit Risiken
4. Was du von mir brauchst (Pakete, Assets, Einstellungen)
```

## Prompt 1 – Projekt-Setup und Grundgerüst

```
Phase 1: Setup.
- Richte das Projekt nach 05_Unity_Architektur.md ein: Ordnerstruktur Assets/_Project, Pakete (URP, Input System, TextMeshPro, ProBuilder), Namespace PlanErfuellt.
- Lege Docs/ an und kopiere die Übergabedateien hinein. Lege Docs/Fortschritt.md (Checkliste aller Phasen), Docs/Entscheidungen.md und Docs/Credits.md an.
- Szenen: Boot, Lab, NewsStudio (additiv), TestRange.
- Core: GameManager, GameState, EventBus, GameClock (pausierbar, Zeitraffer).
- Config: GameConfig als ScriptableObject + Editor-Importer, der 02_Balancing.json einliest und die Assets erzeugt. TextDatabase, die 03_Texte_de.json lädt (inkl. Zufallsauswahl nach Wut-Stufe und Platzhalter-Ersetzung).
- Formula.cs mit allen Formeln aus GDD 4.3/4.4/6.2 und EditMode-Tests mit der Testtabelle aus 05_Unity_Architektur.md.
- DebugMenu (F1, nur Editor/Development Build): Zeitraffer, Geld, Lager.
Prüfe: Projekt kompiliert ohne Fehler, Tests grün, Konsole leer. Aktualisiere Fortschritt.md und berichte kurz.
```

## Prompt 2 – Labor-Blockout und Licht

```
Phase 2: Labor bauen (Blockout, aber schon mit ordentlichen Proportionen).
- Baue das Labor in der Szene Lab nach GDD Abschnitt 3 (Koordinaten umrechnen: z_unity = -z_proto). Alle Stationen als Platzhalter-Prefabs mit korrekter Größe, Kollider und Schild.
- Glaskammer B-12, Förderband, Klonanlage, Verarbeitung, Pult, Mischstation, Zentrifuge, Kryo, Verkostung, Grammophon, Rohrpost, Bestellterminal, Lager, TV + Wut-Skala, Plantafel, Fenster, Laken-Zone, Baugitter (unsichtbar).
- Licht und Stimmung nach 04_Art_Direction.md: warmes Hauptlicht, grünes Kammerlicht, Fog, Bloom, Vignette. Statische Umgebung als Static markieren, Licht backen.
- Mach einen Screenshot aus Spielerstartposition und einen von oben und vergleiche mit 07/08. Nenne die größten Unterschiede.
Ziel: begehbarer Raum mit stimmigem Licht, noch ohne Logik. Fortschritt.md aktualisieren.
```

## Prompt 3 – Spieler und Interaktion

```
Phase 3: Spieler.
- FirstPersonController mit Input System (WASD, Maus mit Pointer Lock, Q/E drehen, Kopfwippen), Kollision mit Kammer (Radius 3.1) und Stationen, Bewegungsbereich laut GDD.
- Ego-Hände mit Spritze (einfaches Modell), Animation beim Halten/Interagieren.
- InteractionSystem nach 05_Unity_Architektur.md: IInteractable, nächstes Ziel, Prompt unten mittig mit Haltebalken, ausgegraut mit Begründung wenn nicht möglich.
- Jede Station bekommt eine Interactable-Komponente mit Platzhalter-Aktion und der Haltezeit aus dem GDD.
- Pause (P/Esc), Fadenkreuz.
Test im Play Mode: an alle Stationen laufen, Prompts prüfen. Fortschritt.md aktualisieren.
```

## Prompt 4 – Kernkreislauf: Mischen, Klonen, Sprühen, Produzieren, Verarbeiten

```
Phase 4: Kernkreislauf nach GDD 1.1, 4 und 5.
- Mischstation-Panel (UI): 3 Regler 0–8 begrenzt durch Lager, Infobox (bekannt/unbekannt), MISCHEN (2.5 s, Rührer und Flüssigkeit animiert), Serum ins Regal (max 6), roh-Erkennung für Ultimativ-Rezepte.
- Pult-Panel (UI): Serum-Regal mit Auswahl und Infobox, Hebel GAS/KLON Kuh/KLON Huhn/FÖRDERBAND, Status-Lampen, Aktionen (Gegenmittel ins Tier, anleinen, reinigen, NOT-STOPP). 3D-Hebel animieren.
- Tier: Zustandsmaschine (empty/enter/ok/unstable/exit/destroy/flying), Einfahren über das Band, Produktion Milch/Eier pro Sekunde, Verarbeitung → Fleisch, NOT-STOPP.
- Platzhalter-Modelle für Kuh und Huhn reichen, aber Mutation sichtbar (Skalierung, Euter, Glühen) und Schild über dem Tier.
- Entdeckung: Kombination erst nach dem Sprühen bekannt (GDD 4.4).
- HUD: Kasse, Rate, Lager-Chips, Tier-Meter Menge/Qualität/Stabilität, Toasts.
Test: Eine Kuh mit 1-1-0 muss Stabilität 62, Qualität 60, Rate 3.9 t/s zeigen. Fortschritt.md aktualisieren.
```

## Prompt 5 – Risiken, Warnzeichen, Explosion, Infektion

```
Phase 5: Risiko-System nach GDD 5.2–5.4 und 8.
- Hitze, Säure, Schweben, Bandriss (Reparatur am Bandmotor 1.6 s), Stromausfall (Kabel reißt, 12 s, Licht 20 %), Instabilität mit 8-s-Countdown und Sirene, Explosion (VFX Schleim und Feuer, Kamerawackeln, Flecken, Kammer verseucht, Laken zerfetzt).
- Warnzeichen 3–5 s vorher mit Ursachen-Toast (GDD 5.3).
- Infektion mit Frist und zufälligen Symptomen als Post-Processing- bzw. Steuerungseffekte, Heilung mit G.
- Krisen am Tier sichtbar (Dampf, Blasen, Zittern, Schweben).
Test mit DebugMenu im Zeitraffer: 2-2-0 auf eine Kuh → wird instabil → NOT-STOPP funktioniert; ohne NOT-STOPP Explosion. Fortschritt.md aktualisieren.
```

## Prompt 6 – Tagesablauf, Bestellung, Direktor

```
Phase 6: Meta-Kreislauf nach GDD 7.
- Bestellterminal-Overlay zu Tagesbeginn (6 Artikel), DayCycle mit 180 s, Quoten-Panel, Tagesziele und Prämien, Übererfüllung +50 %, verfehlter Tag wird wiederholt.
- Direktor im Fernseher: Figur (vorerst 2D-Grafik oder einfaches 3D vor rotem Hintergrund), Untertitel, Wut-Skala mit 3 Stufen (TV flackert bei Stufe 3), alle Wut-Regeln, zufällige Rufe, Leerlauf-Erkennung.
- TV-Bild und Plantafel per RenderTexture oder Canvas, Aktualisierung ca. alle 10 s (TV zusätzlich bei neuem Spruch).
- Einführung Tag 1 (GDD 7.5), Sonderwünsche und Sonderaufträge ab Tag 2.
- Spielwerte (GDD 7.7) rechts im HUD, Spielende (GDD 13) mit Endbildschirm.
Test: Tag 1 mit Einführung komplett durchspielen. Fortschritt.md aktualisieren.
```

## Prompt 7 – Kommissar und Laken

```
Phase 7: Parteikontrolle nach GDD 9 und 6.8.
- Parteikontrolle-Wert, Auslösung der Kontrolle (ab Tag 2), Kommissar-Figur läuft vor der Kammer vorbei, Verdachtsbalken über dem Kopf.
- Laken: werfen/abnehmen mit Animation, verrät Mutationen (Wackeln, Zittern, goldenes Leuchten bei Ω).
- Serum-Regal-Prüfung an der Mischstation + „Regal abdecken“, Wodka anbieten (Kommissar schwankt 10 s).
- Kontrolle bestanden → Prämie, Skandal → Spielende.
Test: Mutiertes Tier ohne Laken → Skandal; mit Laken und 1-1-0 → bestanden. Fortschritt.md aktualisieren.
```

## Prompt 8 – Zentrifuge, Kryo, Ultimativ-Höhepunkt

```
Phase 8: nach GDD 6.3, 6.4, 4.5 und 10.
- Zentrifuge als Timing-Minispiel mit Drehzahlbalken im Prompt (Zonen grau/gelb/grün/rot), Rotor dreht sichtbar, Gehäuse wackelt, Überdrehen zerstört das Serum.
- Kryokammer mit Einfrieren, Nebel, 12-s-Entnahmefenster, Eisblock.
- Ultimative Seren (ab Plan 3), Zwischensequenz beim ersten Einsatz (Licht aus, Tier leuchtet, Fanfare, Direktor sprachlos), danach Sondersendung und Oberkommissar.
Test: Mit DebugMenu Plan 3 setzen, 2-3-5 mischen → Zentrifuge grün → Kryo → Ω → Sequenz läuft. Fortschritt.md aktualisieren.
```

## Prompt 9 – Nebenstationen und Baumodus

```
Phase 9: nach GDD 6.5–6.7, 6.2 (Mikroskop) und 6.10.
- Grammophon (Kurbel, Musik, Effekte), Verkostungsautomat (Siegel, Stempel-Animation), Rohrpost (Kaviar, Kapsel fliegt), Probenmikroskop im Misch-Panel.
- Lustige Rezepte (GDD 4.6) mit Effekten.
- Baumodus: Draufsicht, Raster, Platzieren, Verkaufen, Preise ×1.5, Freischaltung, 4 Maschinen mit Wirkung, Rohre zur Kammer.
Fortschritt.md aktualisieren.
```

## Prompt 10 – Staats-TV, Logbuch, Rekorde, Fenster

```
Phase 10: nach GDD 11, 12 und 7.7.
- NewsStudio-Szene (additiv): Sprecher, Pult, Logo, Diagramm, Lauftext, Schlagzeile, Kamerafahrt, 4 Stufen inkl. „Automatikbetrieb“ ohne Sprecher, Sondersendung, verdrehte Taten im Lauftext, überspringbar.
- Logbuch im Stil des Forschungsnotizbuchs aus Bild 08: Karten mit Skizze, Name, Dosis, Vor- und Nachteile, Geheimakte, Kuriositäten, Rekordtafel. Nur Entdecktes anzeigen.
- Rekorde mit Toast.
- Fenster zeigt Volk: Warteschlange oder Festumzug (Sprites oder einfache Figuren).
Fortschritt.md aktualisieren.
```

## Prompt 11 – Grafik und Animation (Art-Pass)

```
Phase 11: Art-Pass nach 04_Art_Direction.md und den Bildern 07/08.
- Ersetze Platzhalter durch richtige Modelle (eigene Meshes/ProBuilder oder freie Assets – vorher mit mir abstimmen, welche Quellen; Lizenzen in Credits.md).
- Kuh und Huhn im beschriebenen Stil mit Animationen (Laufen, Atmen, Kopf, Zittern, Schweben, weiches Wachsen), Mutationen über Blendshapes oder Bones.
- Pult wie in Bild 08 (Tanks, Schieberegler, Statustafel, NOT-STOPP mit Klappe, Kipphebel).
- Deko, Banner, Poster mit den Texten aus 03_Texte_de.json, Materialien mit Abnutzung.
- HUD- und Panel-Design im Industrie-Stil.
Vorher/Nachher-Screenshots. FPS im Editor-Stats messen und berichten. Fortschritt.md aktualisieren.
```

## Prompt 12 – Ton, Feinschliff, Performance, Build

```
Phase 12: Abschluss.
- AudioService mit Mixer (Master/Musik/SFX), alle Sounds aus GDD 14 (freie Quellen oder prozedural), M stummschalten.
- Feinschliff: Übergänge, Kamerawackeln dosieren, Lesbarkeit der UI, Hauptmenü, Einstellungen (Lautstärke, Mausempfindlichkeit, Grafik Hoch/Niedrig).
- Performance-Pass nach GDD 15, Ziel 60 FPS; Profiler-Ergebnis berichten.
- Windows-Build erstellen, Startanleitung in Docs/README.md.
Spiele einen kompletten Durchlauf bis Plan 2 im Zeitraffer und berichte Fehler. Fortschritt.md abschließen.
```

---

## Nützliche Zwischen-Prompts

**Wenn etwas kaputt ist:**
```
Im Play Mode passiert X, erwartet war Y (laut GDD Abschnitt Z). Lies die Konsole, finde die Ursache, behebe sie und teste erneut. Erkläre kurz, was falsch war.
```

**Wenn der Kontext voll wird / neuer Chat:**
```
Wir setzen den Unity-Umbau von „Plan erfüllt“ fort. Lies Docs/Fortschritt.md und Docs/Entscheidungen.md, dann die Projektdateien. Mach mit Phase N weiter: [Prompt N einfügen].
```

**Abgleich mit dem Prototyp:**
```
Vergleiche das Verhalten von [System] mit 06_Prototyp_Referenz.html (Funktion [Name]). Nenne Abweichungen in Zahlen und Ablauf und gleiche sie an, wenn das GDD nichts anderes sagt.
```
