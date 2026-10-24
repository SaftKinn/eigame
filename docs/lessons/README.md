# Lessons — recurring corrections, confirmed the hard way

A memory layer distinct from the ADRs: **ADRs hold decisions; lessons hold corrections and
confirmed approaches that kept getting rediscovered** across sessions.

**How to use:** skim this list at session start (part of the CLAUDE.md ritual); open a full
lesson only when its summary touches the task. **How to maintain:** record new corrections as
they happen — update the existing lesson rather than duplicating it, delete lessons proven wrong,
keep this index in sync. Don't record what CLAUDE.md, `docs/conventions.md` or an ADR already
holds — link there instead (the `SetAnchorsPreset` trap of 2026-10-24 is a CLAUDE.md gotcha, so
it has no lesson here).

The lessons below are **carried over from Chief Wants His Cut** (2026-10-24; some of them from
Hollowdeep before that), rewritten for this project. Here they are priors until this project
confirms or refutes them. Lessons learned in this project go under their own heading.

## The lessons

### Code structure
- [parity-by-construction](parity-by-construction.md) — The same value computed in two places
  WILL desync; one shared pure function for the shooter and the overlay, the core and the HUD.
- [byte-exact-moves-stepwise-gates](byte-exact-moves-stepwise-gates.md) — Refactors (M1's move
  into the core above all): move, don't retype; build and play-check after every step.

### Config, data & dependencies
- [unwired-knobs-and-silent-fallbacks](unwired-knobs-and-silent-fallbacks.md) — A content field
  nobody reads and a default that hides a missing value fail silently; prove the wiring, make
  fallbacks loud.
- [read-the-installed-library-source](read-the-installed-library-source.md) — Godot's C# API
  (and its physics) is the truth, not the docs or memory; try it on the installed 4.6.2.

### Working with Sezgin
- [confirm-ambiguous-reference-before-building](confirm-ambiguous-reference-before-building.md) —
  "das 2te" fits several orderings; name the candidate back before building. One numbered list
  to a message. The fault he names is the measure, not the cure he names with it.
- [ask-what-the-screen-is-before-building-it](ask-what-the-screen-is-before-building-it.md) —
  Grilling a feature's rules doesn't settle how it looks and is seen; ask that too, and let him
  pick looks by seeing them.

### Process and tools on this machine
- [look-whether-the-file-is-there-before-writing-it](look-whether-the-file-is-there-before-writing-it.md) —
  A "new" file can replace one that exists; glob first, and read the count of tests after a run.
- [powershell-5-eats-quotes-and-encodings](powershell-5-eats-quotes-and-encodings.md) — „…“,
  umlauts and backticks do not survive a PowerShell 5.1 command: texts go through the Edit tool.
- [sed-in-git-bash-drops-crlf](sed-in-git-bash-drops-crlf.md) — `sed -i`, awk and perl write a
  CRLF file back LF and turn `\n` in a replacement into a line break; texts through the Edit tool.
- [close-only-this-games-process](close-only-this-games-process.md) — Other Godot projects run on
  this machine: close the game by its command line (`--path game`), never by name, never kill.
- [a-changed-sound-or-picture-needs-godots-import](a-changed-sound-or-picture-needs-godots-import.md) —
  An imported file is used from `.godot/imported/`: after writing over a picture, `--import`
  before starting the game.

### Learned in this project

_None yet._
