# Penumbra Game Design Document

Version: 0.2 - Chain and Mirror Revision  
Date: 2026-07-08  
Engine: Unity  
Primary platform: PC prototype, with console support as a later target  
Team: Jun - design/programming; Wonjoon - graphic design/programming

## Source Notes

This document is a new working GDD based on:

- `Docs/GameDesignDocument.md` version 0.1, which synthesizes the current Notion project pages.
- The newer concept direction discussed after version 0.1:
  - Floor 4 begins with the player bound to and attacking with a chain.
  - The chain breaks after clearing the main boss of Floor 4.
  - Floor 3 is higher than Floor 4, so a small amount of outside light reaches it.
  - The player first learns mirror use on Floor 3.
  - The cave people believe mirror-focused light is alchemy, or `연금술`, even though it is actually outside light being collected, redirected, and concentrated.

Version 0.1 remains useful as a reference. Version 0.2 should be treated as the current narrative and progression direction for the next design pass.

## 1. High Concept

### Logline

Penumbra is a 2.5D philosophical action-adventure Metroidvania about a chain-bound wanderer who escapes the cave, learns that light can reveal and harm, and chooses whether to return for the people still trapped below.

### One-Sentence Pitch

A forgotten prisoner fights with the chain that binds them, breaks free from fear, learns to focus stolen sunlight through a mirror, and decides whether truth should be given, hidden, or carried gently.

### Genre

2.5D Metroidvania / philosophical action adventure.

### Core Fantasy

The player is not simply becoming stronger. They are learning how to transform their relationship to truth:

- First, they survive with the chain that imprisons them.
- Then, they break the chain and become physically free.
- Then, they learn to redirect light through a mirror.
- Finally, they learn that truth, like focused light, can open paths or burn people who are not ready.

### Design Pillars

1. Truth has weight.
   - Light reveals, heals, and opens paths, but it can also overwhelm.

2. Darkness is not only evil.
   - Shadow represents fear, survival, memory, strategy, and acceptance.

3. The tool changes the question.
   - The chain asks: "How do I survive while bound?"
   - The mirror asks: "How do I direct what I did not create?"

4. Exploration changes meaning on return.
   - The same cave is traversed twice: first as a prison to escape, then as a place of people to save or understand.

5. Puzzles embody the theme.
   - Mirror puzzles are not decorative. They ask the player to redirect truth, block it, split it, soften it, or focus it.

## 2. Product Scope

### Target Experience

The full game is a compact Metroidvania built for a small indie team. The first playable should prove:

- Chain combat in the dark.
- Chain break and movement liberation.
- First mirror use with small light sources.
- One combat room, one traversal room, and one mirror path-breaking room.

### Intended Session Length

- Prototype: 10-20 minutes.
- Vertical slice: 25-45 minutes.
- Full game: 3-6 hours, depending on exploration and endings.

### Core Scope

- Single-player 2.5D side-scrolling exploration.
- Movement: run, jump, double jump, dash, wall jump, air dash, shadow movement.
- Early combat: chain attack while bound.
- Later combat: short melee plus mirror-assisted light attacks.
- State switching: light and shadow modes with different resources and skills.
- Mirror item: keyboard-aimed beam reflection and focused light for puzzles and limited combat.
- Branching endings based on resource use, rescue choices, mercy, and final dialogue.
- Three-act narrative structure.

### Stretch Scope

- Console support.
- Optional co-op puzzle mode.
- Additional optional bosses.
- Expanded NPC rescue quests.
- Challenge rooms and time trials.

## 3. Player Experience

### Emotional Arc

The player begins confused, chained, and hunted. Floor 4 should feel heavy and desperate: the player survives by weaponizing the thing that binds them. After the Floor 4 boss, the chain breaks. The player is lighter, faster, and more exposed.

Floor 3 introduces thin outside light. The player learns to use a broken shackle mirror to focus that light. Cave people call this `연금술`, believing the player creates fire from darkness. The truth is simpler and more dangerous: the player is only redirecting light that already exists.

The surface gives relief but also guilt. The return journey should feel morally complicated. Some enemies may be people. Some people may fear rescue. Some NPCs can be harmed if the player forces too much light onto them.

### Tone

