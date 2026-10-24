# ADR 005 — A tower needs a base: slippery food, real recoil

- **Status:** Accepted (built 2026-10-24, play-unverified)
- **Date:** 2026-10-24
- **Refs:** decision log D16 in `progress.md`; ADR 001 (the physics is the game), ADR 003 (the glue)

## Context

Sezgin won all five waves with a compact tower (his screenshot) and found the cheese superfluous:
"da man sich ja auch so in die Höhe bauen kann." Every food gripped at 0.9–1.0, and a shooter's
recoil was too small to feel (pepper 6 on a mass of 0.6) — so nothing toppled, height was free,
and gluing it added nothing. His idea: "dass alle Türme einen eigenen Rückstoß haben und somit
umfliegen können … 4 Salzstreuer auf einem Käseblock geklebt, aber etwas hochgebaut, lässt den
Turm umfliegen, wenn die Base nicht genug Stabilität bietet."

## Decision

Both, in one round (his pick):
1. **Slippery without cheese:** every food but the cheese (1.0) and the egg (0.9) grips at 0.5 —
   a high stack slides under blows unless it is glued.
2. **Recoil you feel:** pepper 6 → 200, pickle 25 → 150, mustard 15 → 120 (an impulse on the
   shooter, against the shot, every shot). A glued shooter passes it into the whole stack through
   the pins; the higher it sits, the stronger the lever. From one side the shots add up; from both
   sides they partly cancel.

Sized by a sum, then measured: four peppers fire about 12 shots a second; sliding a ~8-mass stack
at grip 0.5 takes ~3900 impulse a second, tipping it over a toast's half-width ~2400 — so ~200 a
pepper shot tips a weak base and not a strong one.

## Alternatives

- **A bonus on the cheese** (a shooter on its cut face shoots further or harder) — the parked
  "touch bonus"; not now.
- Only one of the two; or a bigger recoil (an unglued pepper is already shoved ~330 px/s a shot).

## Consequences

- Measured in the harness (four peppers high on a cheese, the egg aside): on a toast the cheese
  tilts 49–58° and slides 115 px in wave 1; on a second cheese 2° and 3–8 px. His own tower (a
  cheese base, peppers on both sides) stays put (1°, 2–6 px) and still wins.
- Unglued shooters on the board now jump back with every shot — part of the look (ADR 001), and
  a reason to glue them.
- Not touched: how hard the waves are for a good builder — his tower still wins without buying
  for wave 5; that is the next tuning question.
