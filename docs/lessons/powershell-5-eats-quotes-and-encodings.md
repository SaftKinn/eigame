# Windows PowerShell 5.1 eats quotes and encodings

**Summary:** Text with typographic quotes, umlauts or a backtick does not survive a PowerShell 5.1
command line — edit such text with the Edit tool, and read and write files through .NET with UTF-8
named.

## Where it comes from

Carried over from Chief Wants His Cut (2026-10-10, three times in one session): sentences in „…“
were a parse error (PowerShell takes typographic quotes for string delimiters); a backtick inside
a double-quoted string became a carriage return; `Get-Content | Set-Content -Encoding utf8`
turned every "—" into "â€”" (5.1 reads a file without a BOM as ANSI).

## How to apply

- Everything the player reads here is German (Ei, Brett, Käseblock, „Welle geschafft!“): a change
  to such text, or to source with quotes or backticks, goes through the Edit tool.
- Where a script must touch file content: `[IO.File]::ReadAllText` / `WriteAllText` with
  `New-Object Text.UTF8Encoding($false)`. Never `Get-Content` without `-Encoding utf8`, never
  `Set-Content` for a file with umlauts.
- Single-quoted strings for anything with `$` or a backtick; no typographic quotes in a command.
- A replace that matters reports what it did not find.
