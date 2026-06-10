# Penumbra Game Design Document

Version: 0.1  
Date: 2026-06-10  
Engine: Unity  
Primary platform: PC prototype, with console support as a later target  
Team: Jun - design/programming; Wonjoon - graphic design/programming

## Source Notes

This GDD synthesizes the current Penumbra Notion project pages:

- Main project hub: https://app.notion.com/p/27efe708590d808e9317c26af64039e2
- Main game concept / GDD draft: https://app.notion.com/p/27efe708590d81ababc5dbfaca412553
- Story draft: https://app.notion.com/p/285fe708590d80c6935debe16024c89f
- Tasks database: https://app.notion.com/p/27efe708590d813f828fc0e0c06cf6e5
- GDD expansion task: https://app.notion.com/p/27ffe708590d80c3bcbeda16f5e161fd

The newer concept page is treated as canonical when sources conflict. In particular:

- Double jump is available from the start.
- The mirror is the Act I floor 2 key item.
- The core game is single-player. Multiplayer/co-op is documented as an optional stretch mode, not part of the core MVP.

## 1. High Concept

### Logline

Penumbra is a 2.5D philosophical action-adventure Metroidvania about a person who escapes the cave, sees the truth, and chooses to return to the darkness to rescue those left behind.

### One-Sentence Pitch

A shadow-bound wanderer explores a hostile cave world, switches between light and shadow states, solves mirror-based light puzzles, and makes choices that decide whether truth becomes salvation, isolation, or surrender.

### Genre

2.5D Metroidvania / philosophical action adventure.

### Core Fantasy

The player is not simply becoming stronger. They are learning how to carry truth without using it as a weapon. Every new movement ability, combat pattern, and light puzzle should feel like a step from fear toward understanding.

### Design Pillars

1. Truth has weight.
   - The game is about the burden of knowing. Light reveals, heals, and opens paths, but it can also overwhelm.

2. Darkness is not only evil.
   - Shadow represents fear, survival, memory, strategy, and acceptance. The dark state should be useful, tempting, and thematically valid.

3. Combat is readable and deliberate.
   - Encounters reward observation, timing, positioning, and state choice more than button mashing.

4. Exploration changes meaning on return.
   - The same cave is traversed twice: first as a prison to escape, then as a place of people to save.

5. Puzzles embody the theme.
   - Mirror puzzles are not decorative. They ask the player to redirect truth, block it, split it, or share it carefully.

## 2. Product Scope

### Target Experience

The full game is a compact Metroidvania built for a small indie team. The first playable should prove movement, combat, state switching, and one mirror puzzle loop before expanding into the full floor structure.

### Intended Session Length

- Prototype: 10-20 minutes.
- Vertical slice: 25-45 minutes.
- Full game: 3-6 hours, depending on exploration and endings.

### Production Target

The Notion draft lists a 4-6 month development period for a 1-4 person indie team. For the current Unity project, the recommended first milestone is a vertical slice covering the lower cave introduction through one boss and one mirror puzzle.

### Core Scope

- Single-player 2.5D side-scrolling exploration.
- Movement: run, jump, double jump, dash, wall jump, air dash, shadow movement.
- Combat: melee combo, dodge/dash, aerial strike, downward strike, lunge, boss pattern learning.
- State switching: light and shadow modes with different resources and skills.
- Mirror item: keyboard-aimed beam reflection for puzzles.
- Branching endings based on resources and player choices.
- Three-act narrative structure.

### Stretch Scope

- Console support.
- Optional co-op puzzle mode.
- Additional optional bosses.
- Expanded NPC rescue quests.
- Challenge rooms and time trials.

## 3. Player Experience

### Emotional Arc

The player begins confused and hunted. As they climb, they gain movement confidence and become increasingly aware that the cave is built from fear, memory, and false certainty. The surface gives relief but also guilt. The return journey should feel more morally complicated: enemies may be people, light may hurt them, and "saving" someone may require restraint.

### Tone

