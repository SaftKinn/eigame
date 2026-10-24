# Parity by construction, not by discipline

**When the same value or decision is computed in two places (the shooter and the range overlay,
the core and the HUD, a tooltip and the rule), it WILL silently desync — the repeated winner is
one shared pure function both paths call.**

## Where it comes from

Carried over from Chief Wants His Cut (and Hollowdeep and dungeonmaster before it): every second
copy drifted, every time silently, and surfaced as a live inconsistency long after it was pasted.

## How it applies here

- **Range by height** is read by the shooter (`UnitBody.EffectiveRange`) and drawn by the range
  overlay. Both must ask one function — in M1 it moves into the core as exactly one.
- **The sell refund** is shown as a hint while dragging and paid on the drop: one function
  (`Main.RefundFor` today, the core's from M1).
- **Effects of proposal A** (a slow, a sneeze): what the pest does and what the HUD or a tooltip
  says it does read the same rule.

## The correction

The second path always arrives later, when copy-adapt is the path of least resistance — that is
exactly when to extract the shared function instead. If an invariant lives only in a comment or
in parallel `if` blocks, promote it to one owned, tested place.
