# The headless boot never lets go of a piece — read the hand's path through by hand

**The lesson:** build, tests and a headless boot were green, and the game crashed on the first
drop Sezgin made. Nothing in the boot buys, picks up, turns or lets go of a piece, so a change to
the hand (`Main.StartHold` / `UpdateHeld` / `Drop` / `Unglue`) is checked by nothing but his
play. After touching it, walk each path once in the code — in the order the lines run — before
calling it built: what does every helper read, and has the caller already changed it?

## Where it comes from

2026-10-26, lifting a glued group: `PlaceHeld` and `HeldAll` read `_held`, and `Drop` sets
`_held = null` on its first line (so the sell path and a broken piece can't leave it set). Every
drop threw a `NullReferenceException` before `EndHold`; the pieces stayed frozen in the air and
he said "das Spiel hat sich grad aufgehängt". `run.log` had the stack trace at once.

## How to apply

- Helpers of the hand take the piece as an argument, not from `_held` (as they do since then).
- When he says it hangs, read `logs\run.log` first — a C# exception there leaves the game
  running but stuck, which looks like a hang.
- Say which of his inputs the round's checks did not touch ("play-unverified: the drop itself").