- Melancholic, tense, introspective.
- High contrast between oppressive underground spaces and the overwhelming surface.
- Philosophical, but communicated through action, level design, enemy behavior, and short dialogue rather than long exposition.

### References

- Hollow Knight: compact Metroidvania structure, boss readability, somber world.
- Inside: environmental storytelling and oppressive mood.
- Blasphemous: symbolic worldbuilding and religious dread.
- Celeste: precise movement and ability-based progression.

## 4. Core Gameplay Loop

### Macro Loop

1. Explore a cave floor.
2. Fight shadow creatures and learn their patterns.
3. Gain light or shadow resource based on state and actions.
4. Use state abilities to survive, heal, hide, or bypass hazards.
5. Find a key item, ability, memory, or mirror route.
6. Defeat a boss or solve a major puzzle gate.
7. Unlock new traversal options and revisit previous areas.
8. Make a choice that affects NPCs, resources, or ending conditions.

### Moment-to-Moment Loop

1. Read the room: enemy placement, light sources, blockers, platforms.
2. Choose state: light for recovery/revelation, shadow for stealth/control.
3. Move with precision: jump, double jump, dash, wall jump, or air dash.
4. Attack or evade.
5. Use K skill when the situation demands it.
6. Use mirror mode when light routing is needed.
7. Decide whether to destroy, spare, rescue, or ignore key figures.

## 5. Controls

Keyboard is the primary prototype input. Controller mapping should follow after the keyboard prototype feels good.

| Input | Action |
| --- | --- |
| A / D | Move left / right |
| W / S | Look or aim adjustment, contextual vertical input |
| Space | Jump; double jump is available by default |
| Shift | Dodge / dash |
| J | Basic attack; supports combo chain |
| S + J while airborne | Downward strike |
| Shift + W + Space + J | Upward strike / launcher input |
| A or D + Shift + J | Lunge attack |
| P | Toggle light / shadow state |
| K | State skill: light heal or shadow stealth |
| F | Interact, talk, pick up item, activate device |
| R | Mirror aim mode |
| A / D in mirror mode | Rotate mirror angle |
| W / S in mirror mode | Fine aim adjustment |
| 1-4 | Quick slots |
| M | Map |
| Esc | Pause menu |

### Control Goals

- Core movement should be responsive before combat complexity is added.
- Mirror aiming must work without mouse input.
- State switching should be instant enough to use under pressure, but not so free that every fight becomes spam switching.

## 6. Player Mechanics

### Movement

Baseline movement should support crisp platforming and combat spacing.

| Ability | Availability | Function | Design Purpose |
| --- | --- | --- | --- |
| Run | Start | Horizontal movement | Baseline navigation |
| Jump | Start | Ground jump | Core platforming |
| Double jump | Start | One extra air jump | Makes early movement expressive |
| Dash / dodge | Start | Short burst with invulnerability window | Combat survival and gap closing |
| Wall jump | Act I, floor 4 boss reward | Jump off valid walls | Opens vertical shafts |
| Air dash | Act I, floor 3 boss reward | Horizontal air burst | Opens wider gaps and aerial combat |
| Mirror | Act I, floor 2 boss reward | Reflect and redirect light beams | Opens puzzle gates and backtracking |
| Shadow movement | Act I, floor 1 boss reward | Pass through translucent barriers | Opens secret routes and late combat counters |

### Suggested Tuning Values

These values are starting points for prototype tuning, not final balance.

| Parameter | Starting Value |
| --- | --- |
| Player health | 100 |
| Contact damage | 10-15 |
| Standard enemy attack damage | 15-25 |
| Boss attack damage | 20-35 |
| Ground move speed | 6 units/sec |
| Jump height | 3.5-4 units |
| Dash distance | 3-4 units |
| Dash cooldown | 0.6 sec |
| Dodge invulnerability | 0.15-0.25 sec |
| K skill cooldown | 30 sec |

### Stamina

Stamina is recommended for advanced defensive and traversal actions, but should not block basic movement.

