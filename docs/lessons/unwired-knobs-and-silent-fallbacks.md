# Unwired knobs and silent fallbacks

**Content values nobody reads, helpers nobody calls, and fallbacks that substitute different data
all fail the same way: zero errors, plausible behaviour, weeks of silent wrongness. Prove the knob
is wired in the same change; make every fallback announce itself.**

## Where it comes from

Carried over from Chief Wants His Cut (and Hollowdeep and dungeonmaster): a setting that was never
read produced four unrelated-looking symptoms; a wrong input silently fell back to example data.
Both were found only by reading logs after a bad live session.

## How it applies here

- **Content fields.** A new JSON field that nothing reads looks like tuning but does nothing —
  proposal A will add many (a slow's strength, a pierce count, a bomb radius). When adding one,
  grep for its reader in the same change; from M1 a test fails without it.
- **Silent defaults.** `ContentDb` rejects unknown properties and every field is
  `[JsonRequired]`, so a typo can't fall back to a default — keep it that way.
- **Visual keys.** An unknown key draws magenta and warns once, deliberately loud.

## The correction

When adding a knob or a helper, grep for its reader/caller in the same change. Any fallback that
substitutes different data announces itself when it triggers.
