# `grep -q` in a pipe under `pipefail` turns a match into a miss

**The lesson:** in a bash script with `set -o pipefail`, `echo "$output" | grep -q pattern` can
report "not found" for text that is there: `grep -q` stops reading at the first match and exits,
`echo` then dies of SIGPIPE (exit 141), and `pipefail` makes the pipeline's status that failure.
Big outputs show it, small ones often don't — so it passes a quick try.

**What happened (2026-10-24):** the Stop hook's headless boot printed "Content is broken" for a
JSON broken on purpose, and the hook still ended green. Found only because the hook was tried
against a broken file before it was trusted; `bash -x` showed the grep running and the `if` failing.

**How to apply:**
- Match a variable with a here-string, no pipe: `grep -qE "…" <<< "$output"`.
- Try every check against the failure it is meant to catch, not only against a clean tree — a
  check that is never red proves nothing (the same rule as the `tdd` skill's).