- Melancholic, tense, introspective.
- Oppressive underground spaces contrasted with overwhelming surface light.
- Philosophical, but delivered through action, enemy behavior, short dialogue, and level design.

### References

- Hollow Knight: compact Metroidvania structure, boss readability, somber world.
- Inside: environmental storytelling and oppressive mood.
- Blasphemous: symbolic worldbuilding and religious dread.
- Celeste: precise movement and ability-based progression.
- Fullmetal Alchemist, as inspiration only: taboo knowledge, cost, and the misunderstanding of transmutation.
- Plato's cave allegory: shadow as accepted reality.
- The Matrix, as inspiration only: artificial certainty and the pain of waking.

## 4. Core Gameplay Loop

### Macro Loop

1. Explore a cave floor.
2. Fight enemies that express the floor's inner state.
3. Find a new tool, ability, memory, or route.
4. Use state abilities, chain movement, or mirror light to survive.
5. Clear a mid boss or major gate.
6. Defeat the main boss of the floor.
7. Unlock new traversal or mirror options.
8. Revisit previous areas with changed meaning.
9. Make choices that affect NPCs, resources, and endings.

### Moment-to-Moment Loop

1. Read the room: enemy placement, light sources, shadows, blockers, breakable paths.
2. Choose state: light for recovery/revelation, shadow for stealth/control.
3. Move with precision: jump, double jump, dash, wall jump, or air dash.
4. Attack with chain, melee, or mirror-focused light depending on progression.
5. Use mirror mode when light routing or focused burning is needed.
6. Decide whether to destroy, bypass, calm, reveal, rescue, or ignore key figures.

## 5. Controls

Keyboard is the primary prototype input. Controller mapping should follow after the keyboard prototype feels good.

| Input | Action |
| --- | --- |
| A / D | Move left / right |
| W / S | Look or aim adjustment, contextual vertical input |
| Space | Jump; double jump is available by default |
| Shift | Dodge / dash |
| J | Basic attack |
| J during Floor 4 | Chain attack |
| S + J while airborne | Downward strike |
| Shift + W + Space + J | Upward strike / launcher input |
| A or D + Shift + J | Lunge attack |
| P | Toggle light / shadow state |
| K | State skill: light heal or shadow stealth |
| F | Interact, talk, pick up item, activate device |
| R | Mirror aim mode after mirror is acquired |
| A / D in mirror mode | Rotate mirror angle |
| W / S in mirror mode | Fine aim adjustment |
| J in mirror mode | Focus beam pulse if usable light is available |
| 1-4 | Quick slots |
| M | Map |
| Esc | Pause menu |

### Control Goals

- Chain combat should feel readable, weighty, and slightly restrictive.
- Post-chain movement should immediately feel freer.
- Mirror aiming must work without mouse input.
- Mirror attack should not become a universal gun. It requires light, angle, and commitment.
- State switching should be useful under pressure but not become spam.

## 6. Player Mechanics

### Ability Unlock Flow

| Order | Unlock | Location | Function | Design Purpose |
| --- | --- | --- | --- | --- |
| 0 | Run, jump, double jump, dash | Start | Baseline movement | Make early movement expressive |
| 1 | Prison chain | Start / Floor 4 | Primary early weapon | Survival through bondage |
| 2 | Chain break / unbound movement | Floor 4 main boss | Chain is lost, movement becomes lighter | Liberation after fear |
| 3 | Wall jump | Floor 4 main boss reward | Jump off valid walls | Opens vertical shafts |
| 4 | Broken Shackle Mirror | Early Floor 3 | Reflect and focus small light sources | First contact with "alchemy" |
| 5 | Air dash | Floor 3 main boss reward | Horizontal air burst | Opens wider gaps and aerial fights |
| 6 | Prism lens | Floor 2 main boss reward | Split or stabilize beams | Memory routes and advanced mirror puzzles |
| 7 | Shadow movement | Floor 1 main boss reward | Pass through translucent barriers | Final ascent gate and return routes |

### Chain System

The player begins Floor 4 with a chain still attached to their body. It is both weapon and prison.

| Chain Action | Use | Feel |
| --- | --- | --- |
| Chain strike | Basic mid-range attack | Heavy, sweeping, slightly delayed |
| Chain drag | Pull small enemies or objects | Survival tool, not full grappling hook |
| Chain slam | Downward or committed attack | Good against low enemies and brittle floors |
| Chain anchor | Briefly hold against pushback or wind | Teaches weight and restraint |

