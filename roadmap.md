# roadmap.md — Rettet das Ei

Goal of this roadmap: **a tower defense in which every piece of food is bought for what it
does** — then the look, then a run worth playing again.

Every milestone has a **verification gate**: only when the proof is recorded in `progress.md`
does the next one start. Automated proof (`dotnet build`, from M1 `dotnet test`) and play proof
(Sezgin at the mouse) are listed separately — the second is a **live gate** and counts against
the WIP limit (max 3 open, `CLAUDE.md`).

---

## Claude effort (for Claude Code)

- Default **high**.
- **xhigh** for new rules with tricky interactions (effects that stack, recipes), physics
  behaviour that only shows in play, and anything that will touch a save format.
- **medium** for content entries, art mapping, doc sweeps.

---

## Part 1 — Every food a job

### M0 — The prototype
**Goal:** answer one question — is stacking food and watching it wobble and topple under attack
fun, without any frame animation?
- One kitchen, six kinds of food, four pests, five waves, shop, drag and drop with live physics,
  rotate, sell, range by height, game over and win; stand-in drawings; ADR 001.
- **Built 2026-10-24.**
- **Verification:** `dotnet build` green; "Spiel starten.bat" starts the game.
- **Verification (live):** Sezgin plays a few rounds and says whether stacking is fun.
  **Met 2026-10-24:** "Das Spiel ist cool." — with the finding that leads to M2: every food feels
  the same, only the shapes differ.

### M1 — The rules core
**Goal:** the rules that exist live in a tested core; the game plays exactly as before (ADR 002).
- `sim/RettetDasEi.Sim` (no Godot) and `tests/RettetDasEi.Sim.Tests` (xUnit), both in the
  solution; the game's project references the core.
- Moved, without a change in behaviour: the content defs and `ContentDb` (load with a file
  reader, validate); gold, buying and the sell refund; the wave schedule and the phases (build,
  wave, cleared, won, lost); range by height as one function.
- Godot calls the core and reads its events; `Main.cs` keeps the physics, the hand and the view.
- The Stop hook and the pre-commit hook run the tests.
- **Built 2026-10-24** (commits `df9ac84`, `c4c6024`): `GameState`, `WaveSchedule`, `Shop`, `Reach`,
  `ContentDb` in the core; 76 tests; a headless smoke run of wave 1 in a scratch copy.
  *The tests' gate is met; the live gate is open.*
- **Verification (tests):** the shipped content loads; each kind of bad entry is rejected with a
  message naming file and id; the refund rule; the schedule (times, sides — "both" alternates,
  the first from the right — sorted); every phase change (a wave cannot start with the egg on the
  counter, cleared pays the reward, the last wave wins, the egg on the counter loses).
- **Verification (live):** Sezgin plays a round and finds nothing changed.

### M2 — Every food a job (proposal A)
**Goal:** each piece of food is bought for what it does, not for its shape.
- The proposal (from the other chat): Käse is sticky (what lies on it does not slide); Toast
  bounces attackers back; Gurke's slices pierce several pests; Senf leaves a puddle that slows;
  Pfeffer makes pests sneeze (a push back, a short pause in attacking); Tomate bursts like a bomb
  when it rolls onto a pest.
- **Before building:** `/grill-me` proposal A — each job's rule, what is physics and what is the
  core's (ADR 002), how strong, how it is seen without frames → ADR 003. *(Done 2026-10-24: ADR 003
  — the tomato a bomb that no longer shoots, cheese glue that tears, toast a shield wall; all six
  built in one step, his pick.)*
- **Verification (tests):** each rule of the core with its edge cases — how effects stack, end,
  and meet each other.
- **Verification (live):** Sezgin can say what each food is for; buying is a choice between jobs;
  no food is useless or a must.

### M3 — Recipes (proposal B)
**Goal:** how pieces are stacked on one another gives something more — toast–cheese–toast is a
sandwich, a tomato on cheese a pizza.
- **Before building:** `/grill-me` — what a recipe needs (touching, order, rotation), what it
  gives, how the player learns them, how it is shown → ADR.
- **Verification (tests):** recipes found from "which piece rests on which" — found, not found,
  two at once, broken apart.
- **Verification (live):** a recipe found feels like a discovery and changes how he builds.

### M4 — Look and feel
**Goal:** it looks and sounds like a finished game.
- **Before building:** decide the look — the drawn stand-ins as the game's style, or GPT pictures
  (`docs/art-prompts.md`) — from candidates side by side → ADR.
- Art for everything drawn; sound; a main menu.
- **Verification (live):** Sezgin shows it to someone who has not seen it.

---

## Part 2 — Big rocks (after Part 1, not before)

From `progress.md`'s parked ideas; none of them is decided.
- **A run:** about 20 waves with a boss every fifth; unlocks between runs; saves.
- **Kitchen hazards:** wind gusts, a ladle that sweeps the counter, a cat's paw from above,
  something that undermines the board.
- **Touch bonuses:** a shooter standing on cheese deals more (M2 or M3 may take this in).
- **Steam release:** export preset (with `content/*.json` in its non-resource filter), store page.
