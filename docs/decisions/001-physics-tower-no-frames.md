# ADR 001 — A physics tower defense without frame animations

**Date:** 2026-10-24 · **Status:** accepted (prototype)

## Context

Sezgin's two unfinished games (`cartoon_battler`, `Runaway`) stalled on the same thing: walk and
attack animations he could not make look good. GPT makes good single pictures, but no clean
animation frames. He wants a game that does not need them, with units and assets made by GPT.

From the brainstorm on 2026-10-24 he chose a **physics stacking defense** in a **kitchen**: food
stacked into a tower protects an egg from kitchen pests ("Rettet das Ei").

## Decision

1. **No frame animations.** Every unit and pest is one still picture. All motion is code:
   - shooting = wind-up (squash, lean back) → release (snap forward) → elastic settle, plus a real
     recoil impulse on the body;
   - walking = waddle (tilt) / roll (spin) / hop (squat and leap) / fly (hover, wings squashed);
   - attacking = lean back → lunge → recoil, and at the lunge a physics impulse on the food — the
     tower's wobble is the animation;
   - hits = white flash, crumbs, numbers, screen shake; death = flung away spinning and faded.
2. **Godot 4.6.2 .NET + C#**, set up like *Chief Wants His Cut* (the project that works on his
   machine).
3. **Physics is Godot's built-in 2D physics** at 120 ticks a second, 24 solver iterations. Food is
   `RigidBody2D`; pests are not bodies but query the space and push with impulses, so they cannot
   jam or be launched by the stack.
4. **No sim/Godot split for the prototype.** The core of this game is the physics, which lives in
   Godot. If the prototype is fun and rules grow (economy, runs, unlocks), those move into a pure
   C# sim with tests, like Chief Wants His Cut (a later ADR).
5. **Balancing is data** (`game/content/*.json`), **art behind visual keys** with stand-in drawings,
   as in Chief Wants His Cut.

## Consequences

- A picture can be swapped in by dropping a PNG; nothing waits for art.
- The feel rests on tuning (`mass`, `friction`, `push`, damping) — expect playtest rounds on numbers.
- Godot 2D physics can jitter in tall stacks; if it does, first raise ticks/iterations and damping,
  then consider a different 2D physics backend.

## Addendum (2026-10-24)

Played the same day: stacking is fun, every food feels the same. Point 4 is now decided by
**ADR 002** — the rules move into a core with tests (M1), the physics stays in Godot.
