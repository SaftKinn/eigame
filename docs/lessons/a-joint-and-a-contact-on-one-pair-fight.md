# A joint and a contact on one pair of bodies fight — measure a resting stack

**The lesson:** two Godot bodies pinned by joints *and* still colliding with each other are held
twice; the solver trades the two off and puts energy in. A glued stack that should rest jumps,
creeps and never sleeps. When pieces are joined, switch their collision with each other off
(`Joint2D.DisableCollision = true`) — the joints alone hold them.

**What happened (2026-10-24):** the cheese glue (ADR 003) pinned pieces with
`DisableCollision = false` "so the contact stays solid". Sezgin's recording showed a glued tower
wobbling and sliding across the board in the build phase, with no pest on screen. A scratch copy
with a resting stack and the fastest piece logged every second: 836 px/s (the egg), then a
permanent 6 px/s creep; without glue 0; with glue and the pair's collision off 0.

**How to apply:**
- Before naming the cause of "the physics does strange things", log the fastest body of a
  resting scene per second in a scratch copy (headless, `--fixed-fps 60`), once with the
  suspect and once without — a number settles it.
- A body that is part of a joint gets its collision with its partner off unless there is a
  measured reason to keep it.
- His recordings: cut a contact sheet with ffmpeg (`fps=1,scale=480:-1,tile=4x4`) and look at it
  before asking him what he meant; zoom a moment that looks wrong — the "flying" pickle was his
  own hand dragging it.
