# progress.md — Rettet das Ei

Living status document. A milestone counts as done only when its **verification gate**
(`roadmap.md`) is met and the proof is recorded here.

## State header

_The only mandatory session-start read from this file (CLAUDE.md ritual). Everything below is
on-demand._

- **Where we are:** M0 and **M1 closed 2026-10-24** (M1: the rules core, his round "hat
  gepasst"). **M2 built the same day, play-unverified:** every food a job (ADR 003 with its
  addendum) — Käse glues, Toast throws back, Gurke pierces, Senf puddles, Pfeffer sneezes, Tomate
  bursts. Rounds with him since: a glued tower jumping by itself (fixed: the pair's collision
  off); the cheese in red wax, only its cut face sticks — and since 2026-10-26 no wax, **every
  face sticks** (D17, play-unverified); "klebt hier" while holding; a picked-up
  piece snaps to the 15° step; "too easy" → harder waves and less gold, aimed at "wave 5 only
  just"; **ADR 004:** pests gang up (a heap), never block each other, flies hunt the egg round
  the food; **ADR 005:** food but cheese slippery, real recoil — a high stack needs a base.
- **Next:** Sezgin plays (he will, later — the script in `## Next concrete step`); his words → a
  tuning round (`/playtest-triage`) or M2 closed. Likely next tuning: waves 4–5 measured against
  his own tower (`tools/tuning`, tower `his`), not the flat ones. Then M3 (recipes,
  grill first).
- **Open live gates — one:** M2, every food a job, with the difficulty, ADR 004 and ADR 005
  (his round). When a playtest is due, start the game for him (CLAUDE.md).
- **Tests:** 122, green (`dotnet test tests/RettetDasEi.Sim.Tests`).
- **Git:** `main`, remote `origin` = github.com/SaftKinn/eigame (since 2026-10-24). The checkout's
  folder differs from session to session (2026-10-25 `Desktop\Projekte\EggGame`, 2026-10-26
  `Desktop\Claude\rettet-das-ei`): take the folder you are in, never the one written here.

## Current focus

**M2 — every food a job** (`roadmap.md`, ADR 003): built; its live gate is his round. The first
numbers are guesses — expect tuning (all in `units.json`).

## Last session

- **2026-10-26 (his round begins)** — Pulled; the checkout is `Desktop\Claude\rettet-das-ei` again
  (`EggGame` does not exist on this machine). Started the game for his M2 round; his first word:
  "es soll doch jede seite des käßes kleben entferne den roten rand" → asked: all round, sides
  too. `GlueBonds`: any touching piece sticks, the pins on the face they touch at (`Face`),
  two cheeses one bond; "klebt hier" on the nearest face; `DrawKit` cheese bare as before the
  wax (D17, ADR 003 addendum). Then "seitlich muss es magnetisch angezogen werden" → asked
  (snap on letting go / a pull in the physics): the snap — `GlueBonds.SideSnap` from
  `Main.Drop`, sticks at once (D18). His screenshot: no snap beside an upright cheese, the
  reach too far → faces and gaps on the real outlines (`UnitBody.Outline`), 12 px, the snap on
  every face. The Käsedreieck (D19: `cheese_triangle`, `alternate`, `ContentDb.ShopUnits` with
  6 new tests, a right click on the card) and 45° turning (D20). His screenshot of a cheese
  held across two upright ones, one strip glowing → every pair in reach glows and sticks
  (`StickPairs`). Build green (0 warnings), 122 tests, headless boot clean; play-unverified.

- **2026-10-25 (fresh checkout)** — The repo cloned anew into `Desktop\Projekte\EggGame` (the
  old folder `Desktop\Claude\rettet-das-ei` is gone); the lesson `close-only-this-games-process`
  now matches `EggGame`. The game started for his M2 round via "Spiel starten.bat": build green
  (0 warnings), first import fine, no errors in `run.log`. He asked which pests there are (four —
  answered from `enemies.json`); no playtest words yet, so M2's gate stays open. No code changed.

