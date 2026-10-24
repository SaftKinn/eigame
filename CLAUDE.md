# CLAUDE.md

Instructions for Claude Code in this repository. Read first, every session.

## Session ritual

This project runs over many sessions, across different models and effort levels. To survive
context clears and model switches, state lives on disk.

**At the start of every session, read in this order:**
1. This file (`CLAUDE.md`) — conventions
2. ONLY the `## State header` at the top of `progress.md`
3. The highest-numbered file in `docs/decisions/` — the most recent decision
4. `docs/lessons/README.md` — skim the one-line lesson summaries; open a full lesson only when its
   summary touches the task

Then say in two or three sentences (German) where we are and what we're about to do. Don't touch
files until that handshake is done. When you have enough to act, act — don't re-derive
established facts or re-litigate decided ADRs.

**Before touching an area or starting a milestone:** the milestone → ADR map in `progress.md`
says which ADR(s) govern it — read those **before** implementing.

**WIP limit: max 3 open live gates.** A round that would open a fourth doesn't start — the next
session is a playtest session; say so in the handshake. Details and the "WIP-Override" exception
are in the `session-ritual` skill.

**Read on demand, NOT every session:**
- `architecture.md` — when the task touches design; skim the relevant section
- `roadmap.md` — at a milestone transition, or when asked "what's the goal of M2?"
- `docs/conventions.md` — when working in an area (content, Godot scripts, the rules core) or on
  testing, hooks, runtime, troubleshooting, commit style; also holds the **repo layout** with the
  governing ADR per folder
- `SETUP.md` — install, run, controls — point Sezgin at it, the agent can't click through the
  Godot editor
- `docs/art-prompts.md` — when pictures are made
- Older ADRs in `docs/decisions/` — when working in the area they cover
- Individual files in `docs/lessons/` — when their summary in the lessons README matches the task

**While working:**
- Label anything not playtested as **play-unverified**. A done-claim rests on a tool result from
  this session (`dotnet build`, a headless boot, from M1 `dotnet test`). "It compiles" is not
  "it plays well".
- When Sezgin describes a problem or thinks out loud, the deliverable is your assessment — report
  and stop; don't build until asked. For a real fork, lay out 2–3 options with a recommendation.
  Pause only for: destructive actions, real scope changes, a playtest only a human can run, or a
  design fork worth an ADR. **An ADR is written only after he has agreed to the decision.**
