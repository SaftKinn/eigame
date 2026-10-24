# A changed picture needs Godot's import before the game is started

**The lesson:** a file under `game/` that Godot has imported once (`<file>.import` beside it) is
used from its copy in `game/.godot/imported/`, not from the file. Overwriting the file changes
nothing in a game started from the command line: only the editor, or an import run, makes the copy
anew.

## Where it comes from

Carried over from Chief Wants His Cut (2026-10-10): a sound was swapped, the build was green, the
game was started for Sezgin — and he heard the old one.

## How it applies here

- `UnitVisual` / `EnemyVisual` load `res://art/units/<key>.png` and `res://art/enemies/<key>.png`
  through `ResourceLoader` — an imported resource. A **new** picture needs one import before it is
  seen at all; a **changed** one shows the old picture until imported again.
- "Spiel starten.bat" runs the import before every start, so his path is safe; a command-line
  start by Claude needs `<godot> --headless --path game --import` first.
