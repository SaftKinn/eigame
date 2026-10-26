# progress.md — Rettet das Ei

Living status document. A milestone counts as done only when its **verification gate**
(`roadmap.md`) is met and the proof is recorded here.

## State header

_The only mandatory session-start read from this file (CLAUDE.md ritual). Everything below is
on-demand._

- **Where we are:** M0 and **M1 closed 2026-10-24**. **M2 (every food a job, ADR 003) built
  2026-10-24, in his rounds since.** ADR 004 (pests gang up, flies hunt the egg), ADR 005
  (slippery food, real recoil). **2026-10-26, his third round — the building hand reworked
  (D17–D24, ADR 003 addendum), all play-unverified:** the cheese sticks on every face (no wax),
  a 12 px magnet snaps a piece flush, every glue spot in reach glows; the **Käsedreieck** (right
  click on the cheese card); lifting a piece lifts its glued group (right click in the hand
  unglues it, G = glue off, not for cheese); a **shelf** under the window where pieces sleep;
  only what the hand lets go sticks by resting; a hit no longer pushes a pest back (only sneeze
  and toast). He won all five waves before D23 (screenshot).
- **Next:** his next round on this build (`## Next concrete step`) — the hand and the shelf
  first, then the difficulty: without the per-hit push the waves should be harder; measure his
  tower in `tools/tuning` before retuning. Then M2 closed → M3 (recipes, grill first).
- **Open live gates — one:** M2 with the difficulty, ADR 004/005 and D17–D24 (his round). When a
  playtest is due, start the game for him (CLAUDE.md).
- **Tests:** 122, green (`dotnet test tests/RettetDasEi.Sim.Tests`).
- **Git:** `main`, remote `origin` = github.com/SaftKinn/eigame (since 2026-10-24). The checkout's
  folder differs from session to session (2026-10-25 `Desktop\Projekte\EggGame`, 2026-10-26
  `Desktop\Claude\rettet-das-ei`): take the folder you are in, never the one written here.

## Current focus

**M2 — every food a job** (`roadmap.md`, ADR 003): built; its live gate is his round. The first
numbers are guesses — expect tuning (all in `units.json`).

## Last session

- **2026-10-26 (his third M2 round, in the checkout `Desktop\Claude\rettet-das-ei`)** — Thirteen
  of his words, each a round: built, built and tested where the core changed, booted headless,
  committed, pushed and the game restarted for him (his words in `## Playtest notes`). The
  glue: every face (`GlueBonds.Face` on `UnitBody.Outline`, edge to edge), the magnet
  (`Snap`, 12 px, all pairs in reach glow and stick), glue off per piece (G), only the hand's
  pieces stick by resting (`GlueWindow`). The Käsedreieck (`cheese_triangle`, `alternate`,
  `ContentDb.ShopUnits`, 6 new tests → 122). Turning 45° and back to 15°. Rebuilding: the glued
  group in the hand (`GroupOf`, `PlaceHeld`, `Lift`/`Land`), unglue on the right click, the
  shelf (`Main.OnShelf`). No push back per hit (D23). Two bugs he hit: every drop threw
  (`PlaceHeld` read the cleared `_held` — lesson `the-boot-never-lets-go-of-a-piece`); a lifted
  group flung apart on letting go — measured with a scratch probe of his tower from his video, a
  pepper at 5676 px/s → ≤ 465 px/s (the fall), still in 2.5 s. Build 0 warnings, 122 tests,
  headless boot clean after each round; everything play-unverified.

- **2026-10-25 (fresh checkout)** — The repo cloned into `Desktop\Projekte\EggGame` (on that
  machine). The game started for his M2 round; he asked which pests there are; no playtest words,
  no code changed.

## Next concrete step