- **Sezgin is a beginner:** explain simply, one testable step at a time, complete runnable code.
- **When a round ends with a playtest for him, start the game automatically** — don't ask (his
  word, 2026-10-24). Close only this game's process if one runs (lesson
  `close-only-this-games-process`), start `Spiel starten.bat` with PowerShell `Start-Process`
  (Git Bash splits the name at the space), wait for the Godot window, read `logs\`, then give the
  play script.
- **Test-first only where it pays** (from M1): rules in the core with branches or an invariant get
  their tests first; Godot scripts, physics feel, drawings and content tuning are verified by the
  build, a headless boot and Sezgin playing. Details in the `tdd` skill.
- **Record lessons as they happen.** When a correction recurs or an approach is confirmed the hard
  way, write it to `docs/lessons/` (one file per lesson, one line into the README index) in the
  same round. Don't record what this file, `docs/conventions.md` or an ADR already holds.

**At the end of every working session, without being asked** (and on `wrap up` /
`update progress`): run the `session-ritual` skill — `progress.md` update, rotation caps, WIP
check, a check of every `.md` file, ADR scaffold when a decision was made, and finally commit (+
push once a remote exists).

## What this project is

**Rettet das Ei** (working title; `RettetDasEi` in code and folders): a physics tower defense in
**Godot 4.6 + C#**, desktop/Steam. Between waves the player buys food from a shop and stacks it by
hand into a tower on a cutting board; the physics runs the whole time, so a careless stack topples
while it is built. During a wave the food shoots by itself at kitchen pests — ants, meatballs, fruit
flies, a jelly pudding — that walk, roll, fly and hop in from both sides and shove the tower. The
egg must survive: it breaks when its health is gone or when it touches the counter. Higher food
shoots further, which tempts the player to build tall.

**The founding decision (ADR 001): no frame animations, ever.** Every unit and pest is one still
picture; all motion is code — tweens that squash, lean and snap, and the physics itself. An attack
is wind-up → lunge → recoil, and the tower's wobble under the blow is the real animation. Don't
add sprite sheets or AnimationPlayer frame tracks; if something looks stiff, give it a better tween
or more juice (flash, crumbs, shake, numbers), not frames. *Which* still pictures — the drawn
stand-ins Sezgin likes, or GPT pictures — is open (M4, its own ADR).

**The rules core (ADR 002, built in M1):** rules go into a pure C# library with tests; the physics
stays in Godot. Godot reports facts (what touches what, where), the core answers what follows
(how much, how long, whether).

**Where it stands:** the prototype (M0) was played on 2026-10-24 — "das Spiel ist cool", but every
food feels the same. M1 (the rules core) is closed; M2 (every food a job — ADR 003: the tomato a
bomb, cheese glue, toast a shield wall, piercing pickle, mustard puddle, pepper sneeze) is built
and waits for his round; then M3 (recipes). Plan in `roadmap.md`, design in `architecture.md`. **If a design decision is unclear,
`architecture.md` wins — and if you change a decision, update `architecture.md` in the same
change.**

German names used everywhere: **Ei**, **Brett** (cutting board, the safe zone), **Theke** (the
counter), **Welle**, **Laden**, **Gold**, **Reichweite**.

> Language convention: docs and code are **English**. Everything the player reads is **German**.
> Chat with Sezgin is German. **Prompts for GPT (pictures) are always English** (his wish: GPT
> follows them more reliably).

## Golden rules

1. **Read before write.** Grep/read the relevant script before editing.
2. **Physics in Godot, rules in the core** (ADR 002). Units are `RigidBody2D`s; pests are not
   bodies — they ask the physics what they touch (`Main.UnitTouching`) and push with impulses. Read
   the space state only in `_PhysicsProcess`. From M1, a Godot script decides *where and what
   touches*, never *how much, how long or whether* — if you're about to write an `if` about gold,
   damage, a duration or a count in a Godot script, it belongs in the core.
3. **Balancing is data.** Every number that is tuning, not logic, lives in `game/content/*.json`
   (units, enemies, waves, global tuning). A new unit, pest or wave is a JSON entry. Content is
   validated at load (`ContentDb.Validate`) and fails loud — never add a silent default.
4. **Art is behind visual keys.** Content says `"visual": "<key>"`. `art/units/<key>.png` or
   `art/enemies/<key>.png` is used if it exists, else the stand-in drawing in `View/DrawKit.cs`.
   Unknown keys draw **magenta** and warn once. An art swap touches no code.
5. **One function per rule.** A value computed in two places will desync (lesson
   `parity-by-construction`): the shooter and the range overlay, the refund hint and the payment
   call the same function.
6. **Milestone by milestone, with a gate.** A milestone is done when its verification gate in
   `roadmap.md` is met and the proof is in `progress.md`. Don't start M(n+1) mechanics inside M(n).
   Progression, menus and saves wait until Part 1 of the roadmap is played.
7. **No new NuGet dependency without a note** in the commit message and `architecture.md` §4.

## Key gotchas

Broad footguns. Area conventions, testing, hooks, runtime and troubleshooting live in
**`docs/conventions.md`**.

- **Godot SDK version = editor version.** `Godot.NET.Sdk/<version>` in `game/RettetDasEi.csproj`
  must equal the installed editor version exactly (4.6.2), or Godot refuses to build.
- **No implicit usings on the Godot side** — add `using System;` / `using System.Collections.Generic;`.
- **`FileAccess` is ambiguous** next to `System.IO` — alias `using FileAccess = Godot.FileAccess;`.
  Read `res://` files only via Godot's `FileAccess`.
- **The JSON in `game/content/` must be exported**: when an export preset is made, add `content/*.json`
  to its non-resource files filter, or the exe starts without content.
- **UI built in code: never `SetAnchorsPreset` alone.** It keeps the control where it was and
  recomputes the offsets, so a fresh control stays at size zero (on 2026-10-24 the whole shop sat
  above the screen). Use `Hud.Anchor` — anchors and all four offsets together.
- **Windows runtime.** Sezgin runs Windows. Hooks are bash (Git Bash); keep `.sh` files and
  `tools/hooks/*` LF (`.gitattributes` enforces it). Texts with umlauts go through the Edit tool,
  never PowerShell or sed (lessons `powershell-5-eats-quotes-and-encodings`, `sed-in-git-bash-drops-crlf`).
- **Hooks:** the Stop hook (`tools/hooks/check-on-change.sh`) builds and tests when
  `game/scripts/`, `game/content/`, `sim/` or `tests/` changed, and boots headless for content;
  silent on green. `pre-commit` is the same, blocking. Never `--no-verify`.
- **Tests:** `dotnet test tests/RettetDasEi.Sim.Tests` — no Godot, seconds; read the count.
- **The core's namespace is `RettetDasEi.Sim`** — don't name a core type like a Godot one
  (`Range`, `Colors`, …): Godot scripts have both in scope.
- **Build:** `dotnet build game/RettetDasEi.csproj` (or Alt+B in the editor). Sezgin starts the
  game with **"Spiel starten.bat"** (logs in `logs\`).
- **Commit messages:** imperative, scoped — `units: …`, `enemies: …`, `content: …`, `ui: …`,
  `sim(<area>): …`, `tests: …`, `tools: …`, `docs: …`.