- **2026-10-24 (M2's rounds with him)** — Six answers of his turned into rounds, each measured,
  committed and the game started for him (details in `## Playtest notes`): the glue's jumping
  (pins vs contact, measured from his recording, lesson `a-joint-and-a-contact-on-one-pair-fight`);
  the cheese in wax, only its cut face sticks; a held piece snaps to 15°; harder waves and less
  gold; ADR 004 (a crowd, no blocking at all, flies hunt the egg); ADR 005 (slippery food, real
  recoil). A tuning harness (`tools/tuning`) plays fixed towers through the waves
  (`docs/conventions.md`); his own winning tower rebuilt in it showed the flat towers
  underestimate a good builder (lesson `measure-difficulty-with-his-tower`). Remote added and
  pushed.

## Next concrete step

**Sezgin's round for M2** ("Spiel starten.bat") — what to look at, food by food:
1. **Laden:** each card says its job in a sentence — readable, and does it say enough?
2. **Käse:** no wax any more — every side sticks (D17). Hold a piece near it — the face it would
   stick to glows, "klebt hier"; let go, yellow crumbs: it sticks — on top, below, beside, and a
   cheese beside a cheese. Within 12 px of a face it snaps flush when let go (D18), a held
   cheese too. **Käsedreieck:** right click on the cheese card → the triangle (D19); it glues on
   all three faces; point up and point down fit. Turning is in 45° steps now (D20). Does a glued tower stay still in the build phase, sides glued too? Is
   it now too easy? Pick the cheese up:
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
   sometimes lost? Less gold now (100 to start, little per kill). **Wave 3 above all** — the
   harness lost it even with all the gold one can have by then.
7. **Stability (ADR 005):** without cheese, does a high stack slide? Do shooters jump back when
   they fire? Do several peppers glued high on a cheese tip a tower on a weak base (a toast)
   and not on a strong one (a cheese on a cheese)? Is the cheese worth buying now?
8. **Pests (ADR 004):** nobody waits behind anybody any more — does a sneezing or thrown-back pest
   get passed? Do walkers pile up at the tower and attack together (a heap)? Do flies fly
   round the food to the egg — and bite their way in when it is built in? Is an egg on top now
   too easy a target?
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
| D14 | Pests together | **A crowd: walkers push in behind attackers and attack together, drawn as a heap** | His "nicht immer nur einer"; climbing was the costly alternative → ADR 004 |
| D15 | Flies | **Hunt the egg round the food; after `flyPatience` (2 s) bite what blocks them** | His wish; a built-in egg is worth building → ADR 004 |
| D17 | Käse | **Every face sticks (top, bottom, sides); the red wax rind gone** | His word at the start of his round, 2026-10-26: "jede seite"; asked → all round, sides too. Since ADR 005 the cheese is the one thing that holds a tower → ADR 003 (addendum) |
| D18 | Käse's sides | **A magnet as a snap: let go beside a cheese's side, the held one moves flush and sticks at once** | His "sonst fällt … einfach runter"; a pull in the physics all the time would tug towers apart → ADR 003 (addendum) |
| D19 | Käsedreieck | **The cheese block's other shape: an equilateral triangle, one shop card, a right click switches** | His wish ("linksklick" buys, so a right click); three 120° angles cannot be — asked → equilateral. `alternate` in units.json, `ContentDb.ShopUnits` → ADR 003 (addendum) |
| D20 | Turning | **45° steps (was 15°)** | "die anderen winkel wären sonst überflüssig"; the triangle lies flat point up or down |
| D16 | Stability | **Food but cheese and egg grips at 0.5; recoil pepper 200, pickle 150, mustard 120** | Height was free and the cheese pointless; his idea: recoil tips a stack on a weak base → ADR 005 |
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
| Pests' behaviour (crowd, flies) | ADR 004, then 001 |
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
- **M2: difficulty for a good builder.** His compact tower (245 gold, rebuilt as `--tower=his`)
  wins all five waves in the harness and in his game, with gold to spare; the flat harness towers
  lose early. The harness's measure of "only just" should be his tower, not the flat ones —
  harder waves 4–5 until it loses wave 5 sometimes? Ask after his round with ADR 005.

## Playtest notes

- **2026-10-26 — M2, third round (start):** "es soll doch jede seite des käßes kleben entferne
  den roten rand 'schale'" — asked (all round or whichever face is up): "Ringsum, auch
  seitlich" → D17, the wax gone. He had put side glue off twice on 2026-10-24; ADR 005 since.
  Then "seitlich muss es magnetisch angezogen werden sonst fällt der käse einfach runter oder
  das bauteil das man drankleben will" → "Schnappen beim Loslassen" (D18). Then his screenshot
  (an upright cheese beside a flat one, not stuck): "so geht es noch nicht nur übereinander
  schnappt er … man soll präzise bauen können" → the face judged by the centres called it "on
  top", and the reach was a circle round the piece (huge for a long one); now edge to edge,
  12 px reach. Then "auf ein käsedreieck umschalten … 120 grad pro winkel" → asked: an
  equilateral triangle, a right click on the shop card (D19). Then "alle bauteile auch nur um
  45 grad rotieren" (D20).

- **2026-10-24 — M2, sixth answer:** won again with a compact tower (cheese base, the egg upright
  between four peppers, a cheese roof, two pickles on top, a toast beside; screenshot) without
  buying for wave 5. "Der Käseblock ist überflüssig geworden, da man sich ja auch so in die Höhe
  bauen kann." His idea: real recoil that tips a high stack on a weak base → ADR 005 (slippery
  food, recoil ×5–30). His tower rebuilt in the harness (`--tower=his`, 245 gold): wins every run,
  before and after. Open: the waves for a good builder.
- **2026-10-24 — M2, fifth answer:** "wenn der Pfefferstreuer einen anschießt und er zurück
  genockt wird, sollte eine andere Ameise vorgehen können" — then: "es soll allgemein kein Block
  geben" → walkers never block each other (ADR 004 addendum). Retuned: waves about a quarter
  smaller, rewards 25/35/40/50. Measured: 115 gold loses wave 3, 250 wave 4, 345 wave 5, 390
  wins one run in two.
- **2026-10-24 — M2, fourth answer:** "Gegner sollen sich stacken und angreifen, nicht immer nur
  einer. Außerdem sollen die Fliegen um Bauteile rumfliegen und direkt das Ei angreifen" → his
  picks: a crowd (not climbing); flies bite what blocks them after ~2 s → ADR 004. Measured: crowds
  of six ants; flies bite the egg whenever they get through; 115 gold loses wave 3, 250 wave 4,
  345 wave 5, 390 wins. Wave 3 softened (6 flies, 14 ants). A 180-gold tower still lost wave 3 in
  the harness — watch it. A lower ant push (80) was tried and changed nothing; kept at 110.
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
