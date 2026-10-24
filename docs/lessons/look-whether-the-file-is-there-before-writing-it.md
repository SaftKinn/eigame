# Look whether the file is there before writing it

**The lesson:** before a "new" file is written whole — a test class above all — look whether a
file of that name exists (`Glob`, `git ls-files <path>`). If it does, add to it.

## Where it comes from

Carried over from Chief Wants His Cut (2026-10-10): two tests were written to a "new" `DayTests.cs`
that already held eight; the write replaced it. The build was green and the run said
"Bestanden" — with 415 tests where 421 had been a minute before. Only the count gave it away.

## How to apply

- A file named after what it holds (`WaveTests`, `ContentTests`, a lesson, a skill) is likely to
  exist already — glob before choosing where new content goes.
- **Read the number of tests after every run** (from M1), not only the word "Bestanden". A run
  that passes with fewer tests than before is a failure of another kind; `progress.md`'s state
  header holds the last count.
- A written-over file that was committed is one `git checkout` away; one that was not is gone —
  another reason to commit before a sweep.