Rules:

- Chain attacks have more reach than later melee, but more recovery.
- Some enemies can bite, grab, or pull the chain.
- The chain can hit environmental bells, weak stone, and hanging locks.
- The chain should visually remind the player they are not free yet.

After the Floor 4 main boss, the chain breaks. The player loses long chain attacks. The broken shackle remains as a narrative object and becomes the basis of the Floor 3 mirror.

### Post-Chain Melee

After the chain breaks, the player should still have a basic attack.

| Action | Use | Design Purpose |
| --- | --- | --- |
| Broken-link slash | Shorter, faster melee | Keeps combat functional after chain loss |
| Downward strike | Break floors, pogo certain enemies | Maintains Metroidvania combat language |
| Lunge | Gap close and punish | Costs stamina if stamina ships |

The loss of the chain should feel scary at first, then freeing. Floor 3 should quickly replace the missing reach with mirror-based problem solving, not a simple stronger weapon.

### Mirror and Lumen System

The mirror does not create fire. It redirects outside light.

Cave people call the result `연금술` because they do not understand the surface, sunlight, optics, or heat. The player and NPCs initially interpret the mirror as alchemy. Later, the player realizes:

> I did not create light. I only changed where it went.

### Mirror Rules

- The mirror only works when light exists in the room.
- Floor 3 introduces thin shafts of outside light from cracks above.
- A beam hitting the mirror reflects according to the mirror angle.
- Beam intensity decays over distance unless the player is in light state or uses an upgrade.
- Focused beams can heat, burn, reveal, stun, or activate mechanisms.
- Some NPCs panic or suffer if hit by focused light.

### Mirror Actions

| Mirror Action | Use | Limitation |
| --- | --- | --- |
| Reflect | Redirect a beam to a receiver or surface | Requires correct angle |
| Focus spark | Short pulse that burns weak bindings | Requires a light source |
| Lumen cut | Sustained beam that breaks false walls or glass seals | Player is slowed while aiming |
| Reveal | Shows hidden faces, inscriptions, or true enemy bodies | Can frighten shadow NPCs |
| Overfocus | Powerful but risky beam | Can harm NPCs or trigger hazards |

### Breakable Path Types

| Path Type | Broken By | Thematic Meaning |
| --- | --- | --- |
| Tar veil | Focus spark | Fear made physical |
| False wall | Reveal plus strike | Illusion exposed |
| Glass seal | Sustained lumen cut | Memory barrier |
| Chain lock | Lumen cut or chain strike | Certainty and control |
| Shadow membrane | Shadow movement | Acceptance of darkness |

### Light and Shadow State

Light state:

- Improves beam range and stability.
- Enables healing with K skill.
- Reveals hidden inscriptions, NPC faces, and enemy truth forms.
- Can panic shadow NPCs when used directly.

Shadow state:

- Enables stealth with K skill.
- Calms certain frightened enemies or NPCs.
- Lets some enemies become less aggressive.
- Later enables shadow movement through translucent barriers.
- Can obscure clues if overused.

## 7. World Structure

### Overall Map

The world is a cave arranged as a symbolic ascent and return.

| Act | Direction | Purpose |
| --- | --- | --- |
| Prologue | Deep cave | Wake up chained, survive, learn baseline combat |
| Act I: The Ascent | Floor 4 to Floor 1 | Escape the cave and unlock the core kit |
| Act II: The Surface | Surface | Encounter truth and remember those left below |
| Act III: Return of Light | Floor 1 to Floor 4 | Return to rescue, confront, or understand |
| Finale | Heart of Truth | Final choice and ending |

### Act I Floor Summary

| Floor | Name | Theme | Core Tool | Main Boss | Reward |
| --- | --- | --- | --- | --- | --- |
| Floor 4 | Nameless Depths | Fear | Chain | Form of Fear | Chain break, wall jump |
| Floor 3 | Stage of Lies | Illusion | Broken Shackle Mirror | False Self | Air dash, mirror confidence |
| Floor 2 | Maze of Oblivion | Memory | Prism lens | The Oblivioned One | Beam split/stability |
| Floor 1 | Threshold of Escape | Certainty | Mirror plus shadow gates | King of Chains | Shadow movement |