- Max stamina: 100.
- Dash/dodge cost: 20.
- Air dash cost: 20.
- Shadow movement cost: 30.
- Upward strike cost: 15.
- Regeneration delay after spending: 0.5 sec.
- Regeneration rate: 35/sec.

If stamina makes early combat feel too restrictive, ship the prototype without stamina and add it once enemy pressure increases.

## 7. Combat System

### Combat Identity

Combat is skill-based, readable, and symbolic. Enemies express internal states: fear, denial, false self, forgotten memory, corrupt certainty. The player should win by understanding patterns, not by grinding stats.

### Basic Attack

The J attack is a three-hit melee combo.

| Hit | Role | Notes |
| --- | --- | --- |
| Hit 1 | Fast opener | Low commitment, reliable range |
| Hit 2 | Follow-up | Slightly more damage |
| Hit 3 | Finisher | Highest damage, more recovery |

Rules:

- Combo resets if the player waits too long between inputs.
- Player can dash-cancel early hits, but not the finisher.
- Hitstop and sound should make strikes feel sharp without slowing the game too much.

### Special Attacks

| Attack | Input | Use |
| --- | --- | --- |
| Downward strike | S + J airborne | Breaks fragile floors, bounces off certain enemies, punishes low targets |
| Upward strike | Shift + W + Space + J | Anti-air, launcher, vertical boss punish |
| Lunge | A/D + Shift + J | Gap closer, pierces weak enemies, high stamina cost |

### Defense

- Dash/dodge gives a short invulnerability window.
- Bosses should telegraph attacks with pose, sound, and light/shadow effects.
- Blocking is not part of the core kit. Survival depends on movement, spacing, and state choice.

### Health and Recovery

- Health does not regenerate by default.
- Light state K skill heals a fixed amount or percentage.
- Healing should require a short vulnerable animation or commitment window.
- Rest points restore health and refill resources.

### Encounter Design Rules

- Never introduce a new enemy and a new environmental hazard at the exact same moment unless the room is intentionally a late-game test.
- Each floor should introduce one primary enemy behavior and then combine it with older behaviors.
- Bosses must test the ability or idea connected to their floor.

## 8. Light and Shadow System

### Overview

The player can switch between light and shadow states with P. The two states change resource gain, skill behavior, puzzle affordances, and narrative weighting.

### Shared Resource Model

Each state has an associated meter from 0-100.

- Defeating or resolving enemies in light state grants light resource.
- Defeating or bypassing enemies in shadow state grants shadow resource.
- Certain choices grant ending-affecting flags in addition to resource changes.
- Resource percentages are tracked across the full run for ending calculation.

### Light State

Theme: truth, healing, revelation, guidance.

Gameplay:

- Enemies defeated in light state grant light resource.
- Ambient light radius increases with current light resource.
- K skill heals the player.
- Mirror reflections suffer reduced falloff, giving longer beam range and stronger receiver activation.
- Hidden inscriptions, murals, or NPC faces can be revealed in light state.

Risks:

- Some shadow NPCs panic under direct light.
- Certain late-game hazards punish excessive light exposure.
- Light-heavy play may push the player toward the Guide ending, but only if paired with rescue actions.

### Shadow State

Theme: fear, strategy, acceptance, concealment.

Gameplay:

- Enemies defeated or bypassed in shadow state grant shadow resource.
- K skill activates stealth for a short duration.
- The player can temporarily activate shadow blockers in specific mirror puzzles.
- Shadow movement later allows passage through translucent barriers.
- Some enemies become less aggressive if approached in shadow.

Risks:

- Shadow-heavy play may reduce rescue opportunities.
- Darkness can obscure environmental clues.
- Certain bosses gain stronger pressure if the player hides too long.

### State Switching Rules

- P toggles instantly.
- Switching has a short visual/audio pulse so enemies, puzzles, and the player clearly register the change.
- Optional tuning: add a 0.25 sec lockout after switching to prevent spam.
- State choice should matter most in authored situations, not through constant statistical micromanagement.

## 9. Mirror System

### Role

The mirror is the central puzzle key item. It is not a primary weapon. Its role is to redirect, split, block, and interpret light.

