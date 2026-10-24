# architecture.md — Rettet das Ei

> Working title **Rettet das Ei**; `RettetDasEi` in code, folders, the `.csproj` and the
> namespace (`config/name` "Rettet das Ei", saves — once there are any — in
> `%APPDATA%\RettetDasEi`, `project.godot`: `use_custom_user_dir`).

This file is the design source of truth — **if a decision changes, update it here in the same
change.** Sections marked _open_ are not decided yet; they are filled by the decision that
settles them, not by a guess. What is planned but not built carries its milestone (`roadmap.md`).

---

## 1. Design principles

- **No frame animations, ever** (ADR 001). One still picture per unit and pest; all motion is
  code — tweens that squash, lean and snap — and the physics itself. If something looks stiff,
  it gets a better tween or more juice, never frames.
- **The physics is the game** (ADR 001). Food is Godot's `RigidBody2D`; the tower's wobble under
  a blow is the real animation. Nothing fakes a topple.
- **Godot reports facts, the rules core answers** (ADR 002, from M1). Where something is and what
  touches what is the physics'; how much, how long, whether and in which order is the core's —
  plain C#, no Godot, tested in seconds.
- **Content is data.** Every number that is tuning, not logic, is JSON in `game/content/`,
  validated at load, failing loud — never a silent default.
- **Art is behind visual keys.** Content names a key; a picture by that name is used if it exists,
  else a stand-in drawing, else magenta.
- **One function per rule.** A value computed in two places drifts apart (lesson
  `parity-by-construction`): the shooter and the range overlay ask the same function.

---

## 2. The core loop

```
        ┌──────────────── BAUPHASE (build) ───────────────┐
        │ buy food in the Laden, drag it onto the Brett    │
        │ and stack it by hand — the physics runs, a       │
        │ careless stack topples while it is built         │
        │ the Ei goes on top; sell by dragging back        │
        └───────────┬─────────────────────▲───────────────┘
                    │ Leertaste / button   │ Gold from kills and
                    ▼                      │ the wave's reward
        ┌──────────────── WELLE (wave) ────┴───────────────┐
        │ pests walk, roll, hop and fly in from both sides │
        │ and shove the tower; the food shoots by itself,  │
        │ higher food reaches further (Reichweite)         │
        │ lost: the Ei breaks or touches the Theke         │
        │ cleared: next Bauphase · last wave: gewonnen     │
        └──────────────────────────────────────────────────┘
```

**The tension:** height. A tall tower shoots further and keeps pests off the egg longer, but a
tall tower topples. Every purchase is weighed between "stable" (wide, heavy blocks) and "reach"
(shooters high up).

**What persists:** nothing yet — one game is five waves from a fresh start (`R` restarts).
Runs, unlocks and saves wait for the prototype rule in `CLAUDE.md` (Part 2 of `roadmap.md`).

---

## 3. Solution layout

```
rettet-das-ei/
├── game/                           Godot project (open THIS folder in the editor)
│   ├── project.godot               1920×1080, stretch canvas_items, aspect keep; physics 120 Hz, 24 iterations
│   ├── RettetDasEi.csproj          Godot.NET.Sdk 4.6.2 — everything for now; references the core from M1
│   ├── RettetDasEi.sln             the solution (Godot expects it next to project.godot)
│   ├── content/*.json              ALL balancing: units, enemies, waves, game (global tuning)
│   ├── art/units/, art/enemies/    pictures by visual key (docs/art-prompts.md); empty so far
│   ├── scenes/main.tscn            the one scene; Main builds everything in code
│   └── scripts/
│       ├── Main.cs                 the flow: phases, waves, gold, the shop's hand, queries for others
│       ├── Content/                ContentDefs (the JSON's shapes), ContentDb (load + validate) — moves to the core in M1
│       ├── Units/                  UnitBody (RigidBody2D: health, shooting, drag state), UnitVisual, HpBar
│       ├── Enemies/                Enemy (moves, attacks with impulses), EnemyVisual
│       ├── Combat/                 Projectile (straight with lead, lob with splash)
│       ├── Ui/                     Hud (gold, wave, messages, shop, end page; Hud.Anchor), ShopCard, RangeOverlay
│       ├── Fx/                     Fx (crumbs, numbers), FloatingText
│       ├── View/                   DrawKit — the stand-in drawings, magenta for unknown keys
│       └── World/                  KitchenBackdrop
├── sim/RettetDasEi.Sim/            M1 — the rules core, NO Godot reference (ADR 002)
├── tests/RettetDasEi.Sim.Tests/    M1 — xUnit, runs without Godot
├── docs/                           decisions/ (ADRs), lessons/, conventions.md, art-prompts.md
├── tools/hooks/                    check-on-change.sh (Claude Stop hook) + pre-commit
├── .claude/                        settings.json (Stop hook) + project skills
├── Spiel starten.bat               builds, imports, starts; logs in logs\
└── logs/                           git-ignored: dotnet-build.log, import.log, run.log
```

