# progress-archive.md — Rettet das Ei

Older `## Last session` entries and resolved open questions, rotated out of `progress.md` (the
`session-ritual` skill). History only — never needed for normal work.

## Last session (Verlauf)

- **2026-10-24 (M1)** — The rules core built, one seam at a time, a commit each: `df9ac84`
  (content: `ContentDb` loads through a file reader — `GameContent` in the game —, `HexColor` for
  Godot's colour check, 45 tests) and `c4c6024` (`GameState` with gold, shop, waves, phases and
  events; `WaveSchedule`, `Shop.Refund`, `Reach.Of`; `Main` reports and shows events; 76 tests).
  Checked before the move: Godot's `Mathf.RoundToInt` rounds half to even, `Color.HtmlIsValid`'s
  rule (both against GodotSharp 4.6.2 in a scratch program). A test compares every shipped wave
  with the prototype's queue code (pests at the same time keep their order). A mutation check
  turned 4 tests red. A headless smoke run of wave 1 (scratch copy that starts the wave and prints
  events): six ants from the right at 1.0–10.0 s, the egg broken at 22 s, lost once, no errors.
  His round: "hat gepasst" — M1 closed.

- **2026-10-24 (second session)** — Read this project and Chief Wants His Cut (roadmap,
  architecture, CLAUDE.md, conventions, settings, hooks, all skills). Set up the same structure
  here: `architecture.md`, `roadmap.md` (M0–M4 with gates), `docs/conventions.md`,
  `docs/lessons/` (eleven lessons carried over as priors), `docs/decisions/README.md`, the seven
  skills rewritten for this game, `.claude/settings.json` with a Stop hook and a pre-commit hook
  (`tools/hooks/check-on-change.sh`: build; headless boot when content changed; tests once they
  exist — tried in a scratch copy: a broken JSON and a broken C# file are reported, a commit with
  a broken JSON is refused, a clean tree is silent). ADR 002 written after his "A". `git init`
  and the first commits. Then the grill of proposal A (nine questions, all answered) → ADR 003.
  The new skills could not be called by name in that session (Claude Code loads them at start);
  the grill followed `grill-me/SKILL.md` by hand.
