# progress.md — Rettet das Ei

Living status document. A milestone counts as done only when its **verification gate**
(`roadmap.md`) is met and the proof is recorded here.

## State header

_The only mandatory session-start read from this file (CLAUDE.md ritual). Everything below is
on-demand._

- **Where we are:** M0 (the prototype) **closed 2026-10-24** — Sezgin played it: "Das Spiel ist
  cool", but every food feels the same, only the shapes differ. The project is set up like Chief
  Wants His Cut (architecture, roadmap, skills, hooks, git). ADR 002: a rules core with tests, the
  physics stays in Godot.
- **Next:** `/grill-me` proposal A (every food a job) → ADR 003 — no game code before it. Then M1
  (the rules core, no change in behaviour), then M2 (proposal A built).
- **Open live gates — none.**
- **Tests:** none yet (the test project comes with M1). Check now: `dotnet build` + the Stop
  hook's headless boot.
- **Git:** local repo on `main`, no remote yet (GitHub when he wants one).

## Current focus

**Before M1/M2: the grill of proposal A.** Käse klebrig, Toast federt zurück, Gurke durchschlägt,
Senf-Pfütze bremst, Pfeffer lässt niesen, Tomate platzt als Bombe — each rule split along ADR 002
(what is physics, what is the core's), its numbers, how it is seen without frames.

## Last session

- **2026-10-24 (second session)** — Read this project and Chief Wants His Cut (roadmap,
  architecture, CLAUDE.md, conventions, settings, hooks, all skills). Set up the same structure
  here: `architecture.md`, `roadmap.md` (M0–M4 with gates), `docs/conventions.md`,
  `docs/lessons/` (eleven lessons carried over as priors), `docs/decisions/README.md`, the seven
  skills rewritten for this game, `.claude/settings.json` with a Stop hook and a pre-commit hook
  (`tools/hooks/check-on-change.sh`: build; headless boot when content changed; tests once they
  exist — tried in a scratch copy: a broken JSON and a broken C# file are reported, a commit with
  a broken JSON is refused, a clean tree is silent). ADR 002 written after his "A". `git init`
  and the first commit.

## Next concrete step

Run `/grill-me` on proposal A with Sezgin (in German, one fork at a time), close it with ADR 003
and, if useful, a PRD in `docs/plans/`. Then build M1 (`roadmap.md`): the rules core project and
its tests, the existing rules moved without a change in behaviour.

## Decision log

Trivia and choices without an ADR live here; a row with "→ ADR" points to the full record.

| # | Decision | Choice | Rationale |
|---|---|---|---|
| D1 | Genre & theme | **Physics tower defense in a kitchen: stack food, save the egg** | Sezgin's pick from the brainstorm of 2026-10-24 → ADR 001 |
| D2 | Animation | **No frame animations; one still picture, motion from tweens and physics** | His two stalled games stalled on animation frames → ADR 001 |
| D3 | Engine & target | **Godot 4.6.2 .NET, C#, desktop** | The stack that works on his machine (Chief Wants His Cut) → ADR 001 |
| D4 | Physics | **Godot's built-in 2D physics, 120 Hz, 24 iterations; pests are no bodies** | Pests can't jam or be launched by the stack → ADR 001 |
| D5 | Content & art | **Balancing in validated JSON; art behind visual keys with stand-ins** | As in Chief Wants His Cut → ADR 001 |
| D6 | Code structure | **A rules core with tests (`sim/`), the physics in Godot** | Proposal A and B bring many branching rules; a full sim would mean own physics → ADR 002 |
| D7 | Workflow | **Chief Wants His Cut's**: state on disk, session ritual, ADRs, lessons, max 3 open live gates, seven skills, Stop + pre-commit hook | Sezgin, 2026-10-24: set it up like Chief Wants His Cut |
| D8 | Language | Docs/code English; the player reads German; chat German; GPT prompts English | As in Chief Wants His Cut; GPT follows English prompts better |
| D9 | Milestones | **M0 prototype → M1 rules core → M2 every food a job → M3 recipes → M4 look** | The core before the new rules, so they are built test-first; the look after the mechanics |

### Milestone → ADR map

| Milestone / area | Read before touching |
|---|---|
| Anything that moves, is drawn or animated | ADR 001 |
| The rules core, any rule (M1 on) | ADR 002 |
| Content / new entries | ADR 001 (point 5), ADR 002 (validation moves to the core) |
| M2 — every food a job | ADR 001, 002, and ADR 003 once the grill has written it |
| M4 — look | ADR 001; the look's own ADR once decided |

## Milestones

| Milestone | Status | Evidence |
|---|---|---|
| M0 — the prototype | **Closed 2026-10-24** | Built and played the same day; `dotnet build` green; his verdict: "Das Spiel ist cool. ABER jedes Essen fühlt sich gleich an, nur die Formen unterscheiden sich." |
| M1 — the rules core | Next | — |
| M2 — every food a job | Grill first | — |
| M3 — recipes | Later | — |
| M4 — look and feel | Later | — |

## Open questions

- **The look (M4):** keep the drawn stand-ins as the game's style, or GPT pictures? He likes the
  drawn figures (2026-10-24). Decide from candidates side by side → its own ADR.
- **The prototype's playtest questions** of 2026-10-24 that his verdict did not answer one by one:
  is stacking too slippery, sticky or jittery; does the tower topple too easily or never; is the
  egg on top too safe or too fragile; is "higher shoots further" a real temptation. Ask after M1's
  round, or let M2's grill take them in.

## Playtest notes

- **2026-10-24 — M0:** "Das Spiel ist cool." Every food feels the same, only the shapes differ →
  M2 (proposal A: every food a job). The figures drawn in code please him. A bug found and fixed in
  the other chat: the shop sat above the screen (`SetAnchorsPreset` alone; now `Hud.Anchor`).

## Ideas parked (not before Part 1 of the roadmap)

- Recipes (proposal B, M3): toast–cheese–toast = sandwich, tomato on cheese = pizza.
- Touch bonuses: a shooter standing on cheese deals more (Godot reports contacts already).
- Wind gusts, a ladle that sweeps the counter, a cat paw from above.
- A run of 20 waves with a boss every fifth, unlocks between runs.
- Moles-from-below equivalent for the kitchen: something that undermines the board.

## Log

- **2026-10-24** — Prototype 0.1 built from the brainstorm (Küche / "Rettet das Ei"): kitchen
  counter with a cutting board, egg as the king, shop with six foods (Käseblock, Toastbrot,
  Gurken-Kanone, Senfflasche, Pfefferstreuer, Kirschtomate), four pests (Ameise waddles,
  Fleischbällchen rolls, Fruchtfliege flies, Wackelpudding hops), five waves, drag and drop with
  live physics, rotate, sell, range by height, game over and win. Stand-in drawings in `DrawKit`;
  pictures plug in by visual key. ADR 001. Played by Sezgin the same day (M0 closed).
- **2026-10-24** — Project set up like Chief Wants His Cut; ADR 002; git (see Last session).