The folder → governing ADR map is in `docs/conventions.md`.

---

## 4. Tech stack

| Part | Choice | Why |
|---|---|---|
| Engine | **Godot 4.6.2 (.NET build)**, desktop target | The stack that works on Sezgin's machine (Chief Wants His Cut); good 2D physics built in (ADR 001). |
| Language | **C# 12 / .NET 8** | Godot 4.6's .NET target; SDK 8.0 installed. |
| Physics | **Godot's built-in 2D physics**, 120 ticks/s, 24 solver iterations | ADR 001 point 3; if tall stacks jitter, first more ticks/iterations/damping, then another backend. |
| Content | **JSON** via `System.Text.Json` (BCL) | Diffable, editable by an agent, testable without Godot. |
| Core math (M1) | `System.Numerics` | Keeps the core Godot-free. |
| Tests (M1) | **xUnit** | `dotnet test` without the editor. |

**Dependencies:** none beyond Godot.NET.Sdk and the BCL. A new NuGet package needs a line here
and in its commit message.

---

## 5. The game as built (prototype 0.1, played 2026-10-24)

Everything below lives in `game/scripts/` today. What ADR 002 moves into the core is marked
**→ core (M1)**.

### 5.1 Flow and phases (`Main.cs`)

`Main` builds the scene in `_Ready`: backdrop, static bodies (the counter, the board with a
short ramp on each side, two walls), the layers for units, enemies, projectiles and effects, a
camera (for the shake), the HUD, and the egg on the middle of the board. `Phase` is `Build`,
`Wave` or `Over` **→ core (M1)**.

- **A wave starts** (`TryStartWave`) only in the build phase, with nothing in the hand, with the
  egg not on the counter, and while waves are left. The spawn queue is built from the wave's
  groups — `start + i × interval`, the side "left", "right" or "both" (alternating, the first
  from the right) — and sorted by time **→ core (M1)**.
- **During a wave** `_PhysicsProcess` releases what is due from the queue; the egg touching the
  counter is lost at once; an empty queue and no pest left is cleared: the wave's reward is
  added, the next build phase begins, and after the last wave the game is won **→ core (M1)**.
- **Lost** (`GameOver`) when the egg breaks (its health at 0) or touches the counter during a
  wave. The end page shows how many waves were survived. `R` reloads the scene, `F` doubles
  `Engine.TimeScale`.

### 5.2 Food (`UnitBody`, `units.json`)

A `RigidBody2D` per piece: `mass`, `friction` (bounce 0), damping from `game.json`, contact
monitoring on. The shape is a box, a circle, or a capsule (a wide one laid on its side; the egg
is a capsule). Collision layers: 1 the world, 2 the units.

- **Shooting:** a unit with an `attack` counts its cooldown down during a wave and fires at
  `Main.FindTarget` — the nearest living pest on screen by horizontal distance within its range.
  **Range by height:** `range + max(0, BoardTop − y) × rangePerHeight` **→ core (M1)**. A shot is
  wind-up (squash, lean back) → release → elastic settle (ADR 001), and the recoil is a real
  impulse against the direction of the shot.
- **Being hit:** `TakeHit` applies the pest's impulse at the contact point, then the damage; a
  white flash, a number. At 0 health the piece breaks into crumbs and is gone; the egg's break
  ends the game.

