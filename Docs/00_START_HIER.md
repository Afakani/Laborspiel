# Plan erfüllt – Übergabe an Claude Desktop (Unity)

## Inhalt dieses Pakets
| Datei | Zweck |
|---|---|
| 00_START_HIER.md | Diese Anleitung + Projekt-Anweisung zum Einfügen |
| 01_GDD_Plan_erfuellt.md | Vollständiges Game Design Document (Regeln, Formeln, Layout, Ablauf) |
| 02_Balancing.json | Alle Spielwerte (wird in Unity zu ScriptableObjects) |
| 03_Texte_de.json | Alle Spieltexte (Direktor, Nachrichten, Enden, Toasts …) |
| 04_Art_Direction.md | Grafikstil, Farben, Licht, UI, Tiere |
| 05_Unity_Architektur.md | Ordner, Scripts, Prinzipien, Testwerte |
| 06_Prototyp_Referenz.html | Funktionierender Browser-Prototyp als Referenz (im Browser öffnen; mit `#debug` am Ende der URL gibt es Debug-Funktionen unter `window.__dbg`) |
| 07_Referenz_Uebersicht.png / 08_Referenz_Pult_Kammer.png | Stil-Referenzbilder |
| 09_Prompts.md | Die Prompts in Reihenfolge |

## Schritte
1. **Unity-Projekt anlegen** (Unity 6 LTS, Vorlage „Universal 3D“). Unity-Verbindung zu Claude Desktop aktiv lassen, Editor offen.
2. **In Claude Desktop ein neues Projekt anlegen** („Plan erfüllt Unity“) und **alle Dateien dieses Pakets** als Projektwissen hochladen.
3. **Projekt-Anweisung** (Custom Instructions des Projekts) – den Block unten einfügen.
4. Neuen Chat im Projekt starten und **Prompt 0** aus `09_Prompts.md` schicken. Fragen beantworten.
5. Danach Prompt 1, 2, … einzeln. Nach jeder Phase selbst kurz in Unity testen.
6. Für jede Phase gern einen neuen Chat (Kontext bleibt klein, Stand steht in `Docs/Fortschritt.md` im Unity-Projekt).

## Projekt-Anweisung (zum Einfügen)
```
Du baust mit mir das Spiel „Plan erfüllt“ in Unity nach. Projektwissen: GDD (01), Balancing (02), Texte (03), Art Direction (04), Architektur (05), Browser-Prototyp als Referenz (06), Stilbilder (07, 08), Prompts (09).

Regeln:
- Sprache mit mir: Deutsch, kurz, klar, ehrlich. Keine Emojis.
- Quelle der Wahrheit: GDD > Balancing-JSON > Prototyp-Code. Wenn etwas fehlt oder widersprüchlich ist: fragen, nicht erfinden. Eigene Entscheidungen in Docs/Entscheidungen.md festhalten und mir nennen.
- Arbeite direkt im Unity-Projekt über die verbundenen Unity-Tools. Nach jeder Änderung: kompilieren, Konsole auf Fehler prüfen, wenn möglich im Play Mode testen und Screenshot machen.
- Keine Spielwerte oder UI-Texte hart im Code – alles aus ScriptableObjects bzw. 03_Texte_de.json.
- Saubere Architektur nach 05_Unity_Architektur.md, ein Script pro Klasse, Namespace PlanErfuellt, kurze Kommentare auf Deutsch.
- Wenn du ein Script anlegst oder komplett überarbeitest: vollständige Datei schreiben, keine Fragmente.
- Performance-Ziel 60 FPS (GDD Abschnitt 15).
- Nur eine Phase pro Prompt. Am Ende jeder Phase: Docs/Fortschritt.md aktualisieren und kurz berichten (was fertig, was getestet, was offen, was ich prüfen soll).
- Fremd-Assets nur mit freier Lizenz und nach Rücksprache; in Docs/Credits.md eintragen.
```

## Hinweise
- **Umfang:** Der Prototyp hat viele Systeme. Die Prompts bauen zuerst den Kern (Phasen 1–6), die Zusatzsysteme kommen später. Wenn die Zeit knapp wird, Phasen 9–10 kürzen statt den Kern zu verwässern.
- **Prototyp-Funktionen** (für Abgleiche): `calcStab/calcQual/calcRate/calcMeat`, `comboInfo`, `startMix/finishMix`, `applySerum`, `updateSubject` (Risiken, Produktion, Darstellung), `sendToProc/finishProc`, `explode`, `infect/updateInfection`, `startDay/endDay/updateBoss`, `updateWish`, `deliver`, `startInspection` + Inspektionsblock in `updatePlay`, `zentriLoad/zentriStop/updateMachines`, `kryoLoad/kryoTake`, `ultCinematic`, `newsCopy/startNews`, `renderLog`, `machineTick`.
- **Referenzbild 08** zeigt einen fotorealistischen Direktor. Für das Spiel ist eine **stilisierte, erfundene Figur** vorgesehen (siehe Art Direction), keine echte Person.
