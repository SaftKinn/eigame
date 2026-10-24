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
| `game/scripts/Main.cs` | the scene: reports what the physics saw to `GameState` and shows its events (`ReadEvents`); the shop's hand (buy, move, rotate, sell, drop); the queries pests and shooters ask (`GroundY`, `FindTarget`, `HighestUnit`, `UnitTouching`, `Egg`, `CrowdIndex`) | 001, 002 |
| `game/scripts/Content/` | `GameContent` — `res://content/` read through the core's `ContentDb.Load` | 002 |
| `game/scripts/Units/` | `UnitBody` (`RigidBody2D`: health, shooting, recoil, the bomb's trigger, `Shove`, `SpringBack`, hold state), `UnitVisual` (picture or stand-in, flash), `HpBar`, `GlueBonds` (the cheese's pins) | 001, 003 |
| `game/scripts/Enemies/` | `Enemy` (moves along the ground or flies, attacks with impulses — not a body; holds its `PestEffects`, is thrown back, sneezes, slows in puddles), `EnemyVisual` | 001, 003 |
| `game/scripts/Combat/` | `Projectile` (straight with lead and pierce, lob with splash and puddle, itch), `Puddle` | 001, 003 |
| `game/scripts/Ui/` | `Hud` (and `Hud.Anchor`), `ShopCard`, `RangeOverlay` | 001 |
| `game/scripts/Fx/`, `View/`, `World/` | crumbs and numbers; `DrawKit` — the stand-in drawings, magenta for unknown keys; the kitchen backdrop | 001 |
| `game/content/` | all balancing JSON: `units.json`, `enemies.json`, `waves.json`, `game.json` | 001 |
| `game/art/units/`, `game/art/enemies/` | pictures by visual key (`docs/art-prompts.md`) | 001 |
| `sim/RettetDasEi.Sim/Content/` | `ContentDefs`, `ContentDb` (load through a file reader, validate), `HexColor` | 002 |
| `sim/RettetDasEi.Sim/Rules/` | `GameState` (gold, shop, waves, phases), `GameEvent`, `WaveSchedule`, `Shop` (refund), `Reach` (range by height); `PestEffects`, `Jobs` (`Bomb`, `Glue`, `Shield`, `Pierce`) | 002, 003 |
| `tests/RettetDasEi.Sim.Tests/` | xUnit tests for the core; `TestContent` (the tests' own content); `ContentFilesTests` (the shipped files) | 002 |
| `tools/hooks/` | `check-on-change.sh` (Claude Stop hook), `pre-commit` (git) | — |
| `tools/tuning/` | `setup.sh`, `run.sh`, `SmokeHarness.cs` — fixed towers play the waves in a copy (`tuning-run/`, git-ignored) | — |

## Rules core (`sim/RettetDasEi.Sim/`, since M1 — ADR 002)

- Namespace `RettetDasEi.Sim`; Godot scripts say `using RettetDasEi.Sim;`. Don't name a core
  type like a Godot one (`Range`, `Colors`, `Timer`, …) — both namespaces are in scope there.
- **Warnings are errors** in the core (`TreatWarningsAsErrors`).
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
- **Events, not callbacks:** the core appends events (`GameEvent`); the Godot side drains them
  (`GameState.DrainEvents`) — `Main.ReadEvents` right after each call into the core, so a pest
  that is due appears in the same physics step, as in the prototype.
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
- **After any edit:** `dotnet test tests/RettetDasEi.Sim.Tests` — `ContentFilesTests` loads the
  real files and lists every problem (the Stop hook runs it, and boots the game headless too).
- A new content **file** needs its name in `ContentDb` (a constant, `Files`, a property read in
  `Load`) and in `TestContent.Load` — a JSON file the loader does not know fails a test.
- Tuning knobs and what they do:
  - **Toppling:** a pest's `push` (`enemies.json`) against a unit's `mass` and `friction`
    (`units.json`); `unitLinearDamp` / `unitAngularDamp` (`game.json`) calm the stack.
  - **Reach:** an attack's `range` plus `rangePerHeight` × pixels above the board (`game.json`).
  - **Economy:** `startGold`, a unit's `cost`, a pest's `gold`, a wave's `reward`, `sellRefund`.
  - **Pressure:** a wave's `groups` (`count`, `start`, `interval`, `side`); a pest's `speed`, `hp`,
    `damage`, `attackCooldown`.
  - **Shooters:** `damage`, `cooldown`, `speed`, `splash`, `recoil` (a recoil shoves the shooter
    itself — a tall light shooter can shoot itself off the tower), `pierce`.
  - **Stability (ADR 005):** a stack slides when the recoil a second (shots a second × `recoil`)
    passes grip × mass × 980, and tips over its base when that times the height passes mass ×
    980 × half the base's width. Food but cheese and egg grips at 0.5 on purpose. Measure with
    the tuning harness's `pepperweak` / `pepperstrong` (tilt and drift of the cheese) and `his`.
  - **The jobs (ADR 003):** the tomato's `bomb` (`damage`, `radius`, `push` — mind the egg: a
    light piece flies far), the cheese's `glue` (`tearImpulse` against the pests' `push`: below
    it a blow does not tear; `restSpeed`, `regrabSeconds`), the toast's `shield` (`pushShare`,
    `throwBack`), the mustard's `puddle` (`radius`, `seconds`, `slow` — the speed kept), the
    pepper's `itch` (`hits`, `pause`, `push`). Keep a `description` to about 40 characters: the
    shop card shows two lines of it.