### Act III Return Summary

| Floor | Return Name | Theme | Gameplay Focus | Major Event |
| --- | --- | --- | --- | --- |
| Return Floor 1 | Hall of Return | Self-hatred | Enemy groups, traps, shadow blockers | First enemies reveal human faces |
| Return Floor 2 | Abyss of Betrayal | Past | Complex mirror puzzles, timers | Forgotten One mercy choice |
| Return Floor 3 | Crater of Truth | Madness | Light pressure, moving prisms | Truth can harm when forced |
| Return Floor 4 | Sanctum of Shadows | Faith | Mirror plus shadow movement mastery | High Shadow boss |
| Final | Heart of Truth | Choice | Full-kit boss and final dialogue | Shadow Sovereign |

## 8. Floor 4: Nameless Depths

### Theme

Fear, bondage, first survival.

Floor 4 is almost lightless. The player is physically chained. This floor should feel low, heavy, wet, and close. The player has enough power to survive, but not enough freedom to understand the cave.

### Gameplay Focus

- Teach movement, dash, jump, and double jump.
- Teach chain attack range and recovery.
- Teach enemy pattern observation.
- Use darkness and sound to build fear.
- End with the chain breaking as a major emotional and mechanical event.

### Basic Monsters

| Enemy | Behavior | Counterplay | Purpose |
| --- | --- | --- | --- |
| Crawling Silhouette | Low enemy that rushes along the ground | Jump, chain strike, spacing | Teaches basic threat reading |
| Hanging Shade | Waits above, drops when player passes | Stop, bait, dash away | Teaches vertical awareness |
| Chainbiter | Latches onto the player's chain and pulls | Attack quickly or dodge before latch | Makes the chain feel vulnerable |
| Breathless | Reduces local visibility when close | Keep distance, strike during inhale | Turns fear into a spatial mechanic |

### Mid Boss: The Trembling Mass

The Trembling Mass is several frightened silhouettes tangled around a broken pillar.

Combat:

- Rolls slowly, then suddenly lunges.
- Splits into smaller silhouettes at health thresholds.
- Hides in darkness and shakes the screen lightly before charging.

Counterplay:

- Use chain range to stay out of contact.
- Dash through the charge.
- Strike smaller pieces before they regroup.

Purpose:

- Tests chain control and fear management.
- Foreshadows that many monsters may be people or memories clustered together.

### Main Boss: Form of Fear

The Form of Fear is the first major boss. It is not a clean monster. It looks like the player's own panic wearing a cave body.

Attack Families:

- Pressure: close swipes, ground rushes, corner pressure.
- Reposition: crawls onto walls and ceiling.
- Punish: drops spikes or darkness pulses when the player hides too long.

Boss Reward:

- The boss fight ends with the player's chain breaking.
- The player unlocks wall jump or unbound movement.
- Chain attacks are removed or reduced to broken-link melee.

Sample Dialogue:

> "You called the chain a weapon because you were afraid to call it a prison."

## 9. Floor 3: Stage of Lies

### Theme

Illusion, performance, first light.

Floor 3 is higher than Floor 4, so a small amount of outside light enters through cracks, shafts, and thin openings. The light is not warm yet. It feels strange, narrow, and almost artificial because the cave people have never understood it.

### Gameplay Focus

- Teach that light can be redirected.
- Introduce the Broken Shackle Mirror.
- Use mirror-focused light to reveal false paths and break certain barriers.
- Make cave people call mirror use `연금술`.
- Fight enemies that lie through movement, reflections, and decoys.

### Mirror Acquisition

The broken chain leaves behind a polished shackle plate. In Floor 3, the first thin light shaft hits it. The player sees that the shackle can reflect light.

Cave people interpret this as alchemy:

> "The prisoner turned darkness into fire."

The truth:

> The player gathered outside light and sent it somewhere else.

### Basic Monsters

