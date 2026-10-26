# ADR 003 — Every food a job (proposal A)

- **Status:** Accepted (grilled 2026-10-24; built in M2 the same day, play-unverified — see the addendum)
- **Date:** 2026-10-24
- **Refs:** decision log D10–D12 in `progress.md`, `roadmap.md` M2, `architecture.md` §5.7;
  the boundary of ADR 002; no frames, ADR 001

## Context

Sezgin's verdict on the prototype (2026-10-24): "Das Spiel ist cool. ABER jedes Essen fühlt sich
gleich an, nur die Formen unterscheiden sich." Proposal A from the other chat gives each food a
job: Käse sticky, Toast bounces attackers back, Gurke pierces, Senf leaves a slowing puddle,
Pfeffer makes pests sneeze, Tomate bursts like a bomb. Grilled with him on 2026-10-24, one fork at
a time (nine questions). Points marked *(default)* are Claude's recommendation stated in the grill
without a question of their own; he did not object.

## Decision

**Tomate — a bomb, no longer a shooter.**
1. It shoots nothing. Cheap, round, it rolls — its rolling becomes the job, not a flaw.
2. It bursts at **any touch with a pest**: a pest walking into it, or the tomato rolling or falling
   onto one. Once; the tomato is gone.
3. The burst **damages pests** within its radius, and **pushes own food** within it — a real
   impulse, the egg included — **without damaging food**. A tomato beside the egg is a risk.
   Full damage within the radius *(default)*.

**Käse — glue that tears.**
4. What comes to rest on the cheese's **top** sticks to it: the two move as one. The sides do not
   stick *(default)*.
5. A **hard blow tears it loose** (a threshold from content), shown as stretching cheese strings.
   A piece torn loose sticks again once it comes to rest on the cheese *(default)*.
6. Picking a piece up always loosens its glue; what lay on it falls, as today *(default)*.

**Toast — a shield wall.**
7. A pest that strikes the toast is **thrown back** a distance and has to come on again; the toast
   takes only **a share of the push** (content), its damage as before. Flyers are thrown back too
   *(default)*.

**Gurke — slices that pierce.**
8. A slice flies on through up to **N pests** (content), full damage to each *(default)*.

**Senf — less splash, a puddle.**
9. The blob deals **less splash damage** than today and leaves a **puddle** on the ground for a few
   seconds; **walkers** in it move slower. Flyers pass over it. Puddles do not stack — the
   strongest counts *(default)*. It slows moving, not attacking *(default)*.

**Pfeffer — a sneeze that builds up.**
10. Every hit adds to a pest's **itch**; at **N hits** it sneezes — a small push back and a short
    pause in attacking — and the itch starts again. No chance involved. The itch is seen: the pest
    trembles more with each hit.

**How the player learns it.**
11. Each shop card says the job in one short sentence (the `description` in `units.json`); in play
    the job is seen — cheese strings, a mustard puddle, a sneeze cloud, a tomato splat, the toast
    springing back. No tooltips with numbers, no tutorial.

**How it is built.**
12. **All six in one step** (his pick — not food by food), after M1; one live gate for M2.
13. Every number above is a content field with a first guess, tuned by playing.

**Along ADR 002's boundary** — the physics' (Godot): who touches whom (the tomato's trigger, a pest
in a puddle, a slice passing a pest), the glue as a joint and the blow that strains it, every push
and throw-back as a real impulse or move, all juice. The core's: a bomb's damage to each pest in
reach; whether a blow tears the glue; the toast's share of the push and the throw-back distance;
how many pests a slice may still pass; a puddle's slow and which one counts; a pest's itch, when it
sneezes and how long its pause lasts.

## Alternatives

- **Tomate:** keep shooting seeds and burst as well (weaker job, stronger food); burst only when its
  health is gone. **Trigger:** only when rolling with speed (often lies useless); only when struck.
  **Own food:** untouched by the burst (no drama); pushed and damaged (a tomato in the tower almost
  always a mistake).
- **Käse:** only very high friction (hardly different from today — cheese already has 1.0); glued
  for good until the cheese breaks (too rigid, the tower would hardly wobble).
- **Toast:** only throwing back (buys time, does not keep the tower from tipping); a trampoline
  that bounces every pest before it strikes (too strong).
