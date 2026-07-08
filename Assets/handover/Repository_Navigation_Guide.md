# Repository Navigation Guide

## Structure Overview
* **Repository structure:** Standard Unity 3D project structure managed via GitHub.
* **Important folders:** * `Assets/Scripts/` (Core logic)
  * `Assets/Prefabs/Medieval_Units/` (Characters)
  * `Assets/Scenes/` (Game arenas)
* **Critical scenes:** `TutorialScene.unity`, `MainBattleArena.unity`
* **Important assets:** Elixir UI sprites, VR Controller mappings.

## Requirements
* **Dependencies:** Unity VR Interaction Toolkit, NavMesh Components.
* **Package requirements:** Oculus XR Plugin (latest stable).
* **Build requirements:** Android Build Support (Quest).

## Major File Directory

**Tutorial Controller**
* **Path:** `Assets/Scripts/Tutorial/TutorialManager.cs`
* **Purpose:** Manages the flow of the VR tutorial, HUD triggers, and initial elixir logic.
* **Owner:** Yuvraj (Transitioning to Yashashri)
* **Can Modify?:** Yes

**Elixir Manager**
* **Path:** `Assets/Scripts/Economy/ElixirManager.cs`
* **Purpose:** Handles the generation, capping, and spending of elixir resources.
* **Owner:** Yuvraj (Transitioning to Yaseen)
* **Can Modify?:** Yes, but requires strict testing.

**Karma Trace Tracker**
* **Path:** `Assets/Scripts/Karma/KarmaTracker.cs`
* **Purpose:** Logs behavioral traces of units for end-of-match analysis.
* **Owner:** Yuvraj (Transitioning to Yashashri)
* **Can Modify?:** Yes