| Enemy | Behavior | Counterplay | Purpose |
| --- | --- | --- | --- |
| False One | Creates decoys and hides the real body | Reveal with mirror light or watch shadow mismatch | Teaches truth-reading |
| Mask Dancer | Copies player movement, then attacks opposite rhythm | Delay attacks, punish after flourish | Teaches not trusting surface motion |
| Vanishing Pilgrim | Disappears near the player and reappears behind | Keep moving, use light reveal | Teaches controlled space |
| Wall Projection | Body is harmless, wall shadow attacks | Move light angle or attack the shadow source | Connects Plato cave imagery to combat |

### Mid Boss: The Mirror Actor

The Mirror Actor performs as if it is the player.

Combat:

- Copies the player's chain memory even after the chain is gone.
- Creates a mirrored clone that attacks from the opposite side.
- Only the incorrect reflection takes full damage during certain windows.

Counterplay:

- Use mirror light to reveal which figure casts the wrong shadow.
- Punish after the actor finishes a copied combo.
- Use vertical spacing and newly freed movement.

Purpose:

- Teaches that reflection can expose lies.
- Bridges chain combat memory into mirror combat.

### Main Boss: False Self

False Self embodies the version of the protagonist that would rather perform freedom than become free.

Attack Families:

- Pressure: mirrored melee strings and close feints.
- Reposition: aerial flips, false platforms, decoy swaps.
- Punish: counters reckless mirror focusing with reflected beams.

Boss Reward:

- Air dash.
- Improved mirror confidence: slightly faster aim entry or reduced beam wobble.

Sample Dialogue:

> "A mirror does not lie. It only repeats the angle you gave it."

## 10. Floor 2: Maze of Oblivion

### Theme

Memory, forgetting, repetition.

Floor 2 uses the mirror as a memory tool rather than a first discovery. The player already understands basic reflection. Now the game asks them to hold, split, and stabilize light long enough to recover lost truths.

### Gameplay Focus

- Looping rooms and route memory.
- Memory fragments.
- Receiver doors and timed light puzzles.
- Prism lens as a mirror upgrade.
- Boss patterns that repeat with small differences.

### Basic Monsters

| Enemy | Behavior | Counterplay | Purpose |
| --- | --- | --- | --- |
| Form of Oblivion | Patrols the same loop and splits routes | Learn route, use vertical movement | Makes memory spatial |
| Archivist Larva | Rewinds a small part of the room | Interrupt ritual or route light to seal | Teaches time pressure |
| Hollow Familiar | Looks like a known NPC, attacks when approached | Use light reveal, approach slowly | Makes memory emotionally unsafe |
| Name-Eater | Removes labels, map marks, or visual signs temporarily | Defeat quickly or restore with receiver light | Turns forgetting into UI/world pressure |

### Mid Boss: The Keeper of Names

The Keeper of Names collects identities from cave people and stores them in glass tablets.

Combat:

- Summons name fragments as orbiting projectiles.
- Hides its true body among labeled decoys.
- Temporarily scrambles room signage or map symbols.

Counterplay:

- Use mirror light to read the true name tablet.
- Strike the body whose reflection is missing.
- Keep track of loop order.

Purpose:

- Makes memory loss mechanical.
- Prepares the player for the Forgotten One.

### Main Boss: The Oblivioned One

The Oblivioned One is a major memory figure and may connect to the person the protagonist abandoned during the first escape.

Attack Families:

- Pressure: repeated routes that become faster each loop.
- Reposition: disappears into memory doors and returns from alternate exits.
- Punish: damages the player for attacking the wrong memory clone.

Boss Reward:

- Prism lens.
- Beam splitting or sustained receiver activation.
- A major recovered memory about the first escape.

Sample Dialogue:

> "You did not forget me. You learned to live as if I was gone."

## 11. Floor 1: Threshold of Escape

### Theme

Certainty, control, the final lie before the surface.

Floor 1 is close to the surface, but it is also the most controlled part of the cave. The people here have built doctrine around light. They accept only light that has been named, owned, and regulated.

### Gameplay Focus

- Mirror routing through more deliberate architecture.
- Translucent walls and shadow gates.
- Enemies with shields, chains, and ritualized denial.
- The final ascent gate.

### Basic Monsters

| Enemy | Behavior | Counterplay | Purpose |
| --- | --- | --- | --- |
| Denier | Shields from the front | Attack from behind, above, or diagonal | Teaches certainty as defense |
| Chain Saint | Locks parts of the arena with chain lines | Break locks with light or route around | Echoes Floor 4 bondage |
| Lumen Zealot | Uses controlled light bursts as attacks | Hide in shadow, punish cooldown | Shows false ownership of truth |
| Gatebound | Anchors itself to doors and receivers | Split beam or interrupt chant | Combines combat and puzzle pressure |