### Acquisition

The mirror is awarded after defeating The Oblivioned One on Act I floor 2, the Maze of Oblivion.

### Basic Rules

- Press R to enter mirror aim mode.
- A/D rotates the mirror angle.
- W/S fine-tunes the angle.
- Releasing or pressing R again exits aim mode.
- While aiming, player movement is slowed or locked depending on puzzle difficulty.
- A beam hitting the mirror reflects according to the mirror angle.
- Beam intensity decays over distance unless the player is in light state.

### Puzzle Objects

| Object | Function |
| --- | --- |
| Fixed light source | Emits a beam at a set angle |
| Receiver door | Opens while receiving sufficient light |
| Relay mirror | Static reflector placed in the level |
| Prism | Splits one beam into multiple weaker beams |
| Timer receiver | Requires sustained light for a duration |
| Shadow blocker | Temporarily blocks a beam when activated in shadow state |
| Moving prism | Changes beam route over time or via interaction |
| Translucent barrier | Can be crossed after shadow movement is unlocked |

### Puzzle Progression

1. Floor 2 preview: fixed source to receiver, no mirror yet.
2. Floor 2 reward room: first player mirror reflection.
3. Floor 1: mirror plus relay plus translucent barrier.
4. Return floor 1: shadow blockers introduced.
5. Return floor 2: multi-receiver timer puzzle.
6. Return floor 3: prism and light meter management.
7. Return floor 4: mirror, shadow movement, and area unlock combined.

### Design Constraints

- Mirror puzzles must be readable on screen without needing a mouse.
- Beam lines should be bright and crisp against dark backgrounds.
- Receivers should communicate partial activation, full activation, and failure clearly.
- Every major mirror puzzle should be solvable in both keyboard and controller layouts.

## 10. Progression

### Ability Unlock Flow

| Order | Unlock | Location | Gated Content |
| --- | --- | --- | --- |
| 0 | Double jump | Start | Early vertical movement and expressive platforming |
| 1 | Wall jump | Act I floor 4 | Vertical shafts, fear-themed escape paths |
| 2 | Air dash | Act I floor 3 | Long gaps, aerial boss pressure, false platforms |
| 3 | Mirror | Act I floor 2 | Light receivers, reflective doors, memory routes |
| 4 | Shadow movement | Act I floor 1 | Translucent walls, hidden return paths, final gates |

### Growth Model

Penumbra should avoid heavy numerical RPG scaling. Growth is primarily ability-based, with small stat improvements to support pacing.

Recommended upgrades:

- Health vessel fragments.
- Stamina vessel fragments, if stamina ships.
- Light skill cooldown reduction.
- Shadow stealth duration increase.
- Mirror stability upgrades that reduce beam falloff.
- Map markers and memory fragments.

### Suggested Upgrade Curve

| Stage | Health | Stamina | Major Abilities | Notes |
| --- | --- | --- | --- | --- |
| Prologue | 100 | 100 | Double jump, dash | Teach survival |
| After floor 4 | 110 | 100 | Wall jump | Vertical routes open |
| After floor 3 | 120 | 110 | Air dash | Wider traversal spaces |
| After floor 2 | 130 | 110 | Mirror | Puzzle backtracking begins |
| After floor 1 | 140 | 120 | Shadow movement | Return route opens |
| Late Act III | 150-160 | 130 | Full kit | Combined mastery |

## 11. World Structure

### Overall Map

The world is a cave arranged as a symbolic ascent and return.

| Act | Direction | Purpose |
| --- | --- | --- |
| Prologue | Deep cave | Wake up, survive, learn baseline combat |
| Act I: The Ascent | Floor 4 to floor 1 | Escape the cave, unlock traversal kit |
| Act II: The Surface | Surface | Encounter truth and remember the people below |
| Act III: Return of Light | Floor 1 to floor 4 | Return to rescue or confront those left behind |
| Finale | Heart of Truth | Final choice and ending |

### Act I: The Ascent

