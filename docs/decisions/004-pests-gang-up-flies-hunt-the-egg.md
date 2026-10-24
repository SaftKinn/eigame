# ADR 004 — Pests gang up; flies hunt the egg

- **Status:** Accepted (built 2026-10-24, play-unverified)
- **Date:** 2026-10-24
- **Refs:** decision log D14–D15 in `progress.md`, `architecture.md` §5.3; ADR 002 (boundary), ADR 003

## Context

After the tuning round of M2 Sezgin asked: "Gegner sollen sich stacken und angreifen, nicht immer
nur einer. Außerdem sollen die Fliegen um Bauteile rumfliegen und direkt das Ei angreifen, wenn es
möglich ist." In the game walkers queued behind one another (`Main.IsBlockedByEnemy`), so only the
front one ever attacked; flyers flew at the highest piece and bit the first thing they touched.

## Decision

1. **Pulk (his pick):** a walker still queues behind a walker that walks, but never behind one
   that attacks — it pushes in and attacks too. Their blows add up. Walkers attacking at one spot
   are drawn on top of each other (`Main.CrowdIndex`: 0.7 of a radius higher per pest that began
   earlier), so the crowd reads as a heap.
2. **Flies hunt the egg:** a flyer heads for the egg and steers round any other food in its way —
   it tries turns of 30–150° from the straight way, upward first, keeping the side it chose —
   and bites the egg as soon as it touches it, even among other pieces.
3. **Patience (his pick):** while the straight way is blocked it counts; after `flyPatience`
   seconds (`game.json`, 2.0) it bites the piece in its way (`Flight.GivesUp`, the core's). A
   fully built-in egg is therefore worth building — the flies have to eat their way in.
4. Wave 3 softened (6 flies instead of 10, 14 ants instead of 16): with flies hunting the egg it
   was out of reach of what the gold allows by then.

## Alternatives

- **Climbing** — walkers climb onto the one in front and form a heap that reaches higher pieces.
  More spectacular, much more work and harder to balance. Not now.
- **Both** (crowd, then climb). Not now.
- **Flies:** search forever (a built-in egg would be safe from them); fall back to the highest
  piece as before.

## Consequences

- Much harder: crowds of up to six ants were measured at one piece; the egg on top of a tower is
  now the flies' easiest target — building over or round it matters.
- Measured with fixed towers (`docs/conventions.md`): 115 gold loses in wave 3, 250 in wave 4,
  345 in wave 5, 390 (all the gold a game gives by then) wins. A 180-gold tower — the most one
  can have before wave 3 — lost wave 3 every time in the harness; his taller towers may do better.
  Wave 3 is the first thing to watch.
- Steering is Godot's (where the way is free); when to give up is the core's.
