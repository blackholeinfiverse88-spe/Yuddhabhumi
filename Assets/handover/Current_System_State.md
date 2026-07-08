# Current System State

## Gameplay
* **What works today:** The tutorial scene is fully isolated and functional. VR navigation, HUD interactions, and base elixir resource management are stable.
* **What partially works:** Medieval unit deployment (knights/archers) works but occasionally struggles with navmesh pathing and targeting priority.
* **What is missing:** Complete late-game win/loss state resolution and network synchronization for real-time multiplayer.

## Karma
* **Current implementation:** Tracks basic behavior traces and event histories for deployed units.
* **Limitations:** The trace log is heavy on memory and needs optimization for longer matches.
* **Future roadmap:** Integrating Karma traces directly into the Replay system for post-match analysis.

## Replay
* **Current implementation:** Basic state capturing of unit spawn times and locations.
* **Missing pieces:** Full 3D playback of interactions, projectile tracking, and VR camera repositioning during playback.
* **Risks:** High potential for desync between the live match and the recorded replay data.

## Characters
* **Current state:** Core medieval models (knights, archers) are imported. T-pose rigs are set up, and basic animations are mapped to the animator controllers.
* **Missing state:** Advanced attack variations and death animations are still utilizing placeholder assets.
* **Future direction:** Finalizing custom 3D models and replacing AI-generated concept art with final production textures.

## VR
* **Current state:** Head tracking, controller mapping, and primary UI pointers are functional.
* **Known issues:** Minor jitter on the HUD overlay when looking at steep angles.
* **Launch blockers:** Frame drops when too many units are instantiated simultaneously; requires aggressive object pooling.

## Deployment
* **Current state:** Local builds can be compiled via Unity for testing.
* **Readiness level:** Pre-alpha.
* **Missing requirements:** Automated CI/CD pipeline, final Meta Quest manifest configurations, and keystore finalization.