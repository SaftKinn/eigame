#!/usr/bin/env bash
# setup.sh [dir]: a fresh copy of game/ and sim/ with the tuning harness hooked into Main
# (default dir: tuning-run/ in the repo root, git-ignored). Then: cd <dir> && bash run.sh <label> [towers]
# Towers: small (115 gold), w3 (180), mid (250), bigless (345), big (390), his (245, his winning
# tower of 2026-10-24), pepperweak / pepperstrong (four peppers high on a toast / a cheese base).
# Change content in <dir>/game/content, measure, then copy the files into the repo and git diff them.
set -euo pipefail
R="$(cd "$(dirname "$0")/../.." && pwd)"; H="$R/tools/tuning"; T="${1:-$R/tuning-run}"
rm -rf "$T"; mkdir -p "$T"; cp -r "$R/game" "$R/sim" "$T/"
rm -rf "$T/game/.godot/mono" "$T/sim/RettetDasEi.Sim/bin" "$T/sim/RettetDasEi.Sim/obj"
cp "$H/SmokeHarness.cs" "$T/game/scripts/"; cp "$H/run.sh" "$T/"
M="$T/game/scripts/Main.cs"
sed -i 's|^        _hud.ShowMessage("Zieh Essen|        HarnessTower();\n        _hud.ShowMessage("Zieh Essen|' "$M"
sed -i 's|^        _shake = Mathf.MoveToward|        HarnessTick(delta);\n        _shake = Mathf.MoveToward|' "$M"
sed -i 's|^            switch (e)$|            HarnessEvent(e);\n            switch (e)|' "$M"
E="$T/game/scripts/Enemies/Enemy.cs"
sed -i 's|^        AttackOrder = ++_attacksBegun;|        AttackOrder = ++_attacksBegun; if (IsFlyer) GD.Print($"[fly] bites {food.Def.Id}");|' "$E"
hooks="$(grep -c Harness "$M")"; flylog="$(grep -c '\[fly\]' "$E")"
echo "hooks: $hooks (want 3), fly log: $flylog (want 1)"
[ "$hooks" = 3 ] && [ "$flylog" = 1 ] || { echo "a hook did not apply - Main.cs or Enemy.cs changed; fix the sed lines" >&2; exit 1; }
(cd "$T" && dotnet build game/RettetDasEi.csproj -nologo -v q 2>&1 | grep -E " error " | head -5 || true)
echo "ready: cd \"$T\" && bash run.sh <label> [towers]"
