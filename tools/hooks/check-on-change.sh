#!/usr/bin/env bash
# The one check of this repo, used two ways:
#   Stop hook (.claude/settings.json -> hooks.Stop): `bash tools/hooks/check-on-change.sh`
#     looks at the working tree; SILENT on success, so green turns cost nothing. A failure is
#     printed to the terminal (non-blocking, exit 1); it is not pushed into the model context.
#   pre-commit (tools/hooks/pre-commit): `check-on-change.sh --staged`
#     looks at what is staged; a failure aborts the commit.
#
# What it checks, only when game/scripts/, game/content/, sim/ or tests/ changed:
#   1. dotnet build of the game (the Godot project compiles everything under game/);
#   2. content changed -> a short headless boot of the game: ContentDb.Validate runs in
#      Main._Ready, so a broken JSON shows up as "Content is broken" (until M1's tests do it);
#   3. once tests/RettetDasEi.Sim.Tests exists (M1, ADR 002): dotnet test.
set -uo pipefail

cd "$(git rev-parse --show-toplevel 2>/dev/null || pwd)"

PROJECT="game/RettetDasEi.csproj"
TESTS="tests/RettetDasEi.Sim.Tests"
WATCH=(game/scripts game/content sim tests)

if [ "${1:-}" = "--staged" ]; then
  changed="$(git diff --cached --name-only -- "${WATCH[@]}" 2>/dev/null || true)"
  content="$(git diff --cached --name-only -- game/content 2>/dev/null || true)"
  tag="pre-commit"
else
  changed="$(git status --porcelain -- "${WATCH[@]}" 2>/dev/null || true)"
  content="$(git status --porcelain -- game/content 2>/dev/null || true)"
  tag="check-on-change"
fi

# Common case: nothing under the watched paths changed -> instant, silent.
[ -z "$changed" ] && exit 0

fail() {
  {
    echo "[$tag] $1"
    # msbuild prints every error twice: each line once.
    echo "$2" | grep -E "error|Error|ERROR|Failed|failed|Assert|Exception|Content is broken|  - |^units|^enemies|^waves|^game" | awk '!seen[$0]++' | head -40
  } >&2
  exit 1
}

# 1. Build.
if ! output="$(dotnet build "$PROJECT" -nologo -v q 2>&1)"; then
  fail "dotnet build FAILED (changes under game/scripts, game/content, sim or tests):" "$output"
fi

# 2. Content changed -> boot the game headless for a moment and look for content errors.
if [ -n "$content" ]; then
  godot=""
  base="$(cygpath -u "${LOCALAPPDATA:-}" 2>/dev/null || true)"
  for g in "$base"/Microsoft/WinGet/Packages/GodotEngine.GodotEngine.Mono_*/Godot_v4.6.2-stable_mono_win64/Godot_v4.6.2-stable_mono_win64_console.exe; do
    [ -f "$g" ] && godot="$g" && break
  done
  if [ -z "$godot" ]; then
    echo "[$tag] content changed, but Godot 4.6.2 .NET was not found -- content NOT checked." >&2
    exit 1
  fi
  [ -d game/.godot ] || "$godot" --headless --path game --import >/dev/null 2>&1
  output="$("$godot" --headless --path game --quit-after 120 2>&1)"
  # A here-string, not `echo | grep -q`: grep -q stops reading at the first match, echo dies of
  # SIGPIPE and pipefail turns the match into a miss (seen 2026-10-24).
  if grep -qE "Content is broken|SCRIPT ERROR|Unhandled exception|ERROR: .*Exception" <<< "$output"; then
    # The message without stack traces, each line once (a failed _Ready repeats in every frame).
    {
      echo "[$tag] headless boot found errors (changes under game/content):"
      echo "$output" | grep -vE '^[[:space:]]+(at |\[[0-9]+\]|C# backtrace)|^$|^Godot Engine' | awk '!seen[$0]++' | head -20
    } >&2
    exit 1
  fi
fi

# 3. Tests, once the rules core exists (M1).
if [ -d "$TESTS" ]; then
  if ! output="$(dotnet test "$TESTS" --nologo -v q 2>&1)"; then
    fail "dotnet test FAILED:" "$output"
  fi
fi

exit 0
