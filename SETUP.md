# SETUP.md — getting Rettet das Ei running

Steps a human does once. The same tools as *Chief Wants His Cut*, so on Sezgin's machine they are
already there.

## 1. Install (already done for Chief Wants His Cut)

1. **.NET SDK 8** (`winget install Microsoft.DotNet.SDK.8`). Check: `dotnet --version`.
2. **Godot 4.6.2 — the .NET build** (`winget install GodotEngine.GodotEngine.Mono`).
3. The first line of `game/RettetDasEi.csproj` says `Godot.NET.Sdk/4.6.2` — it must be exactly the
   editor's version (title bar / Help → About).

## 2. First run

**The easy way:** double-click **`Spiel starten.bat`** in the repo root. It builds the C# code,
imports the pictures and starts the game. If something goes wrong, the reason is in
`logs\dotnet-build.log`, `logs\import.log` or `logs\run.log` — send those to Claude.

**In the editor:**
1. Start Godot → **Import** → select `game/project.godot`.
2. Build once: hammer icon top right (or **Alt+B**).
3. **F5** runs the game.

Optional, from the repo root in a terminal: `dotnet build game/RettetDasEi.csproj` builds without
the editor.

## 2b. After a fresh clone (git)

Once per clone, in the repo root: `git config core.hooksPath tools/hooks` — this turns on the
check before every commit (it builds, and boots the game headless when content changed). The
repository on this machine has it already (2026-10-24).

## 3. How to play

- **Laden** (bottom): press on a card and drag the food into the kitchen. Let go to drop it.
- **Move** placed food between waves: click it and drag. Food above it falls — the physics is on.
- **Rotate** what you hold: mouse wheel or **Q** / **E**.
- **Sell**: drag a piece back onto the shop. Still in the hand right after buying: full price;
  placed once: price × its remaining health.
- Red while dragging = it overlaps something; let go anyway and it pops up on top and drops.
- **Space** (or the button) starts the wave. The food shoots by itself; higher food reaches further
  (the yellow band on the counter shows the reach while you hold a shooter).
- **Every food has a job** (its shop card says it): **Käse** glues what touches it, on every side
  ("klebt hier" shows while you hold a piece close to it; let go, it snaps flush) — a hard blow
  tears it off; a right click on its shop card switches it to the **Käsedreieck**; pieces turn in
  15° steps and a piece you pick up snaps straight, so Q/E can always turn it level again; **Toast** throws attackers back and is shoved less; **Gurke**'s slices go
  through three pests; **Senf** leaves a puddle that slows walkers; **Pfeffer** makes pests sneeze
  and pause; **Tomate** shoots nothing — it bursts when a pest touches it and shoves your food
  (the red circle shows how far while you hold it).
- **The egg**: lose when it breaks or touches the counter (Theke). On the cutting board (Brett) or on
  the tower it is fine. A wave can't start while it lies on the counter.
- **F** doubles the speed, **R** restarts.

## 4. Pictures from GPT

Put a PNG named after the unit's or pest's `visual` key into `game/art/units/` or
`game/art/enemies/` — it replaces the stand-in drawing at the next start, no code involved. The
prompts and sizes are in `docs/art-prompts.md`. After adding pictures, open the editor once so
Godot imports them.

## Open items (human-only)

- [x] Play the prototype (2026-10-24: "cool", every food feels the same → M2)
- [x] `git init` (2026-10-24)
- [x] A GitHub repository as the remote: `origin` = https://github.com/SaftKinn/eigame (2026-10-24)
