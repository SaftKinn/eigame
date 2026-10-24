# ADR 002 — A rules core with tests; the physics stays in Godot

- **Status:** Accepted
- **Date:** 2026-10-24
- **Refs:** decision log D6 in `progress.md`, `architecture.md` §6; refines ADR 001 point 4

## Context

ADR 001 put everything into the Godot project for the prototype, because the core of this game
is Godot's physics, and said a later ADR would decide what moves into a pure C# sim once rules
grow. Sezgin played the prototype on 2026-10-24: stacking is fun, but every food feels the
same. The next feature (proposal A: cheese sticks, toast bounces pests back, pickle slices
pierce, mustard puddles slow, pepper makes pests sneeze, a tomato bursts like a bomb) and the
one after it (proposal B: recipes such as toast–cheese–toast = sandwich) add many rules with
branches — stacking of effects, durations, counts, radii, which piece lies on which. In *Chief
Wants His Cut* such rules live in a pure sim with tests and that has paid off; there, though,
the sim is the whole game. Here it cannot be: what touches what is the physics.

## Decision

A **rules core**: `sim/RettetDasEi.Sim`, a plain C# library with **no Godot reference**, and
`tests/RettetDasEi.Sim.Tests` (xUnit), as in Chief Wants His Cut. **Godot reports facts, the
core answers what follows.**

The boundary in one sentence: *if the answer needs to know where something is or what touches
what, Godot asks the physics; if the answer is what follows from it — how much, how long,
whether, in which order — the core decides.*

**Into the core:**
1. **Content:** the defs and `ContentDb` (load and validate). The loader takes a function that
   reads a file, so tests load the shipped JSON without Godot.
2. **Gold and the shop:** start gold, buying, the sell refund, rewards for a kill and a wave.
3. **Waves:** the spawn schedule from `waves.json` (who comes when, from which side) and the
   phases — build, wave, cleared, won, lost — with the rules for moving between them (the egg
   on the counter loses a wave and keeps one from starting).
4. **Numbers of an attack:** range by height (one function, read by the shooter and by the
   range overlay), damage, and from proposal A on every effect's rule — a slow's strength,
   length and stacking, a sneeze's pause, how many pests a slice pierces, a bomb's damage over
   its radius.
5. **Recipes** (proposal B): Godot hands over which piece rests on which; the core finds the
   recipes in that and says what they give.
6. Later: runs, unlocks and saves — everything the CLAUDE.md "prototype first" rule still holds
   back.

**Stays in Godot:**
- The bodies, stacking, toppling, contacts; cheese's stickiness (a physics trick); a bounce
  or a push as a real impulse — its *strength* comes from content through the core.
- Pests moving along the counter and flying; projectiles flying and hitting; who is nearest.
- The hand: drag and drop, rotate, the red ghost; and all juice — tweens, flashes, crumbs,
  numbers, shake (ADR 001 point 1).

**How the two talk** (Chief's pattern, cut to this game): the core holds a small state (gold,
the wave, the effects on each pest) and gets commands and facts (`Buy`, `Sell`, `StartWave`,
"pest 7 was hit by a pickle slice", "the egg touches the counter", a fixed time step); it
answers with values and appends **events** the view drains once per frame. No callbacks from
the core into Godot. Time in the core is the physics step's (120 Hz, ADR 001). A random
decision that is a rule goes through a seeded `Rng`; cosmetic randomness (crumbs, a pest's idle
phase) may stay `GD.Rand*`.

**Order:** first the rules that exist move into the core *without a change in behaviour*
(content, gold, waves, range) — a milestone of its own (M1 in `roadmap.md`), closed when
Sezgin plays a round and finds nothing changed. Only then do the new rules of proposal A get
built, test-first where they branch.

## Alternatives

- **B — everything stays in Godot, no tests.** Fastest for the next week. But proposal A alone
  brings a dozen branching rules, and B's recipes are pattern matching — exactly what breaks
  silently and is found only by playing. Rejected.
- **C — a full sim with its own physics** (Chief's shape: the sim steps the world, Godot only
  draws). Would give replay tests of whole waves, but means writing a 2D physics engine and
  throws away what the prototype proved fun. Contradicts ADR 001 point 3. Rejected.

## Consequences

- Rules get fast tests (`dotnet test`, no editor); the content files are validated by a test,
  so a broken JSON fails before the game starts.
- **No replay of a whole wave:** Godot's physics is not deterministic across runs, so the
  invariant tests are about rules (a schedule, a refund, a slow that does not stack past its
  cap), not about a wave's outcome. A wave's feel is still judged by Sezgin playing.
- Two `Vector2` types: the core uses `System.Numerics.Vector2`; conversion lives in one file on
  the Godot side.
- Golden rule "the view never decides an outcome" holds in this game's narrower form: a Godot
  script may decide *where and what touches*, never *how much, how long or whether*.
- The Stop hook and the pre-commit hook run the tests once the test project exists.
