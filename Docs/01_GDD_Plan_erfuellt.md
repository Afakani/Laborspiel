# Plan erfüllt – Game Design Document (Unity-Umsetzung)

Stand: Oktober 2026 · Autor: Atakan (FH Aachen, Experimental Game Design)
Referenz-Implementierung: `06_Prototyp_Referenz.html` (Three.js-Browserprototyp, Version 6). Bei Unklarheiten gilt: **dieses Dokument > Balancing-JSON > Prototyp-Code**. Wenn etwas in keinem davon steht: nachfragen, nicht erfinden.

---

## 1. Überblick

**Genre:** Satirische Labor-/Management-Simulation in Ego-Perspektive, Einzelspieler.
**Plattform:** PC (Windows/macOS), Maus + Tastatur. Gamepad optional später.
**Ton:** Satire auf Planwirtschaft und Propaganda. Comichaft, nicht realistisch-brutal. Kein Blut. Tiere werden „verarbeitet“ (nur angedeutet, Tier fährt auf dem Band in eine Maschine).
**Altersempfehlung:** ab 12, Zielgruppe 16+.

**Fantasie des Spielers:** Du bist der Genosse Biochemiker im Forschungskombinat „VEB Bovine Biotech“, Kammer B-12. Du klonst Kühe und Hühner, mischst Wachstums-Seren und erfüllst die Tagesquote des Direktors – ohne dass die Partei merkt, was du da eigentlich züchtest.

**Kernidee (Designsäule):** *Risiko gegen Ertrag unter Beobachtung.* Mehr Serum = mehr Ertrag, aber instabile Tiere, Krisen und Verdacht. Jede Mechanik soll darauf einzahlen.

**Einheit Geld:** Stalinmünzen (SM).

### 1.1 Kernkreislauf (pro Tier)
1. **Mischen** an der Mischstation: Dosis A (Wachstum), B (Laktation), C (Stabilität) → Serum ins Regal (max. 6).
2. **Klonen** am Pult: Kuh (Milch) oder Huhn (Eier). Tier fährt auf dem Förderband in die Glaskammer.
3. **Sprühen** am Pult: Serum aus dem Regal wählen, GAS-Hebel. Dosen addieren sich am Tier.
4. **Produzieren:** Tier liefert laufend Milch/Eier (t/s).
5. **Risiko managen:** Überhitzung, Säure-Milch, Schweben, Instabilität → reagieren.
6. **Verarbeiten:** Förderband zur Verarbeitung → Fleisch (einmalig). Oder NOT-STOPP (Tier vernichtet, kein Ertrag).
7. Wiederholen bis Tagesende.

### 1.2 Meta-Kreislauf (pro Tag)
Bestellterminal (einmal pro Tag einkaufen) → Schicht (180 s) mit Quote → Tagesende → Staats-TV-Nachrichten → nächster Tag.
5 Pläne (Tagesziele steigen). Verfehlter Tag wird wiederholt (+Direktor-Wut). Spielende nach Plan 5, bei Wut 100 % (gefeuert) oder Skandal (Kommissar).

---

## 2. Steuerung

| Eingabe | Aktion |
|---|---|
| W A S D | Laufen |
| Maus (Pointer Lock) oder Q/E, Pfeiltasten | Umsehen / Drehen |
| Leertaste **halten** | Interagieren (jede Station hat eine Haltezeit, Fortschrittsbalken im Prompt) |
| Leertaste (in Panel) | Panel schließen |
| G | Gegenmittel selbst spritzen |
| L | Logbuch |
| B | Baumodus |
| M | Ton an/aus |
| P / Esc | Pause / Panel schließen |
| Enter | Bestellung bestätigen |

Interaktion: Die nächste Station in Reichweite wird Ziel. Prompt unten mittig zeigt Text + Haltebalken; ausgegraut, wenn nicht möglich (mit Begründung im Text, z. B. „Kryokammer – gesperrt bis Plan 2“).

---

## 3. Labor – Layout

Koordinaten aus dem Prototyp (Meter, Y = oben). **Umrechnung nach Unity: x bleibt, z_unity = −z_proto.** Dann schaut der Spieler beim Start (Prototyp z = 7.2 → Unity z = −7.2) in +Z-Richtung auf die Glaskammer.

