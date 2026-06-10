# Penumbra Project Structure

This project is organized so developers and designers can find the right place by clicking through a small number of stable folders.

## Where To Start

- `README.md`: setup, Unity version, and collaboration rules.
- `Docs/GameDesignDocument.md`: current design direction and gameplay requirements.
- `Assets/Penumbra/00_StartHere`: Unity-side starting point for teammates.
- `Assets/Penumbra/Scenes/Prototype_Cave.unity`: current playable prototype scene.

## Assets Folder

### `Assets/Penumbra`

All game-specific work should live here. If it belongs to Penumbra rather than Unity configuration, put it under this folder.

### `Assets/Penumbra/Scenes`

Use this for Unity scenes. Keep prototype, test, and production scenes clearly named.

Suggested names:

- `Prototype_Cave.unity`
- `CombatTest_Player.unity`
- `PuzzleTest_Mirror.unity`
- `Floor01_LowerCave.unity`

### `Assets/Penumbra/Scripts`

Use this for C# gameplay and editor code. Create subfolders only when there is real code to put in them.

Suggested subfolders:

- `Core`
- `Input`
- `Player`
- `Combat`
- `World`
- `Puzzles`
- `UI`
- `Editor`

### `Assets/Penumbra/Prefabs`

Use this for reusable scene objects such as the player, enemies, pickups, doors, mirrors, hazards, UI widgets, and cameras.

Suggested subfolders:

- `Player`
- `Enemies`
- `Interactables`
- `Puzzles`
- `UI`

### `Assets/Penumbra/Data`

Use this for ScriptableObjects and other editable gameplay data. Designers should be able to tune values here without hunting through scripts.

Suggested subfolders:

- `Abilities`
- `Enemies`
- `Items`
- `Dialogue`
- `Tuning`

### `Assets/Penumbra/Art`

Use this for source and imported visual assets.

Suggested subfolders:

- `Characters`
- `Environments`
- `Tilesets`
- `Animations`
- `Materials`
- `VFX`

### `Assets/Penumbra/Audio`

Use this for music, sound effects, ambience, and Unity audio mixers.

Suggested subfolders:

- `Music`
- `SFX`
- `Ambience`
- `Mixers`

### `Assets/Penumbra/UI`

Use this for UI-specific assets, prefabs, fonts, icons, and menu scenes.

### `Assets/Penumbra/Tests`

Use this for Unity edit-mode and play-mode tests once systems start becoming stable.

### `Assets/Penumbra/ThirdParty`

Use this for imported third-party assets that are not managed through Unity Package Manager. Add a short license note inside any imported asset folder.

### `Assets/Settings`

Keep Unity render pipeline, renderer, and template settings here. Most teammates should not edit this casually.

## Generated Folders

These are local Unity/editor output and should not be committed:

- `Library`
- `Logs`
- `Temp`
- `Obj`
- `Build`
- `Builds`
- `UserSettings`

If Unity behaves strangely, close Unity and delete `Library`; Unity will rebuild it on the next open.

## Rules Of Thumb

- Create folders when there is real content to place there.
- Keep `.meta` files with their matching assets.
- Prefer prefabs and ScriptableObjects over putting everything directly into one scene.
- Keep scenes small and purpose-driven to reduce merge conflicts.
- Do not place game assets directly under `Assets` unless they are project-wide Unity configuration assets.
