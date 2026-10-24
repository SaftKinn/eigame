---
name: tdd
description: Use when a change to the rules core (sim/RettetDasEi.Sim — gold, waves, effects, recipes, content validation; ADR 002, from M1) has real logic or an invariant worth pinning with a failing test first. Red-green-refactor against the xUnit project, tests in blocks allowed. Not for trivial additions, Godot scripts, physics feel, visuals, art mapping, or content tuning.
---

# Test-driven a change to the rules core

ADR 002 makes the rules core the TDD target: plain functions, no engine, exact outcomes. Where it
pays, the test leads. The core and its test project exist since M1 (2026-10-24); `TestContent`
is the tests' own content.

## When it pays — and when it doesn't

**Test first:**
- A rule with branches or edge cases — two slows on one pest, a sneeze during an attack, a slice
  that pierces its last pest, a bomb at the edge of its radius, a recipe with a piece missing.
- An invariant: a refund never above the price, an effect that ends exactly when its time is up,
  "one big step equals many small ones" for a timer, a wave schedule that is sorted.
- Content validation — cheap, and it guards against silent bad data.
- A reported rules bug: reproduce it as a failing test before fixing.

**A test alongside or after is enough:**
- A trivial addition — a clamp, a new field read in one place, a formula with no branch. Don't
  write empty stubs just to watch it fail first.

**No unit test at all:**
- `game/scripts/` (bodies, pests' movement, projectiles, the hand, HUD), the physics' feel,
  `DrawKit` and art mapping, tuning numbers in `game/content/`. Those are verified by the build,
  a headless boot, and Sezgin playing. Godot's physics is not deterministic across runs (ADR 002)
  — never write a test that asserts how a wave ends.

## The loop — for the cases above that are test-first

1. **RED — write the failing test(s)** in `tests/RettetDasEi.Sim.Tests/<Area>Tests.cs` (look
   whether the file exists first — lesson `look-whether-the-file-is-there-before-writing-it`). A
   block of tests for one feature is fine; one behaviour per test. Build the scenario explicitly:
   the tests' own small content (a `TestContent` helper, made in M1 — check its name), the state
   the scenario is about, a fixed seed where a roll is involved. Describe the behaviour, not the
   implementation.
2. **Confirm red:** `dotnet test tests/RettetDasEi.Sim.Tests --filter "FullyQualifiedName~<Test>"`.
   It must fail **for the right reason** (an assertion, not a compile error). A test that was never
   red proves nothing.
3. **GREEN — minimal implementation** in the core. Tunables come from content, randomness from the
   passed-in `Rng`.
4. **Confirm green:** the single test, then the whole suite (`dotnet test tests/RettetDasEi.Sim.Tests`).
   Read the count of tests, not only "Bestanden".
5. **REFACTOR** only once green. Tests stay untouched.
6. Repeat for the next behaviour. Then wire it into Godot and build — the wiring is play-unverified.

## Guardrails

- **Never edit a test to make it pass.** If a test is wrong, say so and fix it as a separate,
  explained step.
- **No green before red** — for the test-first cases.
- **Don't test against `game/content/`** for logic — balancing would break it. One test loads the
  shipped files to validate them; that is all they are for.
- **Exact outcomes.** Assert exact values or tight ranges with a stated reason.
- **Invariants first.** If the new system has an invariant, that test comes before the feature tests.

## Close-out

- Full suite green, the count read.
- Commit test + implementation together: `sim(<area>): <behaviour>`.
- A real design trade-off → ADR via `session-ritual`; plain red-green of an agreed behaviour → no ADR.