| Objekt | Proto x | Proto z | Notiz |
|---|---|---|---|
| Glaskammer (Tank) „B-12“ | 0 | 0 | Zylinder r 2.5, Höhe 8.3, Messingringe, grünes Licht innen. Spieler kollidiert mit Radius 3.1 |
| Förderband | −10.4 … 10.4 | −1.15 … 1.15 | läuft quer durch die Kammer |
| Klonanlage | −8.5 | 0 | Tiere kommen von hier aufs Band |
| Verarbeitung | 9.2 | 0 | Tiere verschwinden hier → Fleisch |
| Pult | 0 | 3.75 | vor der Kammer; Zone: abs(x) < 1.95, z < 5.85 |
| Bandmotor | −6.6 | 1.7 | Reparatur bei Bandriss |
| Mischstation | −5.2 | 4.0 | Tisch mit 3 Tanks (A grün, B blau, C gelb), Rührschüssel, Regal mit 6 Fläschchen |
| Probenmikroskop | −6.5 | 4.35 | auf dem Mischtisch |
| Zentrifuge | 5.6 | 2.0 | Rotor mit 6 Röhrchen, Glaskuppel, Statuslampe |
| Kryokammer | 7.6 | 2.1 | Schrank mit Fenster, Nebel, Lampe |
| Verkostungsautomat | 10.2 | 3.3 | roter Automat, Schild „STAATLICH GEPRÜFT“, Stempelarm |
| Grammophon | 3.5 | 11.0 | an der Rückwand, Trichter, Kurbel |
| Rohrpost | −3.5 | 11.25 | Messingrohr zur Decke, Klappe |
| Bestellterminal | −12 | 0.8 | Bildschirm |
| Lager | 12 | 0.9 | Kisten (Anzahl zeigt Bestand) |
| Fernseher mit Direktor | −11.8 | −2.7 (y 5.0) | großer Röhren-TV an der Wand, daneben Wut-Skala |
| Plantafel | 11.8 | −2.7 (y 5.0) | Tagesziel + Balkendiagramm Verlauf |
| Fenster | Wand x = 16.7 | 5.2 | zeigt Einöde + Volk (Warteschlange / Festumzug) |
| Laken-Zone | abs(x) 1.9–4.3 | z < 4.7 | seitlich der Kammer |

Spielerbereich: x −16 … 16, z 2.3 … 10.8. Rückwand bei z ≈ 11.65. Deko-Zonen: abs(x) > 14.2 oder z > 10.2 (Regale, Spinde, Fässer, Schreibtisch mit Schreibmaschine, Aktenschränke, Pflanze, Uhr über der Tür, Propagandaposter, Warnschilder, Rohre an den Wänden, Deckenträger, Hängelampen).

Baumodus-Raster: 14 × 3 Zellen à 2 m, Zellmitte x = −13 + 2·gx, z = 5 + 2·gz. Gesperrt: gz = 0 und gx 3–7 (vor dem Pult).

**Optik-Richtlinie:** sowjetischer Retro-Industriestil, Rot/Creme/Messing/Dunkelgrau, Low-Poly mit sauberen Materialien, warmes Licht, grünliches Kammerlicht. Propaganda-Embleme (Zahnrad + Ähre, kein echtes Staatswappen). Lesbare Station-Schilder über jeder Station.

---

## 4. Stoffe, Seren und Formeln

### 4.1 Grundstoffe
| Stoff | Kürzel | Farbe | Preis |
|---|---|---|---|
| Wachstum | A-17 | #46d14f | 40 SM |
| Laktation | B-08 | #3aa0ff | 60 SM |
| Stabilität | C-04 | #ffcc2a | 50 SM |
| Gegenmittel | + | #f4f1ea | 150 SM |
| Wodka | – | – | 60 SM |
| Kaviar | – | – | 150 SM |

Start: 700 SM, Lager A 3 / B 3 / C 2, Gegenmittel 1.

### 4.2 Mischen
Regler 0–8 je Stoff (begrenzt durch Lager). Mischen dauert 2.5 s, verbraucht die Stoffe, legt ein Serum ins Regal (max. 6). Serum = {d:[A,B,C], stage, conc, frozen, ult}.
- stage `fertig` = normal. stage `roh` = Mischung entspricht einem Ultimativ-Rezept (Toast: „die Mischung brodelt seltsam …“). stage `zentri` = Rohserum nach der Zentrifuge.