### 5.3 Pests (`Enemy`, `enemies.json`)

Not physics bodies (ADR 001): each asks the space what it touches (`Main.UnitTouching`, a circle
of its radius + 3 px on the units' layer) and pushes with impulses, so it can neither jam nor be
launched by the stack.

- **Moving:** walkers follow the ground (`Main.GroundY`: board, ramp, counter) — *waddle* tilts,
  *roll* spins with the distance, *hop* squats and leaps; walkers queue behind one another
  (`IsBlockedByEnemy`) and turn round beyond the screen's edge. *Fly* heads for the highest piece
  (`Main.HighestUnit`) and hovers.
- **Attacking:** on touching food it attacks in a cycle of `attackCooldown` (at least wind-up
  0.28 s + strike 0.08 s + recover 0.34 s): lean back, lunge, and at the lunge the food gets
  `damage` and an impulse of `push` — walkers forward and a little upward (that is what tips a
  tower), flyers towards the piece. A hit pest is knocked back 5 px; at 0 health it is flung
  away spinning and fades; its `gold` is added **→ core (M1)**.

### 5.4 Projectiles (`Projectile`)

*Straight* flies at `speed`, aimed ahead of the target by its velocity; *lob* picks an arc
(0.55–1.3 s, `lobGravity`) to where a walker will be on the ground and splashes there. A hit is
any living pest within its radius + 10 px; `splash` > 0 damages every pest within the splash
radius. Life 3 s, gone off screen.

### 5.5 The hand: buying, moving, selling

- **Buy:** press a shop card in the build phase with enough gold → the gold is paid and the piece
  is in the hand **→ core (M1: the payment)**. The physics state of a held piece is frozen and
  collides with nothing; it follows the mouse (kept above the counter), rotated by the wheel or
  Q/E in steps of 15°.
- **Overlap:** a held piece that overlaps something is tinted red; let go anyway, it is moved up
  until it is free and drops the last few pixels.
- **Sell:** let go over the shop. Still in the hand since it was bought: the full price; placed
  once: `cost × sellRefund × hp / maxHp`, rounded **→ core (M1)**. The egg cannot be sold.
- Bodies are woken after every pick, drop and break — a sleeping body does not notice that the
  piece under it is gone.

### 5.6 Physics settings

`project.godot`: 120 physics ticks a second, at most 12 steps a frame, 24 solver iterations.
`game.json`: `unitLinearDamp`, `unitAngularDamp`, `lobGravity`. Read the space state
(`DirectSpaceState`) only in `_PhysicsProcess` — a pick from the mouse is kept as
`_pendingPick` and done in the next physics step.

### 5.7 Decided, not built: every food a job (ADR 003, M2)

What changes once M2 is built (after M1), all numbers from content:
- **Tomate** shoots no more: a bomb that bursts at any touch with a pest — damage to pests in its
  radius, a push without damage to own food (the egg too).
- **Käse** glues what comes to rest on its top; a hard blow tears it loose (cheese strings); it
  sticks again at rest; picking up loosens it.
- **Toast** is a shield wall: a pest striking it is thrown back, the toast takes a share of the push.
- **Gurke**'s slices pierce up to N pests. **Senf** splashes less and leaves a puddle that slows
  walkers (puddles don't stack). **Pfeffer** builds an itch; at N hits a pest sneezes — a push back
  and a pause in attacking.
- Each shop card names the job in a sentence; in play the job is seen, without frames.

---

## 6. The rules core (ADR 002; M1, not built yet)

`sim/RettetDasEi.Sim` — no `using Godot;`. Godot reports facts and gives commands; the core holds
a small state, answers with values and appends events.

- **Content:** the defs and `ContentDb` move over; `ContentDb.Load(readFile)` takes a function
  that reads a file (Godot's `FileAccess` in the game, `File.ReadAllText` in tests). All
  problems are reported at once; unknown properties are errors.
- **The game's state:** gold, the wave index, the phase, the spawn schedule and the wave clock.
  Commands: buy, sell (with the refund rule), start a wave, a time step (the physics step's
  delta), a pest killed, the egg broken or on the counter. Events: a pest is due (which, from
  which side), a wave cleared (reward), won, lost (why).
- **Attack numbers:** range by height as one function; from M2 on every effect of proposal A
  (state per pest: slowed, sneezing — with their timers ticked by the step).
- **Recipes (M3):** a function of "which piece rests on which" → the recipes found.
- **What it does not know:** positions in the world beyond what a rule needs, shapes, contacts,
  pictures. A random decision that is a rule draws from a seeded `Rng`.

The pattern every core module follows: a state object and functions that advance it by a
command or a fixed step; no hidden state outside it; what follows from content and state is a
function, not a stored value; the view learns what happened through events it drains once per
frame.

---

## 7. Content

One file per kind in `game/content/`, camelCase, comments and trailing commas allowed. Every
field is required (`[JsonRequired]`); unknown properties are errors (`UnmappedMemberHandling
.Disallow`); `ContentDb.Validate` collects every problem and throws once with all of them. Ids
are English `snake_case`; `name` and `description` are German.

- `units.json` — food (`UnitDef`): `inShop`, `cost`, `shape` (box, circle, capsule, egg), `width`,
  `height`, `mass`, `friction`, `hp`, `fill`/`accent` (colours of the stand-in), `visual`, and an
  optional `attack` (`AttackDef`: `projectile` straight or lob, `visual`, `damage`, `cooldown`,
  `range`, `speed`, `splash`, `recoil`). Six in the shop: Käseblock, Toastbrot, Gurken-Kanone,
  Senfflasche, Pfefferstreuer, Kirschtomate; and the egg (not in the shop).
- `enemies.json` — pests (`EnemyDef`): `movement` (waddle, roll, hop, fly), `radius`, `hp`,
  `speed`, `damage`, `push`, `attackCooldown`, `gold`, colours, `visual`. Ameise, Fleischbällchen,
  Fruchtfliege, Wackelpudding.
- `waves.json` — waves in order (`WaveDef`: `reward`, `groups` of `enemy`, `count`, `start`,
  `interval`, `side`). Five.
- `game.json` — one object (`GameDef`): `startGold`, `rangePerHeight`, `eggUnit`, the damping,
  `sellRefund`, `lobGravity`.

Validated: shapes, projectile kinds, movements and sides from fixed lists; positive sizes, mass,
health, speeds, cooldowns; colours parse; the egg exists and is not in the shop; a wave has
groups and names known pests. From M1 a test loads the shipped files.

---

## 8. Presentation

- **Visual keys:** `UnitVisual` uses `res://art/units/<visual>.png` (drawn facing right) and
  `EnemyVisual` `res://art/enemies/<visual>.png` (facing left) when the file exists, fitted into
  the unit's `width × height`; else `View/DrawKit.cs` draws the stand-in. An unknown key draws
  magenta and warns once. Projectiles are drawn by `DrawKit` from the attack's `visual`. The GPT
  prompts and sizes: `docs/art-prompts.md`. _Open:_ whether the drawn stand-ins stay as the
  game's look instead of GPT pictures (Sezgin likes them, 2026-10-24) — its own ADR.
- **Motion is code** (ADR 001): a unit's shot, a pest's waddle/roll/hop/hover and attack, a white
  flash on a hit, crumbs, floating numbers, the camera's shake; death is a fling and a fade.
- **HUD** (`Ui/Hud.cs`, a `CanvasLayer` built in code): gold, wave, speed top left; messages and
  the egg's warning top centre; the help line and the sell hint above the shop; the shop along the
  bottom (`ShopCard` per unit); the end page. **Placing a control built in code:** `Hud.Anchor` —
  anchors and all four offsets together, never `SetAnchorsPreset` alone (a fresh control stays at
  size zero; on 2026-10-24 the shop sat above the screen).
- **Range overlay:** while a shooter is held, a yellow band on the counter shows its reach at the
  height it is held.
- **Resolution:** 1920×1080 base, scaled to the window (1280×720 by default); 2× MSAA.
- **Input:** mouse (drag, wheel), Q/E, Space, F, R.

---

## 9. Open design questions

Tracked in `progress.md` `## Open questions` — not here. This file records what is decided.