- **Pfeffer:** a sneeze at every hit (at ~3 shots a second a pest would never attack); a chance per
  hit (harder to plan).
- **Senf:** a puddle only (no use alone); today's damage and a puddle (too strong for the price).
- **Teaching:** tooltips with numbers; a hint at the first purchase of each.
- **Order:** simple to tricky (Gurke → … → Käse), or the building blocks first — he chose all at once.

## Consequences

- M2's live gate is one round of play over all six; if something does not fit, the round has to
  find which of the six it was — say what to watch for each in the play script.
- With the tomato no longer shooting and the mustard weaker, the steady damage rests on the pickle
  and the pepper: the waves may need tuning so they can still be cleared — check that first in the
  play round (play-unverified until then).
- The glue is the riskiest physics (joints in Godot 4.6.2, a threshold on a blow): try it in the
  game before building around it (lesson `read-the-installed-library-source`).
- New content fields for all six; each is read and validated (lesson
  `unwired-knobs-and-silent-fallbacks`), each rule of the core gets its tests (M1's test project).

## Addendum (2026-10-24, built)

Built in M2 the same day (`architecture.md` §5.7), play-unverified. Where the build had to fill
in what the grill left open: the sneezing pest **stands still** for its pause (point 10 said "a
pause in attacking" — standing reads clearer, and a walker that walks on would only start
attacking again); the glue is **two pins** per pair (one pin would let the piece turn); a blow
tears the glue of the piece it hits, as the cheese or as the piece on it; picking up shows
cheese strings too, a break does not. First numbers: tomato 15 gold, damage 60, radius 100,
push 350; cheese tears at 400 (a meatball's 480 tears, an ant's 110 does not); toast takes
35 %, throws back 90 px; pickle pierces 3; mustard 8 damage, a puddle of radius 70 for 4 s at
40 % speed; pepper sneezes at 6 hits, 1.2 s pause, 30 px back.

**After his first round (2026-10-24):** a glued tower jumped and drifted by itself in the build
phase (his recording). Cause measured: the pins and the pair's contact fought; the pair's
collision is now off (a resting stack: 836 px/s before, 0 after). At his word the glue is
**seen**: a seam while stuck and a glowing "klebt hier" while a piece in the hand would stick.
Glue at the cheese's sides ("vertikal") was asked and put off — "es würde das Spiel aktuell nur
noch leichter machen".

**After his second round (2026-10-24):** his screenshot showed upright cheeses glued by their
sides; he wants no side gluing and the seam "wie vorher" (gone). Now the cheese wears a red wax
rind and **only its cut face sticks, while it faces up** (within 45°) — point 4's "top" is the
cheese's own top, not the world's. The "klebt hier" preview stays.

**After the start of his third round (2026-10-26):** "es soll doch jede seite des käßes kleben
entferne den roten rand" — asked whether he means all round, sides included, or whichever face
is up: **all round**. The wax rind is gone; every face sticks — top, bottom and sides — and a
cheese beside a cheese glues to it. What changed since he put side glue off twice: ADR 005 made
every food but the cheese slippery, so the cheese is now the one thing that holds a tower; side
glue makes it worth more, and the game easier — the next round shows by how much. Then: "seitlich
muss es magnetisch angezogen werden sonst fällt der käse einfach runter oder das bauteil" — of a
snap when let go or a pull in the physics all the time, his pick: **the snap**. Let go beside a
cheese's side, the held piece or cheese moves flush against it and sticks at once. His screenshot
then showed it not working beside an upright cheese, and "klebt hier" too far off — "man soll
präzise bauen können": faces and gaps are now measured on the real outlines, edge to edge, with a
12 px reach, and the snap works on every face. And: "wenn man linksklick auf den käseblock macht
soll man auf ein käsedreieck umschalten können" (three 120° angles cannot be — asked: an
equilateral triangle; switched by a right click on the shop card, since the left click buys) →
the **Käsedreieck**, the block's other shape, gluing on all three faces. Pieces were turned in
45° steps ("die anderen winkel wären sonst überflüssig") and back to 15° the same hour — "sonst
das 3eck nicht mit jeder seite befestigen": 15° is the coarsest step with both 45° and 60°/120°.