### 4.3 Formeln (Summe der Dosen am Tier: A, B, C)
```
e        = 1 / (1 + 0.15·C)                      // Dämpfung durch C
Stab(d)  = clamp(100 − 14A − 18B − 6·A·B + 20C − 10·max(0, A+B+C−5), 0, 100)
Qual(d)  = clamp(80 − 8A − 12B + 8C, 0, 100)
Rate     = basis · (1 + 0.8·B·e + 0.15·A·e)      // basis: Kuh 2.0 t/s Milch, Huhn 0.8 t/s Eier
Fleisch  = basis · (1 + 0.7·A·e + 0.1·B·e)        // basis: Kuh 6 t, Huhn 2 t (einmalig bei Verarbeitung)
```
Modifikatoren:
- **Konzentriert** (Zentrifuge, conc 1.25 oder 1.5): Rate = basis + (Rate_formel − basis)·conc, Fleisch analog. Am Tier gilt das höchste conc.
- **Tiefgekühlt** (Kryo): +20 Stabilität am Tier pro gesprühtem tiefgekühltem Serum.
- **Musik** (Grammophon läuft): +15 Stabilität, Hitze sinkt zusätzlich 3/s.
- **Siegel** (Verkostung): +15 Qualität.
- **Säure-Milch:** Qualität ×0.4, Rate ×0.4, Fleisch ×0.5.
- Stabilität max. 100.

### 4.4 Kombinationsnamen (Logbuch)
Feste Namen in `02_Balancing.json → comboNames` (z. B. 1-0-0 Mastformel, 0-2-0 Euterflut, 1-1-1 Planformel, 3-3-0 Wahnsinnsmischung). Sonst generisch: A>B „Mastmischung“, B>A „Melkmischung“, A=B „Kombimischung“, A+B=0 „Stabilisator“; Zusatz „(stabilisiert)“ wenn C ≥ max(1,(A+B)/2), „(extrem)“ wenn A+B ≥ 5.

Vor-/Nachteile pro Kombi (comboInfo, auf Kuh-Basis berechnet):
- Fleisch +x % (wenn >4 %), Milch/Eier +x %, Qualität ±, „stabilisiert (+20·C Stabilität)“, „Wirkung gedämpft −x %“ (wenn C>0 und A+B>0), „Stabilität nur x %“ (<61), A≥2 Überhitzungsgefahr, B≥2 Säure-Milch möglich, B≥3 Tier schwebt, A≥3 belastet das Förderband, A+B=0 kein Ertragsgewinn. Fallback „kaum Wirkung, dafür sicher“ / „verbraucht Stoffe“.

**Wichtig – Entdeckung:** Eine Kombination erscheint erst im Logbuch, nachdem sie an einem Tier **gesprüht** wurde. Ungetestete Mischungen heißen „Mischung A#·B#·C#“ und zeigen keine Werte. Ultimative Rezepte werden nirgends verraten, bevor sie einmal benutzt wurden.

### 4.5 Ultimative Seren
| Name | Rezept (A-B-C) | Effekt | Nachteil |
|---|---|---|---|
| Planerfüller Ω | 2-3-5 | Rate ×3.5 (Basis), Qualität 92, Stabilität 100, keine Risiken, Fleisch ×1.3 | Tier leuchtet golden, Parteikontrolle steigt stark |
| Fleischkoloss | 4-1-4 | Fleisch ×4.5, Stabilität 95, Qualität 78, Rate ×1.2 | Riesig, Band reißt bei Verarbeitung zu 50 %, Kommissar bemerkt es sofort |

Herstellung: Rezept mischen (→ `roh`) → Zentrifuge (→ `zentri`) → Kryokammer → beim Herausnehmen ab Plan 3 (planIdx ≥ 2) ultimativ. Vorher: wird nur zu normalem tiefgekühltem Serum („Die Probe reagiert nicht richtig …“). Ein ultimatives Serum ersetzt alle Dosen am Tier.

### 4.6 Lustige Rezepte (Kuriositäten)
Greifen, wenn die **Gesamtdosis am Tier** genau passt:
| Name | Tier | Gesamt A-B-C | Effekt |
|---|---|---|---|
| Milchhuhn | Huhn | 1-3-1 | gibt Milch statt Eier, Rate ×1.6 |
| Singende Kuh | Kuh | 3-0-3 | singt (Noten-Partikel, Melodie), Parteikontrolle −0.8/s, Kommissar-Verdacht ×0.5, Fleisch ×0.8 |
| Münzhuhn | Huhn | 0-4-2 | +3 SM pro Ei (Münz-Funken), Qualität −15 |

Beim ersten Fund: Fanfare, Toast „KURIOS: …“, Direktor-Spruch, Nachrichtenmeldung.

---

## 5. Das Tier

### 5.1 Zustände
`empty → enter (fährt ein) → ok → (unstable) → exit (Verarbeitung) | destroy (NOT-STOPP) | flying (davongeschwebt) → empty`

