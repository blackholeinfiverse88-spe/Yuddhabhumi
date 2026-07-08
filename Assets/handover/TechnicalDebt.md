# Technical Debt Register

## Known Bugs
* **[High]** NavMesh agents occasionally get stuck on the bridge colliders in `MainBattleArena.unity`.
* **[Medium]** Elixir bar UI sometimes desyncs by 0.5 seconds from the actual backend value.

## Known Hacks / Temporary Solutions
* **[Critical]** Units are currently instantiated and destroyed instead of using an Object Pool. This *will* crash the Quest build during heavy combat. 
* **[Medium]** GitHub `.gitignore` was modified late; some local temp files might still be tracked and need to be purged.

## Incomplete / Prototype-Only Systems
* **[High]** The Replay system only logs timestamps; it does not accurately reproduce physics interactions yet.

## System Risks
* **Performance Risks:** **[Critical]** Draw calls spike when multiple archer projectiles are active.
* **Replay Risks:** **[High]** Data size of trace logs grows exponentially in matches longer than 5 minutes.
* **VR Risks:** **[Medium]** HUD elements lack a follow-smoothing script, causing minor discomfort.