### Mid Boss: The Gate Confessor

The Gate Confessor guards the final passage upward and repeats that the outside does not exist.

Combat:

- Uses a shielded front.
- Creates confession circles that restrict movement.
- Light reveals a human face under the mask, causing hesitation windows.

Counterplay:

- Use mirror routing to expose the back of the armor.
- Attack during confession recovery.
- Choose whether to overfocus light or spare the exposed face.

Purpose:

- Tests the player's use of light with restraint.
- Foreshadows return-act enemies revealing human traits.

### Main Boss: King of Chains

The King of Chains is the ruler of certainty. Unlike Floor 4's chain, which was personal bondage, this boss represents social bondage: law, doctrine, fear, and comfort made into chains.

Attack Families:

- Pressure: chain sweeps, lock-on hooks, close denial.
- Reposition: phase movement through translucent walls.
- Punish: arena segments become inaccessible if the player relies on one route too long.

Boss Reward:

- Shadow movement.
- Access to the final ascent route.

Sample Dialogue:

> "A chain is only cruel to the hand that wants to leave."

## 12. Act II: The Surface

The surface should be short but emotionally important.

Goals:

- Provide visual contrast: warm, bright, open.
- Temporarily reduce or remove combat.
- Let the player feel freedom and discomfort at the same time.
- Restore memory: this is not the first escape.
- Present the moral turn: the player can leave, but others remain trapped.

The surface should also clarify the truth of `연금술`:

- The light was never created underground.
- The cave only captured thin fragments of it.
- The mirror was not magic.
- The player's "alchemy" was direction, not creation.

Sample Surface Realization:

> "Above, light does not need permission."

## 13. Act III: Return of Light

The return act should reuse major spaces but change enemy placement, lighting, and NPC interpretation.

### Return Principles

- Some enemies reveal human faces under light.
- Some NPCs panic if the player forces light on them.
- Some paths open only through shadow movement.
- Mirror puzzles become moral puzzles, not just routing puzzles.
- The player can guide, abandon, or accompany the cave people.

### Return Floor Highlights

Return Floor 1: Hall of Return

- First enemies reveal human faces.
- Shadow blockers are introduced.
- Player learns that some "monsters" were people defending certainty.

Return Floor 2: Abyss of Betrayal

- Complex mirror puzzles and timers.
- Forgotten One confrontation.
- Mercy choice: lower weapon, overfocus light, or leave.

Return Floor 3: Crater of Truth

- Moving prisms.
- Truth can harm when forced.
- Overlit enemies show what happens when light is treated as a weapon.

Return Floor 4: Sanctum of Shadows

- Mirror plus shadow movement mastery.
- High Shadow boss.
- The darkest floor becomes the place where the player must understand darkness, not erase it.

## 14. Narrative Design

### Theme

Penumbra adapts the emotional shape of Plato's cave allegory, but rejects a simple "light good, dark bad" reading.

The revised version adds a second metaphor:

- The chain is survival through bondage.
- The mirror is truth through redirection.
- `연금술` is what people call a phenomenon before they understand it.

The story asks:

> If truth is like focused light, when does guidance become violence?

### Protagonist

The protagonist wakes in the cave with no memory. They bear old chains and a strange connection to both light and shadow. They eventually remember they escaped before, reached the surface, and abandoned someone in the cave.

### Key Characters

| Character | Role | Function |
| --- | --- | --- |
| Protagonist | Player character | Embodies the choice to return |
| Cave people | Trapped inhabitants | Show fear, denial, dependence, and survival |
| Forgotten Companion | Abandoned memory figure | Personalizes guilt and equivalent exchange |
| Shadow Painter | NPC who paints shadows for others | Shows compassionate falsehood |
| Chainwright | Upgrade NPC / maker of restraints | Turns chains, mirrors, and memory into trade |
| Surface Witness | Escaped person who refuses to return | Shows that escape is not automatically wisdom |
| Forgotten One | Memory boss | Represents abandonment and the cost of escape |
| High Shadow | Late-game ideological boss | Defends darkness as survival and identity |
| Shadow Sovereign | Final boss | Embodies attachment to ignorance, pain, and certainty |

