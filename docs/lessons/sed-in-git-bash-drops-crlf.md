# `sed -i` in Git Bash writes a CRLF file back with LF

**The lesson:** `sed -i` (and awk, `perl -0pi`) in Git Bash writes a whole file back with LF, and
turns a `\n` in an inserted text into a real line break. Git hides the first — the diff shows only
the line that was meant — but anything that compares text across files sees the difference.

## Where it comes from

Carried over from Chief Wants His Cut: one `sed -i` on a test helper turned an unrelated test red
(a two-line string from a CRLF file no longer found in an LF one); a `sed -i '111i\…'` whose text
held the JSON escape `\n` split one entry into three lines in five language files, and no test
read them; a `perl -0pi` whose pattern held `\n` matched nothing in a CRLF file and said nothing;
a backslash in a Windows path in sed's replacement became a tab.

## How to apply

- Change source and anything the player reads through the **Edit tool** — a text with `\n`,
  quotes or umlauts never goes through sed or a heredoc.
- Check every stream edit with a grep; print the changed line back.
- Write patterns as `\r?\n`. A CR is counted with `tr -cd '\r' < file | wc -c` (`grep -q $'\r'`
  finds none in Git Bash where there is one).
- A look at an older state is `git worktree add <dir> HEAD`, not a stash — a stash pop rewrites
  line endings the other way.
- There is no Python on the desktop machine (`python` opens the Store).
