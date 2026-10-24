# Rettet das Ei

*Working title; `RettetDasEi` in code.* A physics tower defense in Godot 4.6 + C#: stack food into a
wobbly tower on a cutting board, and keep the egg on top alive while ants, meatballs, fruit flies
and a jelly pudding storm the kitchen counter and shove at the tower.

No frame animations anywhere — one picture per unit, all motion from code and physics
([ADR 001](docs/decisions/001-physics-tower-no-frames.md)). Rules go into a tested core, the
physics stays in Godot ([ADR 002](docs/decisions/002-rules-core-physics-in-godot.md)).

## Status

Prototype 0.1 (2026-10-24): one kitchen, six kinds of food, four pests, five waves, shop, drag and
drop, sell, rotate, game over and win; stand-in drawings. **Played 2026-10-24** — fun, but every food
feels the same. The rules core with its tests (M1) is done; every food a job (M2: glue, shield,
pierce, puddle, sneeze, bomb) is built and being played. See
[`progress.md`](progress.md) and [`roadmap.md`](roadmap.md).

## Getting started

See [`SETUP.md`](SETUP.md). Short version: double-click **`Spiel starten.bat`** — or Godot 4.6
**.NET**, import `game/project.godot`, Alt+B, F5.

| Folder / file | What |
|---|---|
| `game/` | Godot project — scene, scripts, content JSON |
| `game/content/` | all balancing: `units.json`, `enemies.json`, `waves.json`, `game.json` |
| `game/art/units/`, `game/art/enemies/` | pictures by visual key (see `docs/art-prompts.md`) |
| `game/scripts/` | `Main.cs` (the flow), `Units/`, `Enemies/`, `Combat/`, `Ui/`, `View/DrawKit.cs` (stand-in drawings) |
| `sim/`, `tests/` | the rules core (content, gold, waves) and its tests — `dotnet test tests/RettetDasEi.Sim.Tests` |
| `architecture.md`, `roadmap.md` | the design as built, the milestones with their gates |
| `docs/` | decisions (ADRs), lessons, conventions, the GPT prompts |
| `tools/hooks/` | the check after every change (Claude Stop hook) and before every commit |
| `.claude/` | Claude Code settings and the project skills |