### 5.2 Risiken (nur wenn kein Ultimativ und Basis-Stabilität S0 < 61; risk = 1 − S0/100)
- **Hitze:** bei A ≥ 2: heat += A·3.2·risk pro s, sonst −5/s (Kühlaggregat −8/s, Musik −3/s). Hitze > 55 = Krise „ÜBERHITZT“. Hitze 100 → instabil.
- **Säure:** bei B ≥ 2: acidT += risk pro s; > 9 → Säure-Milch (dauerhaft).
- **Schweben:** bei B ≥ 3: floatH += 0.45·(B−2)·risk pro s, sonst sinkt 1.5/s. > 2.4 = Krise „SCHWEBT DAVON“. > 7.5 = Tier weg, −300 SM. Gegenmaßnahme: am Pult anleinen (floatH → 0.3).
- **Bandriss:** A ≥ 3: Wahrscheinlichkeit 0.02·A pro s → Förderband gerissen (am Bandmotor 1.6 s halten).
- **Stromausfall:** S0 < 31: Wahrscheinlichkeit 0.03 pro s → Kabel reißt (Funken), 12 s Stromausfall: Licht 20 %, kein Klonen/Sprühen/Band, Produktion ×0.5.
- **Aktuelle Stabilität:** S = clamp(S0 − 0.4·heat). S ≤ 0 oder heat ≥ 100 → **INSTABIL**: 8 s Countdown, Sirene, Tier zittert und spritzt Schleim. Danach Explosion.

### 5.3 Warnzeichen (3–5 s vor der Krise, je einmal pro Tier mit Hinweis-Toast zur Ursache)
- Hitze 30–55: Dampf + Zischen. „Es dampft! Überhitzung droht – Ursache: viel Wachstum A. Hilft: Kühlaggregat, Musik, Not-Stopp.“
- acidT > 4: Blasen + Blubbern. „Die Milch blubbert! Säure droht – Ursache: viel Laktation B. Hilft: Gegenmittel ins Tier oder verarbeiten.“
- floatH 1–2.4: „Das Tier hebt ab – am Pult anleinen!“
- Stabilität < 20: Zittern + Warnton. „Das Tier zittert – Stabilität kritisch! Mehr C-04 oder Not-Stopp.“

### 5.4 Explosion
Schleim-/Feuerpartikel, starkes Wackeln, grüne Flecken am Boden, Brandfleck. Kammer verseucht (Klonen gesperrt, am Pult 3 s reinigen). Laken wird zerfetzt. Parteikontrolle +20, Direktor-Wut +10. Infektion des Spielers wenn Abstand zur Kammer < 7 m oder 50 % Zufall (**erst ab Tag 2**).

### 5.5 Optische Mutation
- Wachstum: Tier wächst (Skalierung 1 + 0.12·min(A,6)), wird aufgebläht.
- Laktation: Euter größer, zusätzliche Euter (Kuh) bzw. Auswüchse (Huhn).
- Laktation ≥ 3: aufgeblasen wie ein Ballon.
- Instabilität > 40 %: grünes Glühen, rot bei Hitze; instabil: pulsierend rot, zittert.
- Ω: goldenes Glühen + Funken. Koloss: riesig.
- Schwebend: leichtes Wippen, Beine strampeln, Huhn flattert.
- Schild über dem Tier: A/B/C-Dosen (oder Name des Ultimativs/der Kuriosität) + Status („STABILITÄT 74 %“, „ÜBERHITZT“, „INSTABIL · 6 s“), roter Stempel „GEPRÜFT“ bei Siegel.

Kuh: schwarz-weiß gefleckt, Ohrmarke. Huhn: weiß, roter Kamm, Flügel. Animationen: Laufen beim Einfahren/Ausfahren, Atmen, Wippen, Beinbewegung beim Schweben. **Diese Animationen sind wichtig und sollen erhalten bleiben.**

### 5.6 Verarbeitung
Förderband (auch bei instabilem Tier erlaubt) → Fleisch = Fleischformel (×0.5 bei Säure).
**Mut-Bonus:** War das Tier instabil oder minStab < 15: 30 % Explosion in der Verarbeitung (kein Fleisch, Wut +10), sonst +150 SM Bonus.

---

## 6. Stationen

