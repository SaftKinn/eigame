---
name: improve-architecture
description: Find deepening opportunities across the codebase — refactors that turn shallow pass-through modules into deep ones, for testability and AI-navigability. Informed by architecture.md and the ADRs; never re-litigates a decided ADR. Use when Sezgin wants to improve architecture, find refactoring opportunities (e.g. Main.cs growing too large, rules leaking into Godot scripts), or make a part more testable. Whole-codebase altitude — not a diff review.
---

# Improve codebase architecture

Surface friction and propose **deepening opportunities**: refactors that turn shallow modules into
deep ones. The aim is testability and AI-navigability — this project's survival strategy across
many sessions and models.

## Vocabulary — use these exact terms

- **Module** — anything with an interface and an implementation (function, class, system).
- **Interface** — everything a caller must know: types, invariants, ordering, error modes, config.
- **Depth** — much behaviour behind a small interface. **Shallow** = interface nearly as complex as
  the implementation.
- **Seam** — where behaviour can change without editing in place.
- **Locality** — change, bugs and knowledge concentrated in one place.

Principles: the **deletion test** (delete it — does complexity vanish or reappear across callers?);
**the interface is the test surface**; **one adapter = hypothetical seam, two = real seam**.

## Process

### 1. Explore
Read `architecture.md` and the governing ADRs first (001 physics and no frames, 002 the rules core,
and whatever the milestone → ADR map names). Then walk the code (Explore subagent for breadth) and
note friction:
- `Main.cs` accreting every system (phases, waves, gold, the hand, queries): would parts behind the
  same order of a physics step be deeper?
- Godot scripts that started deciding rules — how much, how long, whether (an ADR 002 leak, not
  just friction).
- Content validation scattered vs. owned per def.
- The same value computed in two places (lesson `parity-by-construction`).
- Pests and units reaching into `Main.I` for everything: is a query interface hiding there?

### 2. Present candidates (Markdown, in chat, German)
Per candidate: **Files**, **Problem**, **Solution** (plain words), **Benefits** (locality, leverage,
tests), **Before/After** sketch, **Strength** (`Strong` · `Worth exploring` · `Speculative`). End
with a **Top recommendation**. Don't propose interfaces yet. Ask: "Welche davon willst du dir
genauer ansehen?"

ADR conflicts: only surface a candidate that contradicts an ADR when the friction is real enough to
reopen it — flag it clearly.

### 3. Nail down the chosen candidate
Scale the ceremony: small/obvious → state the target shape and build it (byte-exact moves, lesson
`byte-exact-moves-stepwise-gates`). Real forks → `/grill-me`. Mandatory side effects: update
`architecture.md` for any new module concept; offer an ADR when Sezgin rejects a candidate for a
load-bearing reason.

## Respect the invariants
Never cross a golden rule: no Godot in the core, no rules in the view, no second code path for one
rule, no frame animations. A refactor of Godot-side code has no test gate — its gate is the build,
a headless boot and Sezgin finding that the game plays as before; say that it is play-unverified.