**Sezgin's next round on this build** ("Spiel starten.bat") — the hand first (D17–D24, all
play-unverified), then the difficulty:
1. **Building:** a piece within 12 px of a cheese face glows "klebt hier" (every spot in reach);
   let go, it snaps flush and sticks — beside an upright cheese too. A held group (lifted with
   everything glued to it) turned and let go must fall calmly, not fly off (measured: it did).
   Right click while holding: the piece comes off its group. G: "Kleben aus" (not on a cheese).
   What falls while taking a tower apart stays loose.
2. **Käsedreieck:** right click on the cheese card; it glues on all three faces; Q/E 8× = on its
   next face. **Shelf:** park a group there, rebuild below, put it back; is it big enough?
3. **Difficulty (D23):** a hit no longer pushes a pest back — do pests now reach the tower; is
   wave 5 only just? Before D23 he won all five waves. If it is too hard or too easy, measure his
   tower in `tools/tuning` (rebuild it from his screenshot of 2026-10-26) before retuning.
4. **Toast:** stand one at the front of the tower — an attacker flies back, the toast springs,
   the tower wobbles less.
5. **Gurke:** a slice goes through a row of ants. **Senf:** a yellow puddle; ants in it crawl.
   **Pfeffer:** a pest trembles more and more, then "Hatschi!", a hop back and a pause.
6. **Tomate:** hold it — the red circle. Lay it in the pests' way: "Platsch!", pests hurt, your
   food shoved (a tomato beside the egg is a risk).
7. **Overall:** does each food feel different now? Is one useless or a must?
8. **Stability (ADR 005):** without cheese, does a high stack slide? Do several peppers glued
   high on a cheese tip a tower on a weak base (a toast) and not on a strong one?
9. **Pests (ADR 004):** do walkers pile up at the tower and attack together (a heap)? Do flies
   fly round the food to the egg — and bite their way in when it is built in?
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
| D16 | Stability | **Food but cheese and egg grips at 0.5; recoil pepper 200, pickle 150, mustard 120** | Height was free and the cheese pointless; his idea: recoil tips a stack on a weak base → ADR 005 |
| D10 | Tomate | **A bomb, no longer a shooter: bursts at any pest's touch; pushes own food, damages none** | Its rolling becomes the job; risk beside the egg → ADR 003 |
| D11 | Käse, Toast | **Cheese glue that a hard blow tears; toast a shield wall (throw-back, a share of the push)** | Clearly felt jobs for the two blocks → ADR 003 |
| D12 | Gurke, Senf, Pfeffer; teaching; order | **Pierce N; less splash + a puddle; an itch to N hits; the card's sentence; all six in one step** | His picks in the grill of 2026-10-24 → ADR 003 |
| D17 | Käse | **Every face sticks; the wax rind gone** | His "jede seite" (asked: all round); since ADR 005 the cheese holds a tower → ADR 003 addendum |
| D18 | Käse's magnet | **Let go within 12 px of a cheese face: snap flush, stick at once; every spot in reach glows** | "sonst fällt … runter", "präzise bauen"; a pull in the physics would tug towers apart → ADR 003 addendum |
| D19 | Käsedreieck | **The block's other shape, equilateral; one shop card, a right click switches** | His wish (three 120° angles cannot be — asked) → ADR 003 addendum |
| D20 | Turning | **15° steps kept** (45° tried, taken back the same hour) | 15° is the coarsest step with 45° and the triangle's 60°/120° |
| D21 | Glue off | **G while holding switches a piece's glue off/on; not a cheese's** | To build the egg in loose (first on the right click; moved by D22) |
| D22 | Rebuilding | **Lifting lifts the glued group; right click in the hand unglues; a shelf where pieces sleep** | "nervig … alles abreisen"; his picks → ADR 003 addendum (replaces point 6) |
| D23 | Hits | **No push back per hit (was 5 px); only sneeze and toast throw back** | Four peppers held any pest off the tower |
| D24 | Sticking by resting | **Only what the hand just let go (2 s) or a blow just tore off** | "was runterfällt nicht wieder gleich verkleben" → ADR 003 addendum |

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
| M2 — every food a job | **Built 2026-10-24**, live gate open | 111 tests green; build 0 warnings; smoke runs fired every job; a fixed tower wins all five waves. His rounds 2026-10-24 and 2026-10-26 (won all five; the hand reworked, D17–D24, 122 tests) — the closing round open |
| M3 — recipes | Later | — |
| M4 — look and feel | Later | — |