### Dialogue Style

- Short, poetic, direct.
- Dialogue should appear at emotional peaks, before bosses, after major choices, and near environmental discoveries.
- Avoid long philosophical explanations.
- Let mechanics carry the argument.

Sample tone:

> "You did not make fire. You aimed what was already burning."

> "Light forced into closed eyes is still violence."

> "The cave called it alchemy because the cave had never seen the sun."

## 15. Endings

Ending conditions should use both behavior and explicit choices. Resource percentage alone should not determine the ending.

### Ending Variables

- Light usage percentage.
- Shadow usage percentage.
- Rescue interactions completed.
- Mercy choices selected.
- Mirror puzzles completed.
- Overfocus incidents against NPCs.
- Whether the player lowers the weapon during the Forgotten One encounter.
- Final dialogue choice.

### Ending Table

| Ending | Conditions | Outcome |
| --- | --- | --- |
| Guide Ending | High light usage, many rescue choices, mirror puzzles completed, mercy selected, low forced-light harm | The player leads cave people toward the surface |
| Wanderer Ending | Balanced resources, low rescue commitment, avoids ideological extremes | The player leaves alone and lives on the surface |
| Companion Ending | High shadow usage, few rescue actions, chooses to understand or remain with the shadows | The player remains in the cave with those who reject the light |

No ending should feel like a simple fail state. Each ending should answer:

> What does someone owe to people still inside the cave?

## 16. Enemy and Boss Design Rules

### Enemy Rules

- Early enemies teach one idea each.
- Later enemies combine movement pressure with state-specific vulnerabilities.
- Every floor introduces one primary behavior, then combines it with older behaviors.
- Return-act enemies should sometimes reveal human traits.
- Enemies should express internal states: fear, denial, false self, forgotten memory, corrupt certainty.

### Boss Rules

- Every boss has three readable attack families:
  - Pressure.
  - Reposition.
  - Punish.
- Every boss tests the ability or idea connected to the floor.
- Boss dialogue should be brief.
- Bosses should be symbolic, but their patterns must remain readable.
- Final boss should react to the player's dominant state and choices.

## 17. Level Design

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

### Backtracking

Backtracking should be meaningful but not tedious.

- Wall jump opens vertical routes in Floor 4.
- Air dash opens gaps and optional rooms on Floors 4 and 3.
- Mirror opens false walls, receiver doors, and memory routes.
- Prism lens enables multi-receiver puzzles.
- Shadow movement opens translucent passages and shortcuts.
- Return act should alter enemy placement, lighting, and NPC interpretation.

### Checkpoints

- Rest points at the start and midpoint of each floor.
- Checkpoint before each boss.
- Return act should unlock shortcuts to reduce repeated traversal.

## 18. Art Direction

### Visual Identity

Penumbra uses high-contrast silhouette art with strong light/shadow composition. The player, enemies, and cave architecture should read clearly even in dark scenes.

### Palette Direction

- Floor 4: black, cold gray, deep blue, rusted chain metal.
- Floor 3: dark violet, thin gold shafts, reflective silver.
- Floor 2: muted blue, glass green, pale memory white.
- Floor 1: controlled gold, bone white, black chain lines.
- Surface: white, pale gold, warm green, sky tones.
- Danger: muted red or hot white, used sparingly.

### Environment Motifs

- Chains.
- Cave walls and shadow screens.
- Murals and half-visible inscriptions.
- Mirrors and fractured glass.
- Light cracks from the surface.
- Receiver doors with sun/cave symbols.
- Human faces hidden in enemy silhouettes.
- False alchemy diagrams that are actually misunderstood optics.

## 19. Audio Direction

### Music

- Floor 4: heavy, muffled, heartbeat-like percussion.
- Floor 3: thin tones, glass resonance, unstable melodies.
- Floor 2: looped motifs with subtle changes.
- Floor 1: ritual rhythm, controlled choir-like textures.
- Surface: warmer, wider, more harmonic.
- Return act: distorted versions of earlier floor motifs.

### Sound Effects