### 6.1 Pult (Panel)
Linke Spalte: Serum-Regal (Liste, Auswahl, Infobox mit Vor-/Nachteilen nur wenn bekannt). Mitte: Hebel GAS (sprühen), KLON Kuh, KLON Huhn, FÖRDERBAND (bei instabil: „riskieren (Mut-Bonus)“). Status-Lampen: Gas, Temperatur, Förderband, Vitalwerte. Rechte Spalte: Gegenmittel ins Tier (setzt Tier zurück), Tier anleinen, Kammer reinigen, NOT-STOPP (blinkt bei instabil). Physischer Pult in 3D: 3 Drehknöpfe, 3 Hebel (animieren beim Ziehen), großer roter Knopf mit Warnstreifen, Statusanzeige.

### 6.2 Mischstation (Panel)
3 Regler mit Lagerbestand, Infobox (bekannt: Werte; unbekannt: „Unbekannte Mischung“), Knopf MISCHEN, Knopf **PROBE UNTERS MIKROSKOP · 20 SM** (nur für unbekannte Mischungen, einmal pro Mischung): zeigt nach 2 s vage Tendenzen:
`Fleisch ↑↑↑/↑↑/↑/– · Milch ↑↑↑/… · Geschmack ↑/–/↓/↓↓ · Probe ruhig/nervös/brodelt gefährlich`
(Schwellen: Faktor > 1.8 ↑↑↑, > 1.35 ↑↑, > 1.04 ↑; Qualität ≥ 85 ↑, ≥ 75 –, ≥ 55 ↓, sonst ↓↓; Stab ≥ 61 ruhig, ≥ 31 nervös.)
3D: Rührer dreht beim Mischen, Flüssigkeit in Farbe der Mischung, Tankfüllstände zeigen Lager, Fläschchen im Regal in Serumfarbe.

### 6.3 Zentrifuge (Timing-Minispiel, ab Start)
Lädt das erste passende Serum (Rohserum oder normales, noch nicht konzentriertes). Drehzahl steigt: rpm += dt·(0.16 + 0.12·rpm), 0→1. Anzeige im Prompt: Balken mit Zonen (0–60 % grau, 60–75 % gelb, 75–90 % grün, 90–100 % rot) und Nadel. Leertaste = bremsen.
- < 60 %: „Zu früh gebremst – nichts passiert.“
- 60–75 % oder 90–100 %: konzentriert ×1.25
- 75–90 %: PERFEKT ×1.5 (Münzton)
- Rohserum wird unabhängig davon zu `zentri`.
- rpm ≥ 100 %: Gläser zerspringen, Serum verloren.
Danach läuft der Rotor aus, Serum geht automatisch zurück ins Regal (wenn Platz). Rotor dreht sichtbar mit rpm, Gehäuse wackelt über 85 %, Lampe gelb/grün/rot, Surren steigt mit Drehzahl.

### 6.4 Kryokammer (ab Plan 2)
Lädt Zentri-Serum oder normales nicht-tiefgekühltes. 16 s einfrieren (Nebel). Danach **12 s Zeitfenster** zum Herausnehmen (Lampe blinkt, Warnton), sonst Eisblock (Serum verloren). Ergebnis: tiefgekühlt (+20 Stab beim Sprühen) bzw. Ultimativ (siehe 4.5).

### 6.5 Grammophon
Kurbel 1.5 s halten → 30 s Volksmusik (Marschmelodie): Stabilität +15, Hitze −3/s, Parteikontrolle-Anstieg ×0.6, Kommissar-Verdacht ×0.7. Platte dreht, Noten-Partikel. 50 %: Direktor „Ah, der Marsch der Melkerinnen! Herrlich!“

### 6.6 Verkostungsautomat (ab Plan 2)
0.8 s halten, 30 SM: Tier bekommt Siegel „STAATLICH GEPRÜFT“ (+15 Qualität), Stempel-Geräusch, Spruch je nach Qualität (Liste in Texte-JSON).

### 6.7 Rohrpost
Kaviar an den Direktor: Wut −25, Kapsel saust das Rohr hoch (Whoosh), Direktor-Spruch.

### 6.8 Laken
Seitlich an der Kammer 0.8 s halten: Laken über die Kammer werfen/abnehmen. Abgedeckt: kein Sprühen, Parteikontrolle-Anstieg ×0.4, Kommissar-Verdacht stark reduziert. Laken verrät aber: wackelt je nach Mutationsgrad, zittert bei instabil, leuchtet golden bei Ω.

