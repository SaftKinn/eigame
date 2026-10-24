# progress.md — Rettet das Ei

Living status document. A milestone counts as done only when its **verification gate**
(`roadmap.md`) is met and the proof is recorded here.

## State header

_The only mandatory session-start read from this file (CLAUDE.md ritual). Everything below is
on-demand._

- **Where we are:** M0 and **M1 closed 2026-10-24** (M1: the rules core, his round "hat
  gepasst"). **M2 built the same day, play-unverified:** every food a job (ADR 003 with its
  addendum) — Käse glues, Toast throws back, Gurke pierces, Senf puddles, Pfeffer sneezes, Tomate
  bursts. Headless smoke runs fired every job; a fixed six-food tower wins all five waves —
  maybe too easy now (his second round: won with a big glued tower). Fixed after his rounds: a
  glued tower jumping by itself (the pair's collision off, measured calm); the cheese in red wax,
  only its cut face sticks, while facing up; "klebt hier" while holding; a picked-up piece snaps
  to the 15° step.
- **Next:** Sezgin plays M2 (the script in `## Next concrete step`); his words → a tuning round
  (`/playtest-triage`) or M2 closed. Then M3 (recipes, grill first).
- **Open live gates — one:** M2, every food a job (his round).
- **Tests:** 111, green (`dotnet test tests/RettetDasEi.Sim.Tests`).
- **Git:** local repo on `main`, no remote yet (GitHub when he wants one).

## Current focus

**M2 — every food a job** (`roadmap.md`, ADR 003): built; its live gate is his round. The first
numbers are guesses — expect tuning (all in `units.json`).

## Last session

- **2026-10-24 (M2)** — The six jobs built in one step (his pick). `5daa544`: content fields
  (`bomb`, `glue`, `shield` on units; `pierce` required, `puddle`, `itch` on attacks), their
  validation, the core's `PestEffects` and `Jobs` — 111 tests, a mutation check of three rules
  turned each red (a first try with `if (false)` did not build: warnings are errors). `72b0922`:
  the Godot side — `Main.Burst`, `GlueBonds` (two `PinJoint2D`, checked in GodotSharp 4.6.2),
  `Enemy.KnockBack` / `AddItch`, `Puddle`, piercing slices, the job's sentence on the shop card,
  the tomato's radius. Smoke runs in a scratch copy (a tower built by code, logs per job): glue
  sticks, puddles, pierces of 2 and 3, a burst hitting pests and shoving the cheese, a meatball
  sneezing; the toast throwing an ant (39 of 110 push) and a meatball (168 of 480) back; a
  meatball tearing the egg off the cheese (480 ≥ 400) and the egg falling — no errors. The five
  shipped waves against a fixed tower of all six (160 gold, placed for free): all won, no pest
  reached the tower.

- **2026-10-24 (M1)** — The rules core built, one seam at a time, a commit each: `df9ac84`
  (content: `ContentDb` loads through a file reader — `GameContent` in the game —, `HexColor` for
  Godot's colour check, 45 tests) and `c4c6024` (`GameState` with gold, shop, waves, phases and
  events; `WaveSchedule`, `Shop.Refund`, `Reach.Of`; `Main` reports and shows events; 76 tests).
  Checked before the move: Godot's `Mathf.RoundToInt` rounds half to even, `Color.HtmlIsValid`'s
  rule (both against GodotSharp 4.6.2 in a scratch program). A test compares every shipped wave
  with the prototype's queue code (pests at the same time keep their order). A mutation check
  turned 4 tests red. A headless smoke run of wave 1 (scratch copy that starts the wave and prints
  events): six ants from the right at 1.0–10.0 s, the egg broken at 22 s, lost once, no errors.
  His round: "hat gepasst" — M1 closed.

## Next concrete step

**Sezgin's round for M2** ("Spiel starten.bat") — what to look at, food by food:
1. **Laden:** each card says its job in a sentence — readable, and does it say enough?
2. **Käse:** red wax all round, only the yellow cut face on top sticks. Hold a piece over it — the
   cut face glows, "klebt hier"; let go, yellow crumbs: it sticks. A cheese stood upright or upside
   down sticks nothing. Does a glued tower stay still in the build phase? Pick the cheese up:
   cheese strings, what lay on it falls. In wave 2 a meatball's blow should tear a piece off.
   **Turning:** pick up a crooked piece — Q/E brings it back to exactly level or upright.
3. **Toast:** stand one at the front of the tower — an attacker flies back, the toast springs,
   the tower wobbles less.
4. **Gurke:** a slice goes through a row of ants. **Senf:** a yellow puddle; ants in it crawl.
   **Pfeffer:** a pest trembles more and more, then "Hatschi!", a hop back and a pause.
5. **Tomate:** hold it — the red circle. Lay it in the pests' way: "Platsch!", pests hurt, your
   food shoved (a tomato beside the egg is a risk).
6. **Overall:** does each food feel different now? Is one useless or a must? **Difficulty after
   the tuning:** waves 1–2 relaxed, from wave 3 the tower has to be good, wave 5 only just — and
   sometimes lost? Less gold now (100 to start, little per kill).
His words → `/playtest-triage` (tuning in `units.json`) or M2 closed; then M3 (recipes, `/grill-me` first).

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
| D13 | Building M2 | **A sneezing pest stands still; glue is two pins; the card shows the description** | What the grill left open, filled while building → ADR 003 addendum |
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
| M1 — the rules core | **Closed 2026-10-24** | 76 tests green; build 0 warnings; headless boot clean; smoke run of wave 1; his round: "hat gepasst" |
| M2 — every food a job | **Built 2026-10-24**, live gate open | 111 tests green; build 0 warnings; smoke runs fired every job; a fixed tower wins all five waves — his round open |
| M3 — recipes | Later | — |
| M4 — look and feel | Later | — |

## Open questions

- **The look (M4):** keep the drawn stand-ins as the game's style, or GPT pictures? He likes the
  drawn figures (2026-10-24). Decide from candidates side by side → its own ADR.
- **The prototype's playtest questions** of 2026-10-24 that his verdict did not answer one by one:
  is stacking too slippery, sticky or jittery; does the tower topple too easily or never; is the
  egg on top too safe or too fragile; is "higher shoots further" a real temptation. Ask with M2's
  round.
- **M2: difficulty.** He found it too easy ("ich gewinne eigentlich immer"); tuned the same day
  (Playtest notes). Open: is wave 5 now "nur knapp" for him? The cheese glue still makes the egg
  hard to knock off (only a meatball or the pudding tears it).

## Playtest notes

- **2026-10-24 — M2, third answer:** "es war zu leicht, also ich gewinne eigentlich immer" → his
  picks: both levers (harder waves and less gold), "Welle 5 nur knapp". Tuning round (measured with
  fixed towers in a scratch copy, see `docs/conventions.md`): startGold 120 → 100; pests' hp
  ant 22 → 40, meatball 70 → 150, fly 12 → 20, pudding 300 → 600; kill gold 4/9/5/40 → 1/4/2/15;
  rewards 30/40/50/60 → 20/25/30/40; waves denser and bigger (wave 1: 10 ants; wave 4 with a
  pudding; wave 5 two puddings, 24 ants, 14 flies, 8 meatballs). Before: a 115-gold tower won
  everything. After: 115 gold loses in wave 3, 250 in wave 4, 390 (about all the gold a game
  gives by wave 5) wins, 345 loses wave 5 two times in three. His towers are taller than the
  harness's flat ones — expect it a little easier for him.
- **2026-10-24 — M2, second round:** won all five waves with a big glued tower (screenshot).
  "Der Klebebalken gefällt mir doch nicht ganz, mach es wie vorher" → seam out. "Seitliche
  Verklebung soll doch nicht gehen … als wäre er in seiner Wachsschale" → wax rind, only the cut
  face sticks, while facing up. "Wenn man ein Bauteil aufhebt … wieder senkrecht ausgerichtet
  werden kann" → the held rotation snaps to the 15° step.
- **2026-10-24 — M2, first round:** "manchmal springt der Turm … die Physik passt nicht immer.
  Vorher ist auch mal der Turm in die Höhe geflogen." His recording: a glued cheese tower
  wobbling and sliding by itself in the build phase → the glue's pins fought the pair's contact
  (measured), fixed. "Ich will sehen, wenn der Käse sich verbindet … es soll sichtbar sein, dass
  sich das verbinden wird" → a seam and a "klebt hier" preview. Side glue: "vergiss das … es
  würde das Spiel aktuell nur noch leichter machen". Not yet answered: whether "vorher" (flying
  up) was before M2's glue.
- **2026-10-24 — M1:** "hat gepasst" — nothing felt different.
- **2026-10-24 — M0:** "Das Spiel ist cool." Every food feels the same, only the shapes differ →
  M2 (proposal A: every food a job). The figures drawn in code please him. A bug found and fixed in
  the other chat: the shop sat above the screen (`SetAnchorsPreset` alone; now `Hud.Anchor`).

## Ideas parked (not before Part 1 of the roadmap)

- Cheese that glues at its sides too ("vertikal", 2026-10-24) — put off by him: it would only
  make the game easier now.
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
- **2026-10-24** — M1 built: the rules core with 76 tests; played: "hat gepasst".
- **2026-10-24** — M2 built: every food a job, 111 tests (see Last session).