| Floor | Name | Theme | Gameplay Focus | Boss | Reward |
| --- | --- | --- | --- | --- | --- |
| Floor 4 | Nameless Depths | Fear | Limited vision, vertical shafts, ambushes | Form of Fear | Wall jump |
| Floor 3 | Stage of Lies | Illusion | False paths, vanishing platforms, horizontal gaps | False Self | Air dash |
| Floor 2 | Maze of Oblivion | Memory | Looping rooms, light receiver previews, memory fragments | The Oblivioned One | Mirror |
| Floor 1 | Threshold of Escape | Certainty | Mirror routing, translucent walls, final ascent gate | King of Chains | Shadow movement |

### Act II: The Surface

The surface should be short but emotionally important.

Goals:

- Provide visual contrast: warm, bright, open.
- Remove or reduce combat temporarily.
- Let the player feel freedom and discomfort at the same time.
- Restore memory: this is not the first escape.
- Present the key moral turn: the player can leave, but others remain trapped.

### Act III: Return of Light

| Floor | Name | Theme | Gameplay Focus | Major Event |
| --- | --- | --- | --- | --- |
| Return floor 1 | Hall of Return | Self-hatred | Enemy groups, traps, shadow blockers | First enemies reveal human faces |
| Return floor 2 | Abyss of Betrayal | Past | Complex mirror puzzles, timers | Forgotten One confrontation and mercy choice |
| Return floor 3 | Crater of Truth | Madness | Light meter pressure, moving prisms | Truth can harm when forced |
| Return floor 4 | Sanctum of Shadows | Faith | Mirror plus shadow movement mastery | High Shadow boss |
| Final | Heart of Truth | Choice | Full-kit boss and narrative decision | Shadow Sovereign |

## 12. Narrative Design

### Theme

Penumbra adapts the emotional shape of Plato's cave allegory. The story is not just "light good, dark bad." It asks what happens when a person sees truth and then returns to people who may not want it, may fear it, or may be harmed by receiving it too violently.

### Protagonist

The protagonist wakes in the cave with no memory. They bear signs of old chains and a strange connection to both light and shadow. They eventually remember that they escaped before, reached the surface, and abandoned someone in the cave.

### Key Characters

| Character | Role | Function |
| --- | --- | --- |
| Protagonist | Player character | Embodies the choice to return |
| Cave people | Trapped inhabitants | Show fear, denial, and dependence on shadows |
| Forgotten One | Memory figure / boss | Represents abandonment and the cost of escape |
| High Shadow | Late-game ideological boss | Defends darkness as survival and identity |
| Shadow Sovereign | Final boss | Embodies the total human attachment to ignorance, pain, and certainty |

### Story Summary

Prologue: The player wakes in the deep cave, surrounded by people staring at shadows. The player does not remember who they are, but they feel that something above is real.

Act I: The player climbs through four cave floors, each representing a barrier to truth: fear, illusion, memory, and certainty. Each boss unlocks a movement or puzzle ability that also has symbolic meaning.

Act II: The player reaches the surface and sees the real world. The light is beautiful, but the victory feels incomplete. Memory returns: the player has escaped before, alone. This time they choose to return.

Act III: The player descends back into the cave, now seen differently. Some enemies are revealed as people. Some reject rescue. Some are harmed by forced light. The player confronts the Forgotten One and learns whether salvation is an act of guidance, abandonment, or acceptance.

Finale: The Shadow Sovereign challenges the player's belief that truth can save everyone. The final choice determines whether the player guides others out, leaves alone, or remains with the shadows.

### Dialogue Style

- Short, poetic, and direct.
- Dialogue should appear at emotional peaks, before bosses, after major choices, and near environmental discoveries.
- Avoid long philosophical explanations. Let level mechanics carry the argument.

Sample tone:

> "Fear is not the end of the path. It is the name of the first step."

> "Light forced into closed eyes is still violence."

## 13. Endings

Ending conditions should use both behavior and explicit choices. Resource percentage alone should not determine the ending.

### Ending Variables