### 6.9 Bestellterminal
Zu Beginn jedes Tages (Overlay, Spiel pausiert): 6 Artikel mit −/+ (A, B, C, Gegenmittel, Wodka, Kaviar), Lagerbestand, Summe, Kasse. Info: Soll des Tages, Infektionsfrist. Nach Bestätigung: Lieferung (Funken am Lager), Tag startet. Während des Tages: „Bestellungen nur zu Schichtbeginn“.

### 6.10 Baumodus (B)
Isometrische Draufsicht, Rasterplatzierung, Preise steigen ×1.5 pro gleichem Typ, Verkaufen-Werkzeug. Maschinen:
| Typ | Preis | Effekt | Freischaltung |
|---|---|---|---|
| Melkpumpe | 400 | +1.4 t Milch/s bei Kuh (Qualität 85) | Plan 1 |
| Auto-Sprüher | 450 | sprüht alle 15 s das zuletzt benutzte Serum, falls im Regal | Plan 2 |
| Kühlaggregat | 350 | Hitze −8/s | Plan 3 |
| Notfall-Automat | 650 | spritzt bei Krise/Instabilität automatisch Gegenmittel ins Tier (alle 3 s möglich) | Plan 4 |
Rohre von Maschine zur Kammer. Bei Stromausfall halbe Leistung.

---

## 7. Tagesablauf, Quote, Direktor

### 7.1 Tagesziele (180 s pro Tag)
| Plan | Milch | Eier | Fleisch | Prämie |
|---|---|---|---|---|
| 1 | 200 | 60 | 30 | 300 |
| 2 | 270 | 80 | 42 | 350 |
| 3 | 360 | 105 | 56 | 400 |
| 4 | 470 | 135 | 72 | 450 |
| 5 | 600 | 170 | 92 | 0 |
Bezahlung pro t: Milch 1, Eier 1.6, Fleisch 2.2 SM. **Übererfüllung:** über Soll zahlt jede Tonne +50 % (Toast beim ersten Überschreiten je Ware). Plan erfüllt: Prämie, Wut −20, Fanfare. Tagesende ohne Erfüllung: Wut +34, Tag wird wiederholt.

### 7.2 Direktor (im Fernseher)
Dicker Parteidirektor mit Schnurrbart und Orden, spricht über Untertitel (Text-Box „DIREKTOR: …“) und Grummel-Geräusch. Wut-Skala 0–100 neben dem TV (3 Stufen: <34 freundlich, <67 gereizt, ≥67 cholerisch → TV flackert, rotes Licht). Sprüche je Stufe (Texte-JSON: start, shouts, idle, met, fail, boom, unstable, inspect, collapse, ult, stop, passed, fun).
- Leerlauf > 6 s (nicht laufen, nicht interagieren, kein Panel): Wut +0.6/s.
- Explosion +10, Zusammenbruch +10, Plan verfehlt +34, Plan erfüllt −20, Kontrolle bestanden −10, Kaviar −25, Sonderwunsch erfüllt −15 / verfehlt +10.
- Wut 100 → gefeuert (Spielende).
- Zufällige Rufe alle 9–22 s (je wütender, desto öfter).

### 7.3 Sonderwünsche (ab Tag 2, alle 70–110 s, erster nach ~35 s)
- Fleisch: 10 + 5·planIdx t in 75 s
- Eier: 15 + 6·planIdx t in 70 s
- Qualität: ein Tier mit Qualität ≥ 85 verarbeiten in 90 s
- Ruhe: 60 s ohne Explosion und ohne Krise
Belohnung 150 + 50·planIdx SM und Wut −15; verfehlt Wut +10. Anzeige im Quoten-Panel mit Countdown.

### 7.4 Sonderaufträge (ab Tag 2, einer pro Tag)
Zahlt **doppelt** für eine Ware ab Mindestqualität: Krankenhaus Milch ≥ 80 %, Kindergarten Eier ≥ 75 %, Parteibankett Fleisch ≥ 70 %, Schulspeisung Milch ≥ 70 %.

### 7.5 Einführung (Tag 1)
Geführt durch den Direktor, Schritt im Quoten-Hinweis „EINFÜHRUNG x/4“:
1. Mischstation: Serum mischen. („Genosse, das da links ist ein Regler. Regler gut. Explosion schlecht. Verstanden?“)
2. Pult: Kuh klonen.
3. Pult: Serum sprühen.
4. Förderband: Tier verarbeiten.
An Tag 1: **keine Kontrolle, keine Infektion, keine Sonderwünsche/-aufträge.**

