---
name: session-ritual
description: Use at the start of a working session for the handshake (read CLAUDE.md → the State header at the top of progress.md → the latest ADR → lessons index, then state where we are and what's next), and at the end of a session or on "wrap up" / "update progress" to update progress.md within its caps, check every .md file, scaffold the next-numbered ADR and commit. Keeps continuity across context clears and model switches.
---

# Session ritual — start handshake, end wrap-up, ADR scaffold

State lives on disk so the project survives context clears and model switches. This skill is the
one canonical procedure; it formalizes the prose in `CLAUDE.md`.

## Start handshake

1. Read in order: `CLAUDE.md` → ONLY the `## State header` at the top of `progress.md` → the
   **highest-numbered** file in `docs/decisions/` → `docs/lessons/README.md` (skim). The rest of
   `progress.md`, older ADRs and `architecture.md` are on-demand reads.
2. State in 2–3 sentences (German): where we are, what we're about to do. **Don't touch files
   until this handshake is done.** If the WIP limit (max 3 open live gates) makes this a playtest
   session, say so here.
3. **Before touching an area or starting a milestone:** read the ADR(s) the milestone → ADR map in
   `progress.md` names for it. On-demand, but mandatory for that case.

## End-of-session wrap-up (trigger: "wrap up" / "update progress", or unprompted at session end)

Update `progress.md`:
- `## State header` — milestone, newest round, next step, open live gates
- `## Current focus` — only if the milestone changed
- `## Last session` — what we actually did
- `## Next concrete step` — the specific next action, not a vague goal
- `## Open questions` — anything that surfaced but isn't actionable yet
- the `## Milestones` table — when a gate was met, with the evidence (his words, a build, a test count)
- the `## Decision log` — a row for every decision taken (`→ ADR NNN` where there is one)

**Check EVERY `.md` file (every wrap-up, before reporting done):** `progress.md` is not the only
file that goes stale. List them all — `git ls-files -co --exclude-standard '*.md'` — and go
through the list: for everything the session changed (a feature, a key, a file or folder, a setup
step, a default, a count) check what each file says about it and fix what no longer holds. The
usual suspects: `README.md` (status line, folder table), `SETUP.md` (steps, controls, open items),
`architecture.md` (the layout tree in §3 and the section the change touches),
`docs/conventions.md` (repo-layout table, runtime, hooks, troubleshooting), `roadmap.md` (a
"before building" step that is now done), `docs/art-prompts.md` (a new unit or pest needs its
row), the PRD in `docs/plans/` for the feature, the skills under `.claude/skills/` (file names and
procedures they mention). ADRs are history — don't rewrite a decision; a dated addendum is fine
when the built thing has moved on. Say in the wrap-up message how many `.md` files were checked
and which were changed.

**Keep it lean (enforced every wrap-up):** the live `progress.md` holds only the current state.
When you prepend a new `## Last session` entry, move the *previous* one to
`docs/progress-archive.md` (create it on the first rotation) — keep only the newest 1–2 live.
Rotate resolved open questions there too. Caps: State header max 25 lines; `## Current focus` max
2 blocks; decision-log rows max 2 lines ("what + one-clause why + → ADR NNN"); `progress.md` over
400 lines → rotate before committing. The decision log and the milestone → ADR map stay fully live.

**WIP limit (checked at wrap-up, announced in the next handshake):** max 3 open live gates (a live
gate = a "Verification (live)" item in `roadmap.md` that only Sezgin playing can close, or a tuning
change that hasn't been played yet). If this round would leave a FOURTH open, it doesn't start —
the next session is a playtest session; set `## Next concrete step` to the shortest play script
that closes the oldest gates (what to buy, how to stack, which wave, what to look at). Exempt:
rounds that open no live gate (tooling, refactors covered by tests). Sezgin can override
explicitly ("WIP-Override") — note it in the wrap-up.

**Commit (every wrap-up, the last step):** a wrap-up ends with the work committed, without asking
first, and pushed once a remote exists (`git remote` empty → commit only, and say so). Order:
1. `progress.md` and the `.md` check above are done, and `progress.md` describes the state as it
   will be *after* the commit — no "not committed" left behind.
2. Commit in topical groups in the project's style (`units: …`, `enemies: …`, `content: …`,
   `ui: …`, `sim(<area>): …`, `tests: …`, `tools: …`, `docs: …`); code and its tests together,
   docs last.
3. The pre-commit hook has to pass. If it fails, fix the cause or stop and report — never bypass
   it with `--no-verify`.
4. `git push` (if there is a remote), then confirm the working tree is clean.
5. Name the commits in the wrap-up message.

Only this repository: changes a session made in another repo (Chief Wants His Cut) are reported,
not committed.

Wrap-up messages are for a fresh reader, in German: one plain sentence on what changed and why it
matters for play, then evidence, then at most five lines of mechanism — the rest goes in the ADR.
Anything not played by Sezgin is **play-unverified**.

Silence here is the failure mode that breaks continuity. Do it even if not asked.

## new-adr — scaffold the next decision record

Only when a decision weighed a real trade-off with alternatives seriously on the table. Next
sequential number, never reused: `docs/decisions/NNN-short-title.md`, plus a row in
`docs/decisions/README.md` and the decision-log row (`→ ADR NNN`) in `progress.md`. If it governs
an area, add it to the milestone → ADR map as well. **Only after Sezgin has agreed to the
decision** — a proposal is a message, not an ADR.

```markdown
# ADR NNN — Title

- **Status:** Proposed | Accepted | Superseded by ADR NNN
- **Date:** YYYY-MM-DD
- **Refs:** decision log DX in progress.md, optionally architecture.md §Y

## Context
## Decision
## Alternatives
## Consequences
```

If a decision changes a prior one, set the old ADR's status to `Superseded by ADR NNN` and keep it.
If the decision changes a design choice, update `architecture.md` in the same change.
