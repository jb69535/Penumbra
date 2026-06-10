# Penumbra

Penumbra is a Unity 2D/URP project for a philosophical action-adventure Metroidvania.

## Required Unity Version

- Unity `6000.4.10f1`
- Render pipeline: Universal Render Pipeline (URP)

Use this exact Unity editor version on both macOS and Windows. Different patch versions can rewrite project files and create avoidable merge conflicts.

## Setup

1. Install Unity Hub.
2. Install Unity `6000.4.10f1`.
3. Install Git and Git LFS.
4. Clone the repository.
5. Run `git lfs install` once on your machine.
6. Open the project folder from Unity Hub.
7. Let Unity import the project before making edits.

## Collaboration Rules

- Commit `Assets/`, `Packages/`, `ProjectSettings/`, `Docs/`, `.gitignore`, `.gitattributes`, and `README.md`.
- Do not commit generated folders such as `Library/`, `Logs/`, `Temp/`, `Obj/`, `Build/`, `Builds/`, or `UserSettings/`.
- Always commit `.meta` files with their matching asset files.
- Avoid editing the same Unity scene at the same time. Split work into prefabs, ScriptableObjects, and separate scenes when possible.
- Before starting work, pull the latest changes.
- Before pushing, close Unity or let it finish importing so generated project state is stable.

## Current Design Docs

- [Game Design Document](Docs/GameDesignDocument.md)

