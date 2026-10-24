# Close only this game's process — other Godot projects run on this machine

**The lesson:** to show Sezgin a change, the running game is closed, built anew and started again.
Closing it by name — every process called `Godot_v*` — also aims at whatever else Godot is doing
on the machine (Chief Wants His Cut, an Android export of another project). Close the one process
whose command line is this project's.

## Where it comes from

Carried over from Chief Wants His Cut (2026-10-12): the restart command closed every `Godot_v*`;
one afternoon two of them were a headless export of another project. They had no window, so
`CloseMainWindow` did nothing — with `Stop-Process` the export would have been killed midway.

## How to apply

- Pick the process by its command line. Chief Wants His Cut also starts with `--path game`, so a
  relative `game` says nothing about which project. **Claude starts this game with the absolute
  path** (`--path "C:\Users\kings\Desktop\Claude\rettet-das-ei\game"`) and closes what matches it;
  a game started by "Spiel starten.bat" (relative `--path game`) is two processes: the
  `…_console.exe`, whose parent is the `cmd.exe` running that `.bat`, and the window
  `…_mono_win64.exe`, whose parent is that console exe — so look at the parent **and the
  grandparent** (2026-10-24: a parent-only match found the console, whose `CloseMainWindow` does
  nothing, and missed the window). Close the window; the console ends with it:

  ```powershell
  function Cmd($id) { (Get-CimInstance Win32_Process -Filter "ProcessId=$id") }
  $all = @(Get-CimInstance Win32_Process -Filter "Name like 'Godot_v%mono%'" |
      Where-Object { $_.CommandLine -notmatch '--headless' })
  $mine = @($all | Where-Object {
      $parent = Cmd $_.ParentProcessId
      $grand = if ($parent) { Cmd $parent.ParentProcessId }
      $_.CommandLine -match 'rettet-das-ei\\game' -or $parent.CommandLine -match 'rettet-das-ei' -or $grand.CommandLine -match 'rettet-das-ei'
  })
  "$($all.Count) Godot, $($mine.Count) of this game"
  foreach ($p in $mine) { [void](Get-Process -Id $p.ProcessId -ErrorAction SilentlyContinue).CloseMainWindow() }
  ```

- Close, never kill: `CloseMainWindow` ends the game as a click on its window's cross does.
- Read the count back — how many matched, how many are left.
- A headless run started by Claude ends by itself with `--quit-after`; never a reason to close
  something.
- Say that the game was restarted. (There are no saves yet: a restart loses the round that runs.)
