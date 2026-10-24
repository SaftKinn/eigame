# Developer conventions — detail reference

Load this file **on demand**, not every turn. It holds the per-module how-tos, testing and
runtime details, troubleshooting and style. The **non-negotiable short versions** are in
[`../CLAUDE.md`](../CLAUDE.md) (golden rules + key gotchas).

Taken over from *Chief Wants His Cut* (2026-10-24) and cut to what holds for this game. It grows
with the code: when a module, a dev switch or a footgun appears, it gets its line here in the
same change.

## Repo layout → governing ADR

Extend the table with every new folder.

| Path | What | ADR |
|---|---|---|
| `game/scripts/Main.cs` | the flow: phases, waves, gold, the shop's hand (buy, move, rotate, sell, drop), the queries pests and shooters ask (`GroundY`, `FindTarget`, `HighestUnit`, `UnitTouching`, `IsBlockedByEnemy`) | 001 |
| `game/scripts/Content/` | `ContentDefs` (the JSON's shapes), `ContentDb` (load, validate) — into the core in M1 | 001, 002 |
| `game/scripts/Units/` | `UnitBody` (`RigidBody2D`: health, shooting, recoil, hold state), `UnitVisual` (picture or stand-in, flash), `HpBar` | 001 |
| `game/scripts/Enemies/` | `Enemy` (moves along the ground or flies, attacks with impulses — not a body), `EnemyVisual` | 001 |
| `game/scripts/Combat/` | `Projectile` (straight with lead, lob with splash) | 001 |
| `game/scripts/Ui/` | `Hud` (and `Hud.Anchor`), `ShopCard`, `RangeOverlay` | 001 |
| `game/scripts/Fx/`, `View/`, `World/` | crumbs and numbers; `DrawKit` — the stand-in drawings, magenta for unknown keys; the kitchen backdrop | 001 |
| `game/content/` | all balancing JSON: `units.json`, `enemies.json`, `waves.json`, `game.json` | 001 |
| `game/art/units/`, `game/art/enemies/` | pictures by visual key (`docs/art-prompts.md`) | 001 |
| `sim/RettetDasEi.Sim/` | M1 — the rules core, no Godot | 002 |
| `tests/RettetDasEi.Sim.Tests/` | M1 — xUnit tests for the core | 002 |
| `tools/hooks/` | `check-on-change.sh` (Claude Stop hook), `pre-commit` (git) | — |

## Rules core (`sim/RettetDasEi.Sim/`, from M1 — ADR 002)

- **No `using Godot;` anywhere.** The project has no Godot reference; a Godot type needed there
  means the code belongs on the Godot side, or the core needs its own plain type.
- **The boundary:** where something is and what touches what is Godot's; how much, how long,
  whether and in which order is the core's. If a Godot script is about to write an `if` about
  damage, gold, a duration or a count, it belongs in the core.
- **Randomness:** a random decision that is a rule takes an `Rng` with a seed; never
  `System.Random`, `Random.Shared` or `GD.Randf` in the core. Cosmetic randomness on the Godot
  side may stay `GD.Rand*`.
- **Iteration order is part of the outcome:** don't iterate a `Dictionary`/`HashSet` where order
  matters; sort or use lists.
- **Hot paths allocate nothing:** no LINQ or `new List` inside per-pest loops of a step.
- **Events, not callbacks:** the core appends events; the Godot side drains them once per frame.
- **Time** is the physics step's delta (120 Hz), passed in.
- **New mechanic checklist:** content field (if tunable) → validation rule → pure logic in the
  core → test → an event if the view must react → a visual key if it is drawn.

## Content (`game/content/`)

- One file per kind, camelCase, German `name`/`description`. Ids are English `snake_case` and
  never change once saves exist (there are none yet).
- Every field is required (`[JsonRequired]`); unknown properties are errors — a typo must not
  become a silent default. An optional field is nullable on purpose (`attack`).
- Every entry that is drawn has a `visual` key. Its picture is `game/art/units/<key>.png` or
  `game/art/enemies/<key>.png` (sizes and facing in `docs/art-prompts.md`); until then `DrawKit`
  draws the stand-in — add the key's drawing there in the same change, or it renders magenta.
- **After any edit:** until M1 the Stop hook starts the game headless for a moment and reports a
  broken content file; from M1 `dotnet test` loads the real files and lists every problem.
- Tuning knobs and what they do:
  - **Toppling:** a pest's `push` (`enemies.json`) against a unit's `mass` and `friction`
    (`units.json`); `unitLinearDamp` / `unitAngularDamp` (`game.json`) calm the stack.
  - **Reach:** an attack's `range` plus `rangePerHeight` × pixels above the board (`game.json`).
  - **Economy:** `startGold`, a unit's `cost`, a pest's `gold`, a wave's `reward`, `sellRefund`.
  - **Pressure:** a wave's `groups` (`count`, `start`, `interval`, `side`); a pest's `speed`, `hp`,
    `damage`, `attackCooldown`.
  - **Shooters:** `damage`, `cooldown`, `speed`, `splash`, `recoil` (a recoil shoves the shooter
    itself — a tall light shooter can shoot itself off the tower).
- A new **required** field also goes into every entry of its file — and from M1 into the tests'
  own content.

## Godot side (`game/scripts/`)

- Nodes are `partial` classes and the file name equals the class name (Godot requirement).
- Explicit usings — `Godot.NET.Sdk` has no implicit usings.
- **Read the space state only in `_PhysicsProcess`.** Input that needs a physics query (a pick
  under the mouse) is kept and done in the next physics step (`Main._pendingPick`).
- Wake the bodies after anything that takes a piece away (`Main.WakeAll`): a sleeping body does
  not notice that what it rested on is gone.
- Collision layers: 1 the world (counter, board, walls), 2 the units. Pests are on no layer —
  they query layer 2.
- **UI built in code:** place every control with `Hud.Anchor` (anchors and all four offsets),
  never `SetAnchorsPreset` alone.
- **Motion is code, never frames** (ADR 001): tweens on the visual child, not on the body — the
  body is the physics'.
- A text the player reads is German, written where it is shown.

## Testing

- **Now (before M1):** `dotnet build game/RettetDasEi.csproj` and a headless boot —
  `<godot> --headless --path game --quit-after 300` must print no `ERROR` and no `Content is
  broken`. The Stop hook does both (below). Everything else is verified by Sezgin playing.
- **From M1:** `dotnet test tests/RettetDasEi.Sim.Tests` — no Godot needed, a few seconds. Assert
  exact outcomes with explicit set-up; prefer a tiny scenario that isolates one behaviour. Tests
  don't use the shipped content for logic (a test helper with its own small content), so tuning
  never breaks a logic test; one test loads the shipped files to validate them.
- **Read the number of tests after every run**, not only the word "Bestanden" (lesson
  `look-whether-the-file-is-there-before-writing-it`).
- No replay test of a whole wave: Godot's physics is not deterministic across runs (ADR 002).

## Hooks

- **Stop hook** (`.claude/settings.json` → `bash tools/hooks/check-on-change.sh`): when something
  under `game/scripts/`, `game/content/` (from M1 also `sim/`, `tests/`) differs from the last
  commit, it builds; when content changed it also boots the game headless for a moment and looks
  for content errors; from M1 it runs the tests. Silent on green; a failure is printed
  (non-blocking).
- **pre-commit** (`tools/hooks/pre-commit`, active through `git config core.hooksPath tools/hooks`
  — once per clone): the same check for what is staged, blocking. Never bypass it with
  `--no-verify` to get a commit through; fix the cause.

## Runtime / build

- Godot editor: import `game/project.godot`. Build with the hammer or Alt+B; F5 runs.
- **"Spiel starten.bat"** (repo root): `dotnet build` → `--import` → start; logs in
  `logs\dotnet-build.log`, `import.log`, `run.log`. When Sezgin reports a problem after using it,
  read those three first.
- The editor's exe from the command line:
  `%LOCALAPPDATA%\Microsoft\WinGet\Packages\GodotEngine.GodotEngine.Mono_*\Godot_v4.6.2-stable_mono_win64\Godot_v4.6.2-stable_mono_win64_console.exe`
  (Git Bash: `$(cygpath -u "$LOCALAPPDATA")/…`). A `godot` link on the PATH may start without its
  assemblies.
- A new or changed picture needs `--headless --path game --import` before a command-line start
  (lesson `a-changed-sound-or-picture-needs-godots-import`).
- To restart the game for Sezgin: close only this game's process (lesson
  `close-only-this-games-process`), build, start; say that it was restarted.
