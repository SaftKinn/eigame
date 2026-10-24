---
name: add-content
description: Use when adding game content or a new kind of mechanic — a new food (unit), pest (enemy) or wave in game/content/*.json, or a new field/kind that content needs (an effect, a projectile kind, a movement). Walks data → validation → rule in the core → test → what Godot does with it → visual key, so the entry is wired end to end. The main "grow the game" lever.
---

# Add content (and the mechanics it needs)

Golden rule: balancing is data. Most additions are a JSON entry; some need a new field or kind.
Either way it must be wired end to end — a field nobody reads is the classic silent bug (lesson
`unwired-knobs-and-silent-fallbacks`).

## A. Pure content (existing fields only)

1. Add the entry to the right file: food → `game/content/units.json`, pest → `enemies.json`,
   wave → `waves.json` (in order). Id: English `snake_case`, stable. `name`/`description`: German.
2. Every field is required — copy a neighbour and change it. References must exist (a wave's
   `enemy` ids).
3. **The look:** add a stand-in drawing for the new `visual` key to `game/scripts/View/DrawKit.cs`
   (a shape and the `fill`/`accent` colours that read well against the kitchen and differ from
   neighbours) — else it draws magenta. Add its row to `docs/art-prompts.md` (file, facing,
   aspect) so a picture can follow.
4. **Check:** the Stop hook builds and boots headless; by hand: `dotnet build
   game/RettetDasEi.csproj`, then a headless boot (`docs/conventions.md`, Testing) — no
   `Content is broken`. From M1: `dotnet test tests/RettetDasEi.Sim.Tests` validates the real files.
5. Commit: `content: add <thing>`. Say in the round that it's **play-unverified**, and what
   Sezgin should look at (does it topple, is it worth its price).

## B. A new field or kind (needs code)

1. **Design check:** does it fit the loop (`architecture.md` §2) and the ADRs of the area
   (milestone → ADR map in `progress.md`)? If it's a real fork, `/grill-me` first.
2. **Split it along ADR 002:** what is the physics' (a contact, an impulse, friction, a joint) and
   what is the core's (how much, how long, whether, a count). Write that down in one line before
   coding.
3. **Def:** add the property to its def (`ContentDefs.cs` now, the core's defs from M1) —
   `[JsonRequired]`, or nullable when it is genuinely optional (like `attack`); never a default
   "to be safe". Add it to every entry of the file.
4. **Validation:** add the rule to `ContentDb.Validate` (ranges, references, a kind from a fixed
   list) — from M1 with a test that bad content is rejected with a clear message.
5. **Rule (core, from M1; test-first where `tdd` says it pays):** the scenario in the tests' own
   content and the core code.
6. **Godot side:** the physics part (an impulse, a query), and how it is **seen without frames**
   (ADR 001): a tween on the visual, a flash, crumbs, a number, a shake. If the view must react to
   the core, read its event.
7. **Content:** at least one real entry using the new field.
8. **Docs:** `architecture.md` §5–§7 if the mechanic is new; `docs/conventions.md`'s tuning list;
   ADR if a trade-off was decided.
9. Build, boot, tests green → commit code + tests + content together.

## Checklist before saying done
- [ ] Every new field is read (grep for it)
- [ ] Invalid values are rejected at load
- [ ] From M1: a test covers the rule
- [ ] Visual key drawn (no magenta), its row in `docs/art-prompts.md`
- [ ] No frame animation added (ADR 001)
- [ ] Marked play-unverified in the wrap-up
