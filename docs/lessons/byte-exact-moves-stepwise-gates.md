# Refactors: byte-exact moves, stepwise gates

**Move code mechanically (never retype), cut the seam so behaviour cannot change, and migrate one
seam at a time with every gate green after each step. Review effort belongs on new glue — the
moves are the safe part.**

## Where it comes from

Carried over from Chief Wants His Cut (and Hollowdeep and dungeonmaster): across many extraction
rounds, every bug a review confirmed was in new glue or new test code, none in the mechanical
moves. Hand-retyping is where drift enters.

## How it applies here

- **M1 moves the existing rules into the core** (content, gold, waves, range — ADR 002). Move
  whole methods and keep the order of what happens in a step identical. One seam at a time:
  content first (its tests prove the move), then gold, then the waves.
- There are no tests of the old Godot code, so the gate after each step is `dotnet build`, the
  new core tests, and a headless boot without errors; the last gate is Sezgin playing a round and
  finding nothing changed.
- A test that has to be edited during a pure move is a design smell of the cut, not a chore.

## The correction

Slice, don't retype. Migrate incrementally, all gates green after each step, no big-bang diffs.
