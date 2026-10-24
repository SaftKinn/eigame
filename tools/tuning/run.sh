#!/usr/bin/env bash
# (copied to the tuning dir by tools/tuning/setup.sh; reads the harness lines the game prints)
# run.sh <label> [towers...]: each tower twice against the content in game/content, one line each
G="$(ls -d "$(cygpath -u "$LOCALAPPDATA")"/Microsoft/WinGet/Packages/GodotEngine.GodotEngine.Mono_*/Godot_v4.6.2-stable_mono_win64/*console.exe)"
label="$1"; shift; towers="${*:-small mid big}"
for t in $towers; do for r in 1 2; do
  out="$("$G" --headless --path game --fixed-fps 60 --quit-after 40000 -- --tower=$t 2>&1)"
  result="$(grep -o 'RESULT.*' <<< "$out")"
  waves="$(grep -o 'cleared [0-9] gold=[0-9]* crowd=[0-9]* tilt=[0-9]* drift=[0-9]*' <<< "$out" | tr '\n' ' ')"
  bites="$(grep -oF '[fly] bites' <<< "$out" | wc -l)/egg:$(grep -cF '[fly] bites egg' <<< "$out")"
  echo "$label $t#$r: $(grep -o 'cost=[0-9]*' <<< "$out") $result | $waves| fly bites (all/egg) $bites"
done; done
