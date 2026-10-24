---
name: grill-me
description: Interview Sezgin relentlessly about a plan or design decision until reaching shared understanding, resolving each branch of the decision tree one fork at a time. Use before a milestone's mechanics are built (proposal A — every food a job; proposal B — recipes), when he wants to stress-test a plan, or says "grill me". For design forks — not for mechanical edits.
---

# Grill the plan before any code

Resolve the decision tree *with Sezgin* first, then build. Deciding together beats handing over a
finished solution — this is the tool for that.

## How to grill

1. **Answer it yourself first.** Check whether the docs or code already decide it:
   `architecture.md` (design wins), the ADRs (the milestone → ADR map in `progress.md` points to
   the governing one), `roadmap.md` (the milestone's goal and gate), the "What this project is"
   section of `CLAUDE.md`, then the code and `game/content/`. Only ask what genuinely needs his call.
2. **One fork at a time.** Walk the tree branch by branch, dependencies first — never dump a list.
   Each answer narrows the next question.
3. **Always recommend.** For every question give your recommended answer with a one-line why,
   and where it helps, how comparable games solve it (physics stackers and tower defenses: e.g.
   Tricky Towers, Kingdom Rush, Plants vs. Zombies — name the mechanic, not a copy). Use
   `AskUserQuestion` (recommended option first, labelled "(Empfohlen)") for clean 2–4-way forks;
   ask in chat for open-ended points. Questions in **German**, simple words — he is a beginner;
   this file stays English.
4. **Be relentless, but converge.** The rule, its numbers (which go into `game/content/`), edge
   cases (what if two effects meet, what if the piece falls off the tower, what about the egg),
   how it feels in play, how it touches the loop (build phase ↔ wave, gold, height) — until the
   design is unambiguous. Stop the moment nothing left would change what gets built.
5. **Ask how it is seen.** Without frames (ADR 001) every effect needs an answer to "what does the
   player see?" — a tween, a puddle, crumbs, a number, a sound. One fork for that per feature,
   before the first view (lesson `ask-what-the-screen-is-before-building-it`).

## Respect the golden rules while grilling

Surface tensions with the golden rules (`CLAUDE.md`) as forks:
- **The boundary of ADR 002:** for each new rule, say which part is the physics' (contacts, a real
  impulse, friction, a joint) and which is the core's (how much, how long, whether, a count). A
  rule that would make a Godot script decide "how much" is a fork, not a detail.
- **No frames** (ADR 001): a mechanic that seems to need an animation needs a tween instead.
- **Balancing is data:** every number named in the grill becomes a content field, validated.
- **One function per rule:** a value shown and applied in two places is one function.

## Close-out

Summarise the agreed design in a few lines (German) and offer to scaffold the next ADR
(`session-ritual`) and/or write a PRD (`to-prd`). Then, and only then, build — one testable step
at a time, test-first with `tdd` for the core rules worth it.