- Chain: metal drag, heavy swing, impact, strain, final break.
- Mirror: glass hum, angle ticks, beam focus rise.
- Focused light: sharp hiss, heat crackle, receiver resonance.
- Light state: clear, resonant, glass-like tones.
- Shadow state: muffled, low, breath-like textures.

## 20. Technical Design Notes

### Recommended Systems

- Player controller.
- Chain attack controller.
- Ability unlock manager.
- Combat hitbox/hurtbox system.
- Enemy state machines.
- Boss state machines.
- Light/shadow state manager.
- Resource and ending flag tracker.
- Mirror beam simulation.
- Breakable light path system.
- Interactable and dialogue system.
- Save/checkpoint system.
- Room/scene loading.
- Map discovery system.

### Mirror Implementation

For the prototype:

- Use 2D raycasts for beam direction.
- Reflect ray direction using surface normals or mirror angle.
- Limit reflection count to a small number, such as 3-5.
- Draw beams with LineRenderer or custom 2D sprites.
- Receivers detect beam contact and intensity.
- Breakable objects listen for sustained beam intensity.
- Focused light combat should require an external light source flag.
- Add debug gizmos for beam path, receiver threshold, and breakable intensity.

### Data Approach

Use ScriptableObjects for:

- Enemy stats.
- Boss phase data.
- Ability definitions.
- Chain attack tuning.
- Mirror beam tuning.
- Breakable light path definitions.
- Upgrade definitions.
- Dialogue snippets.
- Room metadata.
- Ending thresholds and flags.

## 21. Prototype Plan

### MVP Prototype

Goal: prove the revised identity at the smallest scale.

Required:

- Player run, jump, double jump, dash.
- Chain attack with at least one enemy.
- Chain break moment, even if scripted.
- Post-chain movement feel change.
- One thin light source.
- One mirror reflection interaction.
- One breakable path opened by focused light.
- One checkpoint.

### Vertical Slice

Goal: prove the full revised Penumbra promise.

Required:

- Floor 4 mini-route with chain combat.
- Floor 4 boss or mini-boss that breaks the chain.
- Floor 3 mini-route with first mirror use.
- Two enemy types.
- One mirror path-breaking puzzle.
- One short dialogue sequence where NPCs call the mirror effect `연금술`.
- Basic HUD and pause menu.
- Save/checkpoint loop.

### Full Production Milestones

| Milestone | Target |
| --- | --- |
| M1: Controller and chain foundation | Player movement, chain combat, one enemy |
| M2: Chain break and unbound movement | Boss event, wall jump, post-chain melee |
| M3: Mirror prototype | Beam reflection, focused light, breakable path |
| M4: Floor 4 slice | Nameless Depths route, mid boss, main boss |
| M5: Floor 3 slice | Stage of Lies route, mirror tutorial, False Self |
| M6: Act I complete | Four floors, four main bosses, major unlocks |
| M7: Act II complete | Surface scene, memory reveal, truth of "alchemy" |
| M8: Act III complete | Return floors, advanced puzzles, High Shadow |
| M9: Finale and endings | Final boss, ending logic, credits |
| M10: Polish and QA | Balance, accessibility, bug fixing, performance |

## 22. Open Questions

1. After the chain breaks, should the player keep a short broken-link melee weapon or become fully mirror-focused?
2. Should the mirror be physically made from the broken shackle, or found separately in Floor 3?
3. Should focused light directly damage enemies, or mostly reveal, stun, and break armor?
4. Should `연금술` be a religious term, a technical guild term, or a folk myth among cave people?
5. Should Floor 3 reward only air dash, or both air dash and a mirror stability upgrade?
6. How often should forced light harm NPCs, and should the game warn the player before the first serious consequence?
7. Should the final game use English dialogue, Korean dialogue, or bilingual terminology?

## 23. Immediate Next Steps

1. Update the playable prototype plan around Floor 4 chain combat.
2. Tune chain attack timing, range, recovery, and hitstop.
3. Build one chain-vulnerable enemy and one chain-resistant enemy.
4. Prototype a scripted chain break moment.
5. Implement one simple external light source on Floor 3.
6. Implement mirror reflect and focus pulse.
7. Add one breakable path that opens only through focused light.
8. Write the first short NPC line calling mirror use `연금술`.
9. Review whether the new flow feels better than the old Floor 2 mirror acquisition.