### 7.6 HUD
- Oben: Leiste „VEB · FORSCHUNGSKOMBINAT PLAN ERFÜLLT“, Kasse (SM), Rate (t/s), Tag + Restzeit, Plan x/5.
- Links oben: Chips A/B/C/Gegenmittel/Seren im Regal; darunter Quoten-Panel (Milch/Eier/Fleisch mit Balken, Zeit, Sonderwunsch/Auftrag, Hinweistext).
- Rechts: Spielwerte Nahrung, Volk, Umwelt, Direktor-Wut, Parteikontrolle, Infektion (nur wenn infiziert); Knöpfe Logbuch, Staats-TV, Ton.
- Links unten: Tier-Meter Menge/Qualität/Stabilität (12 Segmente).
- Unten mittig: Interaktions-Prompt (+ Zentrifugen-Balken). Darüber: Direktor-Untertitel.
- Rechts unten: Baumodus-Knopf. Toasts (max. 4) rechts.
- Roter Bildschirmrand pulsiert bei hoher Wut / wenn Zeit < 12 s.

### 7.7 Spielwerte (Welt)
```
avgQ    = Summe(t·Q) / Summe(t)  (Start 80)
famine  = clamp((62 − avgQ)/40, 0, 1)
Volk    = 100·(1 − famine)
supply  = (Milch + 2·Eier + 3·Fleisch) / max(1, Tag·5)
Nahrung = clamp(55 + 0.35·supply, 0, 130)·(1 − 0.6·famine)
Umwelt  = clamp(100 − Dosen gesamt − 8·Explosionen, 0, 100)
```
Fenster: Volk < 65 oder Nahrung < 50 → Warteschlange vor leerem Milchladen; Volk ≥ 85 und Nahrung ≥ 80 → Festumzug mit Fahnen, Konfetti, Banner „DANKE, GENOSSE BIOCHEMIKER!“.

---

## 8. Infektion
Durch Explosion (ab Tag 2). Frist: bis Ende des nächsten Tages (2 Tage). Grüner Bildschirm-Schleier, steigender Infektionswert. Alle 10–22 s ein zufälliges Symptom für 7–12 s: grüne Sicht, viel zu schnell (×2.4), bleischwer (×0.45), verschwommen, schwankender Boden, Steuerung invertiert. Heilung: G (verbraucht Gegenmittel). Frist abgelaufen → Zusammenbruch: Tag verloren, Wut +10.

---

## 9. Parteikontrolle & Kommissar

**Parteikontrolle (0–100):** steigt bei mutiertem Tier um (0.35 + 0.8 wenn Stab < 61 + 1 wenn schwebend + 0.8·Glühen + 1.5 Ω / 1 Koloss) pro s, ×0.4 mit Laken, ×0.6 mit Musik; −0.8/s bei Singender Kuh; +0.5/s bei verseuchter Kammer; sinkt sonst um 0.6/s.

**Kontrolle** startet (ab Tag 2) bei Parteikontrolle ≥ 75 oder nach Zeitplan (erste ~70 s nach Start von Tag 2, danach alle 100 s), mindestens 45 s Abstand. Kommissar (graue Uniform, Mütze mit rotem Band, Klemmbrett, Schild „KOMMISSAR“, Verdachtsbalken über dem Kopf) läuft von rechts nach links vor der Kammer vorbei (1.5 m/s).
Verdacht pro s (wenn Tier mutiert und Kommissar innerhalb 4.2 m der Kammer):
- ohne Laken: 22 (Koloss 40)
- mit Laken: 2 + 0.6·Mutationsgrad (max 8) + 6 wenn instabil; Ω 9; Koloss 12
- verseuchte Kammer: +6
- **Serum-Regal:** an der Mischstation +12/s, wenn verdächtige Seren offen stehen (ultimativ, roh/zentri oder A+B ≥ 5). Gegenmaßnahme: während der Kontrolle an der Mischstation „Serum-Regal abdecken“.
- Multiplikatoren: Oberkommissar ×1.6, Musik ×0.7, Singende Kuh ×0.5.
- **Wodka:** am Kommissar 0.5 s halten → 10 s Pause (er schwankt), Verdacht −40.
Verdacht 100 → Skandal (Spielende). Kontrolle bestanden → +400 SM (Oberkommissar ×2), Wut −10, Nachrichtenmeldung.

---

