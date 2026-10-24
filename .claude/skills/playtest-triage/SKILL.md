---
name: playtest-triage
description: Use when Sezgin reports how a play session felt ("zu wackelig", "kippt zu schnell", "zu schwer", "ruckelt", "das Essen ist nutzlos"), pastes Godot output or the logs of "Spiel starten.bat", or brings playtest notes, and wants a fix or tuning round. Classifies each complaint (content tuning, physics feel, look, core bug, view bug, performance, design question), applies the right fix, checks it, commits per round, updates progress.md.
---

# Playtest triage — turn a play session into a fix round

A session was played, something felt off. The job is **classification first, smallest correct fix
second**. Most "feel" complaints are numbers in `game/content/`, not code.

## Procedure

1. **Read the evidence.** Sezgin's own words carry the most signal (lesson
   `confirm-ambiguous-reference-before-building`: the fault he names is the measure). Then the
   logs: `logs\dotnet-build.log`, `logs\import.log`, `logs\run.log`; the HUD values he quotes; a
   screenshot or a video he sends.
2. **Classify each complaint:**
   - **Tuning** — prices, gold, health, damage, cooldowns, ranges, waves → edit
     `game/content/*.json`. State old → new and the reasoning ("Ameise `push` 110 → 80: der Turm
     kippt schon in Welle 1"). Change few numbers at a time — one variable per playtest where
     possible.
   - **Physics feel** — slippery, jittery, topples too easily or never, a piece flies off →
     `mass`, `friction` (`units.json`), `push` (`enemies.json`), damping (`game.json`); only then
     the physics settings in `project.godot` (ticks, iterations). Measure one change at a time;
     say what he should try to see it.
   - **Look** — a drawing, a size, a colour, a tween that looks stiff → `DrawKit`, the visuals'
     tweens; more juice, never frames (ADR 001).
   - **Core bug** (from M1) — a wrong outcome of a rule (gold, a wave, an effect) → reproduce as a
     failing test first (`tdd`).
   - **View bug** — drawn wrong, HUD wrong, input wrong while the rule is right → fix in
     `game/scripts/`; check no rule leaked into the view (ADR 002's boundary).
   - **Performance** — measure before changing: how many bodies and pests at the drop, physics
     time vs. draw time.
   - **Design question** — "sollte Käse kleben?" → not a fix; add it to `## Open questions` or
     start `/grill-me`.
3. **Fix** per class. Check: `dotnet test tests/RettetDasEi.Sim.Tests`, `dotnet build
   game/RettetDasEi.csproj` and a headless boot. Code in the core: tests first.
4. **Commit per round**, scoped: `content: …`, `units: …`, `enemies: …`, `sim(<area>): …`.
5. **Record** via `session-ritual`: what changed and why, marked **play-unverified** — the next
   play session is the gate. Name exactly what Sezgin should check next time (which wave, what to
   build, what to look at), in German.

## Notes

- Every report is about the version *before* your change. Claim "Änderung drin, play-unverified,
  prüf X", never "behoben".
- Tuning that keeps coming back to the same number is a sign the mechanic, not the number, is
  wrong — say so. ("Every food feels the same" was such a sign: not a number, a mechanic — M2.)
- A played and unchanged tuning closes its live gate; note it in the milestone table.
