# Project skills

Reusable, invocable procedures for this repo (`.claude/skills/<name>/SKILL.md`). Each one packages a
recurring multi-step workflow so it survives context clears and model switches.

**How a skill runs** (it is *not* an event trigger like the Stop hook):
- You type the slash command (e.g. `/playtest-triage`), **or**
- Claude invokes it on its own when your request clearly matches the skill's `description`. That's a
  judgment call — type the slash command if you want to be sure.

Only name + description sit in context at rest; the body loads when used.

## The skills

| Skill | Command | What it does |
|---|---|---|
| **session-ritual** | `/session-ritual` | Start handshake (CLAUDE.md → progress State header → latest ADR → lessons) + end-of-session wrap-up within caps + scaffold the next ADR + commit. |
| **grill-me** | `/grill-me` | Stress-test a plan one fork at a time, with a recommendation per question (in German) → offer an ADR. Use before each milestone's mechanics (M2: proposal A). |
| **to-prd** | `/to-prd` | Write a grilled-out plan as a PRD to `docs/plans/<slug>.md` (no interview). |
| **tdd** | `/tdd` | Red-green-refactor for rules-core changes that are worth it (real logic, invariants, validation); says when a test alongside is enough and when none is needed. Guards against test-bending. From M1. |
| **add-content** | `/add-content` | Add food, a pest or a wave — or a new field/kind the content needs: JSON → validation → rule in the core → test → what Godot does with it → visual key. The main "grow the game" lever. |
| **playtest-triage** | `/playtest-triage` | Turn "how it felt" into a fix round: classify (tuning / physics feel / look / core bug / view bug / performance / design question), smallest correct fix, check, commit, progress update. |
| **improve-architecture** | `/improve-architecture` | Whole-codebase deepening review (deletion test, seams, locality) — where Godot scripts have started deciding rules. |

All seven are taken over from *Chief Wants His Cut* (2026-10-24) and rewritten for this game: a
physics tower defense whose rules core (ADR 002) is small and whose physics stays in Godot.
`grill-me` goes back to Matt Pocock's skill of that name. Where a skill names a file or class that
M1 has yet to create (the core, the test project, `TestContent`), it says so — fix the name in
the skill when the code differs.

## Related: the hooks (not skills)

`tools/hooks/check-on-change.sh` is wired as a `Stop` hook in `.claude/settings.json`. It builds
**only when `game/scripts/` or `game/content/` changed** (from M1 also `sim/`, `tests/`), boots the
game headless for a moment when content changed, runs the tests once they exist, and stays
**silent on green**. `tools/hooks/pre-commit` does the same for what is staged, as a blocking git
hook (`git config core.hooksPath tools/hooks`, once per clone).

## Maintenance

When you add a skill, add a folder `<name>/SKILL.md` **and a row to the table above**.
`settings.local.json` stays local; the skills and `settings.json` are committed (`.gitignore` allowlist).
