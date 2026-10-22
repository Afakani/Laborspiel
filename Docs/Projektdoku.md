# Plan erfüllt – Projektdokumentation

Kurzfassung für die Abgabe. Details: Spielregeln im GDD (`01_GDD_Plan_erfuellt.md`), Umsetzung in `Fortschritt.md`, alle eigenen Entscheidungen in `Entscheidungen.md` (D1–D144), Starten und Steuerung in `README.md`.

## 1. Idee

Satire auf Planwirtschaft. Der Spieler leitet als Biochemiker das Labor B-12 eines Volkseigenen Betriebs. Die Partei verlangt jeden Tag Milch, Eier und Fleisch. Dafür werden Kühe und Hühner geklont und mit selbst gemischten Seren aufgepumpt – je stärker das Serum, desto mehr Ertrag, desto instabiler das Tier. Ein Kommissar kontrolliert das Labor und darf keine Mutationen sehen, der Direktor schaut aus dem Fernseher zu und wird wütend, wenn der Plan wackelt. Ziel: fünf Pläne erfüllen, ohne entlassen oder verhaftet zu werden.

## 2. Spielablauf

1. **Einsatzbesprechung** in sechs Seiten, danach Einführung (geführte Schritte mit Pfeil) oder freies Spiel.
2. **Schichtbeginn:** Auftragskarte mit dem Tagessoll, die Zeit läuft erst nach „Schicht beginnen“.
3. **Schicht (5 Minuten):** mischen (A-17 Wachstum → Fleisch, B-08 Laktation → Milch/Eier, C-04 Stabilität), klonen, Gas-Hebel ziehen, Tier beobachten, bei Gefahr Not-Stopp, verarbeiten (Fleisch). Nebenbei: Kommissar mit Laken täuschen, Direktor mit Kaviar beruhigen, Maschinen bauen, für morgen bestellen.
4. **Schichtende:** Plan erfüllt → nächster Plan, sonst wird der Tag wiederholt. Staats-TV-Sendung je nach Ergebnis.
5. **Spielende:** fünf Pläne erfüllt, entlassen (Wut 100 %), Skandal (Kommissar sieht Mutation) oder Infektion.

## 3. Technik

| Bereich | Umsetzung |
|---|---|
| Engine | Unity 6000.6.6f1, URP, Input System, TextMeshPro |
| Code | 322 C#-Dateien, rund 25 000 Zeilen, Namespace `PlanErfuellt`, ein Script pro Klasse |
| Daten | Alle Spielwerte in `Balancing.json`, alle Texte in `Texte_de.json`; ein Importer erzeugt daraus das `GameConfig`-Asset (89 Datenklassen, per `Tools/gen_config.py` erzeugt) |
| Architektur | Reine Logikklassen ohne Unity-Abhängigkeit (z. B. `Formula`, `DayLogic`, `InspectionLogic`, `HintLogic`, `WarningLogic`) + dünne MonoBehaviours für Darstellung; Kommunikation über `EventBus` |
| Szene | Labor, Stationen, HUD und Panels werden von Editor-Skripten gebaut (Menü *Plan erfüllt > Labor-Blockout bauen*), damit Änderungen reproduzierbar sind |
| Grafik | Eigene Modelle aus Grundformen und Drehkörpern, Texturen von Poly Haven (CC0) und selbst erzeugt, gebackenes Licht, Post-Processing |
| Ton | Alle Klänge selbst erzeugt (`Tools/gen_audio.py`) |
| Speichern | JSON-Spielstand zu Schichtbeginn, Einstellungen und Tastenbelegung in den PlayerPrefs |

## 4. Qualitätssicherung

- **179 EditMode-Tests** (NUnit) für Formeln, Tagesablauf, Kontrolle, Bestellung, Maschinen, Spielstand, Einführung, Warnungen und Tastenbelegung.
- **Play-Tests** nach jeder Änderung mit Screenshots (`Docs/Screenshots/`).
- **Balancing-Messung:** ein Autopilot spielt das Spiel über die echten Systeme im Zeitraffer. Werkzeug *Balancing-Serie* spielt viele Varianten nacheinander (z. B. alle 27 Dosen 0–2 je Stoff, Melkpumpen, Kühe durchschleusen) und schreibt Kennzahlen je Schicht (`Docs/Balancing/*.csv`). Daraus entstanden die Anpassungen D132, D137, D138.
- **Spieltests mit Personen:** Rückmeldungen führten zu Einsatzbesprechung in Seiten, Auftragskarte, Hinweisen beim ersten Auftreten, Pflicht-Mischung und -Bestellung in der Einführung, Hebel-Symbolen, Warnmeldungen und der Kommissar-Ankündigung (D133–D144).
- **Leistung:** Ziel 60 FPS; gemessen rund 300 FPS bei 1600 × 900 (FPS-Protokoll `-fpslog`).

## 5. Bekannte Grenzen

- Nur Deutsch.
- Texte der Einführung nennen die Standardtasten, auch wenn man Tasten neu belegt (Prompt und Auftragskarte zeigen die aktuelle Taste).
- Tiere und Figuren sind bewusst einfache, stilisierte Modelle.

## 6. Quellen

Siehe `Credits.md` (Schriften OFL, Poly-Haven-Texturen CC0, Unity-Pakete). Direktor und Kommissar sind erfundene Figuren.