- A frame to look at: `<godot> --path game --write-movie <folder>\f.png --fixed-fps 30
  --quit-after 5` (a real window opens — start it off screen with `--position -4000,-4000`).
- Keys in the game: Space starts a wave, F doubles the speed, R restarts, Q/E or the wheel
  rotate what is held.

## When you're stuck on reality

- **Godot says the C# script class can't be found** → not built (Alt+B), class name ≠ file name,
  or the class isn't `partial`.
- **"SDK Godot.NET.Sdk/x.y.z could not be found"** → csproj version ≠ installed editor version.
- **A control built in code has no size or sits off screen** → `SetAnchorsPreset` alone; use
  `Hud.Anchor`.
- **The game stops at start with "Content is broken"** → read the message; it lists every problem
  with file and id.
- **Everything (or one thing) is magenta** → a `visual` key without a drawing in `DrawKit` and
  without a picture.
- **Pieces float in the air after something under them broke or was taken** → they are asleep;
  `WakeAll` was not called on that path.
- **The tower jitters when tall** → more physics ticks or solver iterations, more damping
  (`game.json`) — measure one change at a time (ADR 001, consequences).

## Style

- Commits: imperative, scoped — `units: …`, `enemies: …`, `content: …`, `ui: …`, `sim(<area>): …`,
  `tests: …`, `tools: …`, `docs: …`. Direct to `main` while it's a solo project.
- Comments explain *why* and point at ADRs; the code says *what*.
- Wrap-up messages and progress entries are for a fresh reader: one plain sentence on what changed
  and why it matters for play, then evidence, then at most five lines of mechanism.
