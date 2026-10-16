# Plan erfüllt – Art Direction

Referenzbilder: `07_Referenz_Uebersicht.png` (Labor von oben) und `08_Referenz_Pult_Kammer.png` (Ego-Sicht auf Pult und Kammer). Ziel ist **diese Richtung**, keine exakte Kopie.

## Gesamteindruck
- **Stilisierter Realismus:** echte Materialien (abgenutzter Stahl, Messing, Glas, Beton, Stoff), aber leicht überzeichnete Formen, wie ein hochwertiges Indie-Spiel. Nicht fotorealistisch, nicht Low-Poly-Flat.
- **Stimmung:** sowjetische Industriehalle in der Spätschicht. Warmes Glühlampenlicht gegen giftiges Grün aus der Kammer, Dunst in der Luft, viele Rohre.
- **Humor über Inhalte, nicht über Grafikstil:** Die Umgebung ist ernst und schwer, die Tiere und Propaganda sind komisch.

## Farbpalette
| Rolle | Farbe |
|---|---|
| Propaganda-Rot (Banner, Teppich, Schilder) | #8E1B1B – #B3121B |
| Creme (Schrift auf Rot, Papier) | #F2E6C9 |
| Messing / Gold (Ringe, Embleme) | #B08A3A – #D4A017 |
| Stahl dunkel / Beton | #2F3236 / #5A5650 |
| Kammergrün (Gas, Glühen, Seren) | #6DFF4A / #3FFF7A |
| Stoff A Wachstum | #46D14F |
| Stoff B Laktation | #3AA0FF |
| Stoff C Stabilität | #FFCC2A |
| Warnung | Gelb-Schwarz-Streifen, Rot #FF3B2E |

## Schlüsselelemente
1. **Glaskammer B-12:** runder Glaszylinder mit dickem Stahl- und Messingrahmen, oben drei Sprühdüsen mit grünen Lichtkegeln, grüner Nebel innen, Schild „B-12“ und Plakette „FÜR EINE STÄRKERE VOLKSVERSORGUNG“. Bodenplatte mit Förderband.
2. **Pult (Ego-Sicht):** schweres Metallpult, links drei Glastanks (grün, blau, gelb) mit Schläuchen, Feld „MISCHUNG“ mit drei leuchtenden Schiebereglern, Statustafel „KAMMER B-12“ (GAS, TEMPERATUR, FÖRDERBAND, VITALWERTE mit Häkchen bzw. Lampen), großer roter NOT-STOPP-Knopf mit Klappdeckel und Warnstreifen, drei Kipphebel GAS / KLON / FÖRDERBAND (grün, schwarz, rot). Abnutzung, Schrauben, Kratzer.
3. **Klonanlage:** großer Zylinder „KLONANLAGE 01“ bzw. „KLON 02“ mit Kuhsymbol und Stern, Tiere kommen auf einem geschwungenen Förderband heraus.
4. **Fernseher und Plantafel:** großer Bildschirm mit Direktor vor rotem Hintergrund, Sprechblase oder Slogan („MEHR NAHRUNG · STÄRKERE MENSCHEN · HELLERE ZUKUNFT“, „PRODUKTIONSZIEL +20 %“). Darunter drei Tafeln MILCH / EIER / FLEISCH mit Icon, Balkendiagramm und „Ist / Soll t“. Banner „PLAN > PRODUKTION > WOHLSTAND“.
5. **Direktor:** stilisierte Figur (eckiges Kinn, Schnurrbart optional, Uniform mit Orden, Schulterklappen). **Keine echte Person, kein echtes Staatswappen.** Comic-Stil wie im Übersichtsbild ist bevorzugt, weil er besser zum Ton passt.
6. **Forschungslogbuch:** aufgeschlagenes Notizbuch mit Bleistiftskizzen von Kühen, Pfeilen, Notizen („Mehr Laktation? Stabil bei C+“), Häkchen-Liste. Vorbild für das Logbuch-UI (Papiertextur, Handschrift-Font).
7. **Mischstation mit großen Tanks A/B/C** (grün, gelb, rot im Übersichtsbild bzw. grün, blau, gelb im Pult-Bild – im Spiel gilt Grün/Blau/Gelb).
8. **Fenster:** Blick auf eine düstere Einöde, Stacheldraht, Wachtürme, Fabrik mit roten Fahnen, Abendmond.
9. **Deko:** Propagandabanner mit Ähren-Zahnrad-Emblem („PLAN · GENETIK · ERNÄHRT DAS VOLK“, „WISSENSCHAFT DIENT DEM VOLK“, „STABIL. PRODUKTIV. SOZIALISTISCH.“), roter Teppich mit Emblem, Schreibtisch mit Lampe, Röhrenmonitor, Aktenschränke, Bücherstapel (GENETIK, NÄHRSTOFFE, WACHSTUM, NEBENWIRKUNGEN?), Emaille-Tasse, Reagenzglas-Ständer, Mikroskop, Gitterroste im Boden, Laufstege mit gelbem Geländer, Kisten, Fässer.

## Tiere
- Kühe: niedlich-komisch, große Augen, rosa Schnauze, schwarz-weiß gefleckt, gelbe Ohrmarke. Mutiert: grotesk aufgebläht, viele große Euter (siehe Pult-Bild), grünes Glühen.
- Hühner: im gleichen Stil, weiß, roter Kamm.
- Animationen sind wichtig: Laufen auf dem Band, Atmen, Kopf drehen, Zittern bei Instabilität, Schweben mit strampelnden Beinen, Wachsen beim Sprühen (weich animiert, nicht springen).

## Licht und Rendering (URP)
- Ein warmes Hauptlicht (Mixed, mit Schatten), gebackenes Licht für statische Umgebung, wenige Echtzeit-Punktlichter (Kammer grün, TV rot).
- Volumetrischer Dunst bzw. Fog, Bloom für Leuchten (Kammer, Lampen, Bildschirme), leichte Vignette, Color Grading warm mit grünen Highlights.
- Emissive Materialien für Bildschirme, Lampen und Seren.
- Performance-Ziel 60 FPS (siehe GDD Abschnitt 15).

## UI
- HUD im Stil abgenutzter Industrieschilder: dunkle Paneele mit leichtem Rahmen, Schrift in Creme und Gelb, rote Akzente.
- Segment-Meter MENGE / QUALITÄT / STABILITÄT mit Icons (Flasche, Stern, Sanduhr) wie unten links im Pult-Bild.
- Schriften: kräftige Grotesk für Überschriften (z. B. Oswald oder Bebas), Mono für Zahlen (z. B. JetBrains Mono), Handschrift für das Logbuch (z. B. Caveat). Nur frei lizenzierte Schriften (SIL OFL).

## Assets
Vor dem Selbstbauen prüfen, ob es passende **kostenlose oder vorhandene Assets** gibt (Unity Asset Store, Kenney, Quaternius, Poly Haven für Texturen und HDRIs). Modelle, die selbst gebaut werden, mit ProBuilder oder als einfache Meshes, dann mit guten Materialien aufwerten. Jede Fremdquelle mit Lizenz in `Docs/Credits.md` eintragen.