- Light usage percentage.
- Shadow usage percentage.
- Rescue interactions completed.
- Mercy choices selected.
- Mirror puzzles completed.
- Whether the player lowers the weapon during the Forgotten One encounter.
- Final dialogue choice.

### Ending Table

| Ending | Conditions | Outcome |
| --- | --- | --- |
| Guide Ending | High light usage, many rescue choices, major mirror puzzles completed, mercy choice selected | The player leads cave people toward the surface |
| Wanderer Ending | Balanced resources, low rescue commitment, avoids ideological extremes | The player leaves alone and lives on the surface |
| Companion Ending | High shadow usage, few rescue actions, chooses to understand or join the shadows | The player remains in the cave with those who reject the light |

### Ending Design Goal

No ending should feel like a simple fail state. Each ending should express a different answer to the question: "What does someone owe to people still inside the cave?"

## 14. Enemies

### Standard Enemy Concepts

| Enemy | Theme | Behavior | Counterplay |
| --- | --- | --- | --- |
| Silhouette | Fear | Swarms and pressures edges of arenas | Spacing, dash, quick combo |
| False One | Illusion | Creates decoys and disrupts air space | Identify real body, use air dash |
| Form of Oblivion | Memory | Patrol loops and splits upper/lower routes | Learn route, use vertical movement |
| Denier | Doubt | Shields from the front | Attack from diagonal, lunge, mirror puzzle variants |
| Corrupted Light | Obsession | Mimics jump/dash patterns | Bait movement, punish recovery |

### Enemy Rules

- Early enemies should teach one idea each.
- Later enemies combine movement pressure with state-specific vulnerabilities.
- Some return-act enemies should briefly reveal human traits to support the narrative turn.

## 15. Bosses

### Boss Progression

| Boss | Location | Theme | Combat Concept | Reward |
| --- | --- | --- | --- | --- |
| Form of Fear | Act I floor 4 | Fear | Close pressure, walls and ceiling hazards | Wall jump |
| False Self | Act I floor 3 | Self-deception | Decoys, aerial movement, mirrored attacks | Air dash |
| The Oblivioned One | Act I floor 2 | Memory | Phase loops, route repetition, receiver foreshadowing | Mirror |
| King of Chains | Act I floor 1 | Bondage / certainty | Translucent walls, area denial, phase movement | Shadow movement |
| High Shadow | Return floor 4 | Faith / fanaticism | Light pillars, darkness veils, limited summons | Narrative gate |
| Shadow Sovereign | Finale | Human attachment to darkness | State-reactive AI and arena transformation | Ending choice |

### Boss Design Rules

- Every boss has three readable attack families: pressure, reposition, punish.
- Every boss should create at least one opening that rewards the newly taught ability.
- Boss dialogue should be brief and should frame the emotional conflict.
- Final boss should react to the player's dominant state and choices.

## 16. Level Design

### Room Types

| Room Type | Purpose |
| --- | --- |
| Tutorial room | Teach a mechanic safely |
| Combat chamber | Test enemy pattern recognition |
| Traversal room | Test movement ability |
| Puzzle gate | Test mirror or state logic |
| Memory room | Deliver story through environment |
| Rest room | Heal, checkpoint, update map |
| Boss arena | Test floor mastery |
| Return variant | Recontextualize a previous room |

### Metroidvania Backtracking

Backtracking should be meaningful but not tedious.

- Wall jump opens vertical routes in earlier floor 4 rooms.
- Air dash opens gaps and optional combat rooms on floors 4 and 3.
- Mirror opens sealed memory doors and receiver gates.
- Shadow movement opens hidden passages and shortcuts.
- Return act should reuse major spaces but alter enemy placement, lighting, and NPC interpretation.

### Checkpoints

- Rest points at the start and midpoint of each floor.
- Checkpoint before each boss.
- Return act should unlock shortcuts to reduce repeated traversal.

## 17. UI and UX

### HUD Elements

- Health bar.
- Light meter.
- Shadow meter.
- Current state icon or color treatment.
- Skill cooldown indicator.
- Stamina bar, if stamina ships.
- Mirror angle indicator during R aim mode.
- Context prompt for F interactions.