## 10. Ultimativ-Höhepunkt
Beim **ersten** Sprühen eines Ultimativ-Serums: Licht dimmt auf 10 %, nur das Tier leuchtet, Fanfare, Direktor sprachlos („… … …“, dann „Ein ultimatives Serum?! Das melde ich als MEINE Erfindung!“). Nach 4.5 s: **Sondersendung** im Staats-TV („Kombinat B-12 erschafft ‚Planerfüller Ω‘ – Direktor nimmt Orden persönlich entgegen“). 25 s danach kommt der **Oberkommissar** (größer, dunkelrotes Schild, Verdacht ×1.6, Prämie ×2).

---

## 11. Staats-TV (Nachrichten)
Nach jedem Tag automatisch, sonst über Knopf (nur nach erster Mutation). Kamera wechselt in ein separates TV-Studio (Sprecher am Pult, Pflanze, Logo „ZF Zentrales Staatsfernsehen“, Diagramm „Bevölkerung (offiziell)“ Plan vs. Ist), Lauftext unten, Schlagzeile, „LIVE“. Stufe nach Hungersnot: boom / warn / crisis / dead (Sprecher verschwunden, Automatikbetrieb). Texte in Texte-JSON mit Platzhaltern {year} {meat} {quality} {pop} {earned}. **Konkrete Taten** werden verdreht in den Lauftext eingefügt (Explosion → „kontrollierte Wärmefreisetzung“, davongeschwebte Kuh → „in den Weltraum entsandt“, Infektion → „gesunde Gesichtsfarbe“, Kuriositäten, Kontrollen, Aufträge). Länge 9 s, überspringbar.

---

## 12. Logbuch (L)
Karten für jede **getestete** Kombination: Skizze (einfache Strichzeichnung der Kuh, Größe nach A, Euter nach B, gestrichelter Rahmen bei C), Name, Dosis, Stabilität, Vorteile (grün), Nachteile (rot). Abschnitte: Geheimakte (nur benutzte Ultimative, mit Rezept), Kuriositäten (gefundene lustige Rezepte), Rekordtafel (höchster Ertrag t/s, meistes Fleisch aus einem Tier, Tagesrekorde je Ware; neuer Rekord → Toast + Fanfare, Direktor meldet ihn „unter MEINEM Namen“).

---

## 13. Spielende
- Plan 5 erfüllt → „final“: Volk ≥ 70 und ≤ 1 verfehlter Tag → Held der Planwirtschaft; Volk < 40 → Held eines leeren Landes; sonst Solides Mittelmaß.
- Wut 100 → Fristlos entlassen. Verdacht 100 → Skandal in der Säule.
Endbildschirm mit Statistik: Pläne, Schichten, verfehlte Tage, Volk, Milch/Eier/Fleisch, Kasse, Versuche, Explosionen, Maschinen, Spielzeit. Texte in Texte-JSON.

---

## 14. Ton
Muhen (tiefer bei viel A, wobbelig bei B ≥ 2/Ultimativ), Gackern, Singende Kuh (Melodie), Zischen (Sprühen/Dampf), Blubbern (Mischen/Säure), Explosion, Sirene (instabil, Schleife), Surren der Zentrifuge (Tonhöhe = rpm), Glas klirrt, Eis knistert, Kurbel, Marschmusik (Grammophon), Münze, Fanfare, trauriges Motiv (Plan verfehlt), Stempel, Rohrpost-Whoosh, Warnton, Klick (Hebel/Knöpfe), Grummeln des Direktors (Länge nach Text, tiefer/lauter bei Wut). Prototyp erzeugt alles per WebAudio-Synthese; in Unity echte Clips oder prozedural – Lautstärke-Mixer mit Master/Musik/SFX, Stummschalten (M).

---

## 15. Technische Anforderungen (Unity)
- Unity 6 LTS (falls nicht installiert: 2022.3 LTS), **URP**, neues **Input System**, **TextMeshPro**, Cinemachine optional.
- Ziel 60 FPS auf Mittelklasse-Laptop: wenige Echtzeit-Lichter (Hauptlicht mit Schatten + max. 4 Punktlichter ohne Schatten), Schatten nur für große Objekte, statische Deko als Static + gebackenes Licht, GPU-Instancing für Deko, UI-Texte nicht jeden Frame neu setzen. Plantafel und TV-Bild ca. alle 10 s aktualisieren (TV zusätzlich bei neuem Spruch).
- Alle Zahlen in **ScriptableObjects** (aus `02_Balancing.json`), alle Texte aus `03_Texte_de.json` (später lokalisierbar). Keine Magic Numbers im Code.
- Debug-Menü (nur im Editor/Development Build): Zeitraffer, Geld/Lager setzen, Tag/Plan setzen, Tier spawnen, Kontrolle auslösen, Ultimativ geben.