- A new **required** field also goes into every entry of its file — and into `TestContent`, or
  every test fails at load, not at its assertion.

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

- `dotnet test tests/RettetDasEi.Sim.Tests` — no Godot needed, a few seconds (116 tests on
  2026-10-24). Assert exact outcomes with explicit set-up; prefer a tiny scenario that isolates one
  behaviour. Tests don't use the shipped content for logic (`TestContent`, its own small content),
  so tuning never breaks a logic test; `ContentFilesTests` loads the shipped files to validate them.
- The Godot side: `dotnet build game/RettetDasEi.csproj` and a headless boot —
  `<godot> --headless --path game --quit-after 300` must print no `ERROR` and no `Content is
  broken`. A boot stays in the build phase; to see a wave run without a person, a **scratch
  copy** of `game/` and `sim/` whose `Main` starts a wave by itself and prints every event, run
  with `--headless --fixed-fps 60 --quit-after 3600` (done for M1 on 2026-10-24) — never such a
  change in the repo. For M2 the copy also built a tower in `Main._Ready`, put its own wave into
  its `waves.json` and printed a line where each job fires (`[glue]`, `[bomb]`, `[shield]`,
  `[sneeze]`, `[pierce]`, `[puddle]`): a job that never prints was never tried.
- **Warnings are errors in the core:** a mutation that makes code unreachable (`if (false)`)
  does not build — no test runs, and a grep for "Bestanden" finds nothing. Mutate so it compiles.
- **Tuning the difficulty** (2026-10-24): `bash tools/tuning/setup.sh` copies `game/` and `sim/`
  to `tuning-run/` (git-ignored) and hooks `tools/tuning/SmokeHarness.cs` (a partial `Main`) into
  that copy — three hooks: build a fixed tower in `_Ready`, start every wave 2.5 s into a build
  phase, quit at won/lost. Then `cd tuning-run && bash run.sh <label> [towers]` plays each tower
  twice (~4 s a run) and prints the wave it was lost in, the gold after each wave, the largest
  crowd, a probe's tilt and drift, and how many fly bites went to the egg. Towers: `small` (115
  gold), `w3` (180, the most one can have before wave 3), `mid` (250), `bigless` (345), `big`
  (390) — flat, shooters on the board —, **`his`** (245, his winning tower of 2026-10-24: the
  reference for "only just", lesson `measure-difficulty-with-his-tower`), `pepperweak` /
  `pepperstrong` (ADR 005). The target he set: a careless build loses early, a good one wins
  wave 5 only just. Runs differ (physics): never tune a number on one run, and stop when a change
  shows nothing. Change content in `tuning-run/game/content`, measure, then copy the files into
  the repo and `git diff` them. If `setup.sh` says a hook did not apply, `Main.cs` or `Enemy.cs`
  changed where its `sed` lines look.
- **A mutation check** shows the tests can fail: break a rule on purpose (in a copy, or restore
  it with `git checkout -- <file>` once it is committed), see which tests turn red, put it back,
  see green again.
- **Read the number of tests after every run**, not only the word "Bestanden" (lesson
  `look-whether-the-file-is-there-before-writing-it`).
- No replay test of a whole wave: Godot's physics is not deterministic across runs (ADR 002).

## Hooks

- **Stop hook** (`.claude/settings.json` → `bash tools/hooks/check-on-change.sh`): when something
  under `game/scripts/`, `game/content/`, `sim/` or `tests/` differs from the last commit, it
  builds; when content changed it also boots the game headless for a moment and looks for content
  errors; then it runs the tests. Silent on green; a failure is printed (non-blocking).
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
- **A glued stack jumps or creeps while nothing touches it** → joints and the pair's own contact
  fight; `DisableCollision` must be true on the glue's pins (lesson
  `a-joint-and-a-contact-on-one-pair-fight`).
- **The tower jitters when tall** → more physics ticks or solver iterations, more damping
  (`game.json`) — measure one change at a time (ADR 001, consequences).

## Style

- Commits: imperative, scoped — `units: …`, `enemies: …`, `content: …`, `ui: …`, `sim(<area>): …`,
  `tests: …`, `tools: …`, `docs: …`. Direct to `main` while it's a solo project.
- Comments explain *why* and point at ADRs; the code says *what*.
- Wrap-up messages and progress entries are for a fresh reader: one plain sentence on what changed
  and why it matters for play, then evidence, then at most five lines of mechanism.