### Menus

- Pause menu.
- Map screen.
- Controls screen.
- Inventory / key items.
- Memory fragments.
- Settings: audio, display, input remapping, accessibility.

### Map

The map should show:

- Current floor.
- Visited rooms.
- Locked doors.
- Mirror receivers.
- Ability-gated paths.
- Rest points.
- Major unresolved memory rooms.

## 18. Art Direction

### Visual Identity

Penumbra uses high-contrast silhouette art with strong light/shadow composition. The player, enemies, and cave architecture should read clearly even in dark scenes.

### Palette Direction

- Underground: black, deep blue, desaturated violet, cold gray.
- Surface: white, pale gold, warm green, sky tones.
- Light mechanics: gold, white, sharp beam lines.
- Shadow mechanics: dark violet, smoky gray, soft edge distortion.
- Danger: muted red or hot white, used sparingly.

### Environment Motifs

- Chains.
- Cave walls and shadow screens.
- Murals and half-visible inscriptions.
- Mirrors and fractured glass.
- Receiver doors with sun/cave symbols.
- Human faces hidden in shadow silhouettes.

### Animation Priorities

1. Player locomotion and combat readability.
2. State switch transformation.
3. Mirror aiming and beam reflection.
4. Boss telegraphs.
5. Enemy hit reactions.

## 19. Audio Direction

### Music

- Minimal, atmospheric underground score.
- Strong contrast for surface sequence: warmer, wider, more harmonic.
- Return act should distort earlier motifs.
- Boss music should incorporate the floor theme.

### Sound Effects

- Light state: clear, resonant, glass-like tones.
- Shadow state: muffled, low, breath-like textures.
- Mirror beam: focused hum with pitch change on alignment.
- Receivers: rising tone as activation builds.
- State switch: short pulse, distinct for each direction.

### Voice / Dialogue

Full voice acting is not required for MVP. Text dialogue with sound motifs or vocal textures is enough.

## 20. Technical Design Notes

### Unity Project Direction

The current project is a Unity 2D/URP setup. The game should use a 2D gameplay foundation with 2.5D presentation where useful.

Recommended systems:

- Player controller.
- Ability unlock manager.
- Combat hitbox/hurtbox system.
- Enemy state machines.
- Boss state machines.
- Light/shadow state manager.
- Resource and ending flag tracker.
- Mirror beam simulation.
- Interactable system.
- Save/checkpoint system.
- Room/scene loading.
- Map discovery system.

### Mirror Implementation Approach

For the prototype:

- Use 2D raycasts for beam direction.
- Reflect ray direction using surface normals or mirror angle.
- Limit reflection count to a small number, such as 3-5.
- Draw beams with LineRenderer or custom 2D sprites.
- Receivers detect beam contact and intensity.
- Add debug gizmos for beam path and receiver threshold.

### Data Approach

Use ScriptableObjects for:

- Enemy stats.
- Boss phase data.
- Ability definitions.
- Upgrade definitions.
- Dialogue snippets.
- Room metadata.
- Ending thresholds.

### Save Data

Minimum save data:

- Current scene/room.
- Player health and resources.
- Unlocked abilities.
- Defeated bosses.
- Opened gates.
- Collected memory fragments.
- Ending variables.

## 21. Optional Co-op / Multiplayer Stretch

The core game is single-player. If multiplayer is pursued later, it should be designed as a separate mode or carefully scoped co-op variant.

### Co-op Concept

Two players represent light and shadow aspects. One can stabilize beams and reveal truth; the other can block beams, pass through shadow routes, or calm hostile silhouettes.

### Basic Rules

- Local co-op first; online networking only if the local version proves fun.
- Both players share story progress.
- Each player has distinct puzzle responsibilities.
- Combat scaling adds enemy behaviors, not just more health.
- If one player falls, the other can revive at a cost.

### Co-op Puzzle Examples

