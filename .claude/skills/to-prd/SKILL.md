---
name: to-prd
description: Turn the current conversation context — ideally after a /grill-me session — into a PRD written to docs/plans/. Do NOT interview; synthesize what's already been decided. Use when Sezgin wants a grilled-out plan written as a spec, or says "to-prd" / "schreib den Plan".
---

# Synthesize the grilled context into a PRD

The companion to `/grill-me`: grill-me builds the shared understanding, this writes it up.
**Do NOT interview** — if the context is thin (no grilling happened), say so and suggest `/grill-me`.

## Process

1. **Explore.** Read the current code in the area, `architecture.md`, the governing ADRs and the
   milestone in `roadmap.md`. Use the game's vocabulary (Ei, Brett, Theke, Welle, Laden, Gold,
   Reichweite, the foods' and pests' German names); don't re-decide what ADRs settled.
2. **Sketch the test seams.** The natural seam is the rules core's public surface (ADR 002): its
   commands, its step, its pure functions, the loader — with explicit set-up. Name which
   invariants get a test first, and what is verified only by playing (the physics' feel, how a
   topple looks). Check with Sezgin that the seams match his expectation.
3. **Write the PRD** to `docs/plans/<feature-slug>.md` (create the folder the first time) with the
   template below, then point `progress.md`'s `## Next concrete step` at it. Slice it: each slice
   ends in something that runs and that he can try.

<prd-template>
## Problem Statement   — from the player's perspective
## Solution            — from the player's perspective
## User Stories        — a numbered list: "As a player, I want …, so that …"; cover every part of the loop the feature touches (build phase, wave, shop, the egg)
## Implementation Decisions — what is the core's and what is Godot's (ADR 002), data shapes, content fields, events the view needs, how it is seen without frames.
                              NO file paths or code (they rot). Exception: a small record/enum that pins a decision.
## Content             — the new JSON fields and a few example entries (German names)
## Slices              — in order, each one testable on its own
## Testing Decisions   — invariants, scenarios, what is verified only by playing
## Out of Scope
## Further Notes
</prd-template>

## Hand-offs
- Build the core rules worth it test-first with `/tdd`; physics, view and tuning are verified by
  the build and by Sezgin playing.
- A real trade-off uncovered while writing → ADR via `/session-ritual` (the PRD is the plan, the
  ADR records the decision).
