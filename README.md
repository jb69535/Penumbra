# Penumbra

Penumbra is a Unity 3D/URP project configured for a Hollow Knight-style 2.5D Metroidvania. Gameplay uses 2D physics and side-scroller controls; scenes use an orthographic camera, sprite planes, Z-depth, parallax, foreground masks, and optional 3D set dressing.

## Required Unity Version

- Unity `6000.4.10f1`
- Render pipeline: Universal Render Pipeline (URP), using the regular Universal Renderer by default

Use this exact Unity editor version on both macOS and Windows. Different patch versions can rewrite project files and create avoidable merge conflicts.

## Setup

1. Install Unity Hub.
2. Install Unity `6000.4.10f1`.
3. Install Git and Git LFS.
4. Clone the repository.
5. Run `git lfs install` once on your machine.
6. Open the project folder from Unity Hub.
7. Let Unity import the project before making edits.

## Project Map

- Start in Unity at `Assets/Penumbra/00_StartHere`.
- Open `Assets/Penumbra/Scenes/Prototype_Cave.unity` for the playable vertical-slice prototype.
- Game scenes live in `Assets/Penumbra/Scenes`.
- Gameplay code lives in `Assets/Penumbra/Scripts`.
- Reusable scene objects live in `Assets/Penumbra/Prefabs`.
- Tunable gameplay data lives in `Assets/Penumbra/Data`.
- Designer-owned visual and audio assets live in `Assets/Penumbra/Art` and `Assets/Penumbra/Audio`.
- Unity render pipeline and project configuration assets stay in `Assets/Settings` and `ProjectSettings`.
- Use `Tools > Penumbra > Rebuild Prototype Vertical Slice` in Unity to regenerate the prototype scene, prefabs, placeholder sprites, and tuning data after script changes.

For the full folder guide, read [Project Structure](Docs/ProjectStructure.md).

## Collaboration Rules

- Commit `Assets/`, `Packages/`, `ProjectSettings/`, `Docs/`, `.gitignore`, `.gitattributes`, and `README.md`.
- Do not commit generated folders such as `Library/`, `Logs/`, `Temp/`, `Obj/`, `Build/`, `Builds/`, or `UserSettings/`.
- Always commit `.meta` files with their matching asset files.
- Avoid editing the same Unity scene at the same time. Split work into prefabs, ScriptableObjects, and separate scenes when possible.
- Before starting work, pull the latest changes.
- Before pushing, close Unity or let it finish importing so generated project state is stable.

## Current Design Docs

- [Game Design Document](Docs/GameDesignDocument.md)
- [3D/2.5D Technical Setup](Docs/TechnicalSetup_2_5D.md)