- Player A holds a mirror beam on a receiver while Player B activates a shadow blocker to prevent the beam from hitting a harmful target.
- Player A reveals a hidden platform with light while Player B crosses through a shadow barrier.
- Both players stand at separate memory altars to align a prism route.

### Network Risks

Online multiplayer would add significant complexity: synchronization, latency-sensitive combat, mirrored beam state, save consistency, and QA burden. It should not be included in the first production milestone.

## 22. Prototype Plan

### MVP Prototype

Goal: prove the game feels good at the smallest scale.

Required:

- Player run/jump/double jump.
- Dash/dodge.
- Basic attack combo.
- Light/shadow toggle.
- Light heal and shadow stealth placeholders.
- One enemy type.
- One small traversal room.
- One combat room.
- One mirror puzzle room.
- One checkpoint.

### Vertical Slice

Goal: prove the full Penumbra promise.

Required:

- Floor 4 or floor 2 slice with complete art direction.
- Two enemy types.
- One miniboss or boss.
- One ability unlock.
- One mirror puzzle with receiver feedback.
- One memory room.
- One short dialogue sequence.
- Basic HUD and pause menu.
- Save/checkpoint loop.

### Full Production Milestones

| Milestone | Target |
| --- | --- |
| M1: Controller and combat foundation | Player movement, combat, one enemy |
| M2: State system | Light/shadow resources, skills, HUD |
| M3: Mirror prototype | Beam reflection, receiver doors, one puzzle |
| M4: First floor slice | Floor layout, boss, reward, checkpoint |
| M5: Act I complete | Four floors, four bosses, major unlocks |
| M6: Act II complete | Surface scene, memory reveal |
| M7: Act III complete | Return floors, advanced puzzles, High Shadow |
| M8: Finale and endings | Final boss, ending logic, credits |
| M9: Polish and QA | Balance, accessibility, bug fixing, performance |

## 23. Accessibility

Minimum accessibility considerations:

- Rebindable controls.
- Controller support.
- Adjustable screen shake.
- Subtitles / text speed options.
- High-contrast beam visibility option.
- Assist option for mirror aim sensitivity.
- Optional longer puzzle timers.
- Clear checkpointing before bosses.

## 24. Risks and Mitigations

| Risk | Impact | Mitigation |
| --- | --- | --- |
| Mirror puzzles become hard to read | Players feel stuck for the wrong reason | Strong beam visuals, receiver feedback, limited reflection count |
| State system becomes too abstract | Light/shadow feels cosmetic | Tie state to skills, resources, NPC reactions, puzzles |
| Scope too large for small team | Production stalls | Build MVP first, then vertical slice, then Act I |
| Story overwhelms gameplay | Pacing slows | Keep dialogue short and deliver theme through mechanics |
| Backtracking feels repetitive | Metroidvania fatigue | Add shortcuts, changed enemy layouts, return-act reinterpretation |
| Multiplayer scope expands too early | Core game suffers | Treat co-op as stretch, not MVP |

## 25. Open Questions

1. Should stamina ship in the first prototype, or should it wait until combat and movement feel good?
2. Is the final game intended to be jam-sized, a semester project, or a commercial indie release?
3. Should dialogue be English, Korean, or bilingual?
4. Should the visual target be pure 2D silhouettes or 2.5D characters in a 2D plane?
5. How many endings are required for the first complete build?
6. Should the player ever kill cave people, or should "defeat" mean cleanse, calm, or disperse?
7. Is the mirror ever allowed to damage enemies, or should it remain strictly puzzle-focused?

## 26. Immediate Next Steps

1. Build the MVP controller: run, jump, double jump, dash.
2. Add the light/shadow state manager and HUD placeholder.
3. Implement one enemy with readable patrol, attack, hit, and death states.
4. Add J combo and dash/dodge invulnerability.
5. Prototype mirror raycast reflection in a graybox room.
6. Create one receiver door and one fixed light source.
7. Make a tiny playable loop: enter room, fight, solve mirror puzzle, unlock exit.
8. Review feel before expanding lore, floors, or boss content.