## Open questions

- **The look (M4):** keep the drawn stand-ins as the game's style, or GPT pictures? He likes the
  drawn figures (2026-10-24). Decide from candidates side by side → its own ADR.
- **The prototype's playtest questions** of 2026-10-24 that his verdict did not answer one by one:
  is stacking too slippery, sticky or jittery; does the tower topple too easily or never; is the
  egg on top too safe or too fragile; is "higher shoots further" a real temptation. Ask with M2's
  round.
- **M2: difficulty for a good builder.** His towers win all five waves (2026-10-24 twice, and
  2026-10-26 with the glue on every face). D23 (no push back per hit) should make it harder by
  itself — and D17/D18/D22 easier to build well. Measure after his next round, with his tower of
  2026-10-26 rebuilt in `tools/tuning` (its towers still date from 2026-10-24).
- **The shelf:** placed under the window (x 170–670, y 560) by guess — the right size and place?
  A piece left on it is never in danger: is parking a shooter there for a wave ever wanted?
- **A placed piece with its glue off** shows nothing once it lies; he will say if he needs a mark.

## Playtest notes

- **2026-10-26 — M2, third round** (in order; each a round, built and the game restarted):
  1. "es soll doch jede seite des käßes kleben entferne den roten rand 'schale'" — asked: "Ringsum,
     auch seitlich" → D17. (He had put side glue off twice on 2026-10-24; ADR 005 since.)
  2. "seitlich muss es magnetisch angezogen werden sonst fällt der käse einfach runter" → "Schnappen
     beim Loslassen" (D18).
  3. His screenshot, an upright cheese beside a flat one, not stuck: "nur übereinander schnappt er
     … die klebestelle taucht zu früh auf … man soll präzise bauen können" → faces judged by the
     centres had called it "on top", the reach a circle round the piece; now edge to edge, 12 px.
  4. "linksklick auf den käseblock … auf ein käsedreieck umschalten … 120 grad pro winkel" → asked:
     equilateral, a right click on the shop card (D19).
  5. "nur um 45 grad rotieren" (D20) — then 6. "die rotierung muss doch feiner … sonst das 3eck
     nicht mit jeder seite" → back to 15°.
  7. His screenshot, a cheese held across two: "wenn … 2mal verklebt … soll es auch angezeigt
     werden" → every spot in reach glows and sticks.
  8. "mit halten und rechtsklick kann man kleben deaktivieren … praktisch für das ei" (D21).
  9. **Won all five waves** (screenshot): on the board two toasts outside, two upright cheeses,
     the egg loose between them; a pickle on each toast; a cheese across the two as a roof with
     four peppers and two more sideways at its ends. Nothing said about it being too easy — ask.
     "alle bauteile außer den käse … bei dem ei stand Kleben au" → not a cheese; the label fixed.
  10. "nervig … alles abreisen … um seinen turm neu zu gestalten" → his picks: a shelf, lifting
      lifts the glued group, unglue while holding, shelf pieces sleep (D22).
  11. "das spiel hat sich grad aufgehängt" → every drop threw (a bug of round 10), fixed.
  12. "jeder schuss knocked gegner dezent zurück das muss raus … kann kein boss der welt an den
      turm … es darf nur mit niesen nockback kommen" (D23).
  13. His recording: "manchmal springt alles noch komisch und fliegt wild herum. Außerdem soll …
      das was runterfällt nicht wieder gleich verkleben" → a lifted group flung apart on letting
      go (measured 5676 px/s, fixed); D24.

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
- **2026-10-26** — His third M2 round: the building hand reworked (D17–D24), the Käsedreieck, the
  shelf; 122 tests.
