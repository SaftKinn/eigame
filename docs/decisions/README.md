# Architecture Decision Records (ADRs)

This is where decisions live that weighed a real trade-off — not every little thing.
Trivia stays in the decision log in `progress.md`.

## When an ADR?

When a decision had alternatives that were seriously on the table, and the choice binds
later work. Rule of thumb: if a new model would ask in three months "why this way and not
another?", it belongs here.

## Convention

- One file per decision, numbered sequentially: `NNN-short-title.md`.
- Numbers are never reused. If an ADR is superseded, the old one gets the status
  `Superseded by ADR NNN` and stays (history rather than deletion). A dated addendum is fine
  when the built thing has moved on.
- The highest-numbered file is mandatory reading at session start (session ritual in
  `CLAUDE.md`).
- Template: the `session-ritual` skill (`new-adr`).

## Index

| ADR | Title | Status |
|---|---|---|
| [001](001-physics-tower-no-frames.md) | A physics tower defense without frame animations | Accepted (prototype) |
| [002](002-rules-core-physics-in-godot.md) | A rules core with tests; the physics stays in Godot | Accepted |
| [003](003-every-food-a-job.md) | Every food a job (proposal A) | Accepted, not built (M2) |
