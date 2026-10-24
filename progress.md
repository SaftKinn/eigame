# progress.md — Rettet das Ei

Living status document. A milestone counts as done only when its **verification gate**
(`roadmap.md`) is met and the proof is recorded here.

## State header

_The only mandatory session-start read from this file (CLAUDE.md ritual). Everything below is
on-demand._

- **Where we are:** M0 (the prototype) **closed 2026-10-24** — Sezgin played it: "Das Spiel ist
  cool", but every food feels the same, only the shapes differ. The project is set up like Chief
  Wants His Cut (architecture, roadmap, skills, hooks, git). ADR 002: a rules core with tests, the
  physics stays in Godot. **ADR 003** (grilled the same day): every food a job — not built.
  **M1 built 2026-10-24:** content, gold, shop, waves, phases and range live in the core
  (`sim/`), play-unverified.
- **Next:** Sezgin plays one round of M1 (the script in `## Next concrete step`) — nothing may
  feel different. Then M2 (all six jobs of ADR 003 in one step, his pick).
- **Open live gates — one:** M1, "nothing changed" (his round).
- **Tests:** 76, green (`dotnet test tests/RettetDasEi.Sim.Tests`).
- **Git:** local repo on `main`, no remote yet (GitHub when he wants one).

## Current focus

**M1 — the rules core** (`roadmap.md`, ADR 002): built; its live gate is his round. **Then M2** —
ADR 003 in one step.

## Last session

- **2026-10-24 (M1)** — The rules core built, one seam at a time, a commit each: `df9ac84`
  (content: `ContentDb` loads through a file reader — `GameContent` in the game —, `HexColor` for
  Godot's colour check, 45 tests) and `c4c6024` (`GameState` with gold, shop, waves, phases and
  events; `WaveSchedule`, `Shop.Refund`, `Reach.Of`; `Main` reports and shows events; 76 tests).
  Checked before the move: Godot's `Mathf.RoundToInt` rounds half to even, `Color.HtmlIsValid`'s
  rule (both against GodotSharp 4.6.2 in a scratch program). A test compares every shipped wave
  with the prototype's queue code (pests at the same time keep their order). A mutation check
  turned 4 tests red. A headless smoke run of wave 1 (scratch copy that starts the wave and prints
  events): six ants from the right at 1.0–10.0 s, the egg broken at 22 s, lost once, no errors.

- **2026-10-24 (second session)** — Read this project and Chief Wants His Cut (roadmap,
  architecture, CLAUDE.md, conventions, settings, hooks, all skills). Set up the same structure
  here: `architecture.md`, `roadmap.md` (M0–M4 with gates), `docs/conventions.md`,
  `docs/lessons/` (eleven lessons carried over as priors), `docs/decisions/README.md`, the seven
  skills rewritten for this game, `.claude/settings.json` with a Stop hook and a pre-commit hook
  (`tools/hooks/check-on-change.sh`: build; headless boot when content changed; tests once they
  exist — tried in a scratch copy: a broken JSON and a broken C# file are reported, a commit with
  a broken JSON is refused, a clean tree is silent). ADR 002 written after his "A". `git init`
  and the first commits. Then the grill of proposal A (nine questions, all answered) → ADR 003.
  The new skills could not be called by name in that session (Claude Code loads them at start);
  the grill followed `grill-me/SKILL.md` by hand.

## Next concrete step

**Sezgin's round for M1** ("Spiel starten.bat") — does anything feel different from before?
1. Start: 120 Gold, the egg on the board, the hint at the top.
2. Buy a Käseblock and sell it straight back from the hand: 120 Gold again. Place one, then sell
   it: you get its price × its health back.
3. Push the egg onto the counter and press Space: the wave does not start, the red warning shows.
4. Build a tower, egg on top, play waves 1–5: "Welle N!", pests from the right/both sides as
   before, "+N" over a killed pest, "Welle N geschafft! +N Gold", and at the end "Gewonnen!".
5. One round where the egg falls or breaks: "Das Ei ist kaputt!" with the right reason and count.
His answer closes M1's live gate (or becomes a fix round); then M2 — ADR 003 in one step.

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
| D10 | Tomate | **A bomb, no longer a shooter: bursts at any pest's touch; pushes own food, damages none** | Its rolling becomes the job; risk beside the egg → ADR 003 |
| D11 | Käse, Toast | **Cheese glue that a hard blow tears; toast a shield wall (throw-back, a share of the push)** | Clearly felt jobs for the two blocks → ADR 003 |
| D12 | Gurke, Senf, Pfeffer; teaching; order | **Pierce N; less splash + a puddle; an itch to N hits; the card's sentence; all six in one step** | His picks in the grill of 2026-10-24 → ADR 003 |

### Milestone → ADR map

| Milestone / area | Read before touching |
|---|---|
| Anything that moves, is drawn or animated | ADR 001 |
| The rules core, any rule (M1 on) | ADR 002 |
| Content / new entries | ADR 001 (point 5), ADR 002 (validation moves to the core) |
| M2 — every food a job | ADR 003, then 001 and 002 |
| M4 — look | ADR 001; the look's own ADR once decided |

## Milestones

| Milestone | Status | Evidence |
|---|---|---|
| M0 — the prototype | **Closed 2026-10-24** | Built and played the same day; `dotnet build` green; his verdict: "Das Spiel ist cool. ABER jedes Essen fühlt sich gleich an, nur die Formen unterscheiden sich." |
| M1 — the rules core | **Built 2026-10-24**, live gate open | 76 tests green; build 0 warnings; headless boot clean; smoke run of wave 1 in a scratch copy — his round open |
| M2 — every food a job | Grilled → ADR 003; built after M1 | — |
| M3 — recipes | Later | — |
| M4 — look and feel | Later | — |

## Open questions

- **The look (M4):** keep the drawn stand-ins as the game's style, or GPT pictures? He likes the
  drawn figures (2026-10-24). Decide from candidates side by side → its own ADR.
- **The prototype's playtest questions** of 2026-10-24 that his verdict did not answer one by one:
  is stacking too slippery, sticky or jittery; does the tower topple too easily or never; is the
  egg on top too safe or too fragile; is "higher shoots further" a real temptation. Ask after M1's
  round.
- **M2's damage:** with the tomato a bomb and the mustard weaker, is there still enough steady
  damage to clear the waves? First thing to watch in M2's round (ADR 003, consequences).

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
- **2026-10-24** — Project set up like Chief Wants His Cut; ADR 002; git; proposal A grilled →
  ADR 003.
- **2026-10-24** — M1 built: the rules core with 76 tests (see Last session).
