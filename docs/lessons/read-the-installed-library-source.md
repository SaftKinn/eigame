# Read the installed library's source, check its real contract

**When behaviour contradicts expectations, the truth is in the installed engine version — which
differs from the docs, from older versions and from memory. Code written from the mental model
confirms the wrong assumption.**

## Where it comes from

Carried over from Chief Wants His Cut (and Hollowdeep and dungeonmaster): hangs, dead branches and
silent zeroes were all explained by reading the *installed* library code, not the docs.

## How it applies here

- **Godot's C# API moves between minor versions.** Training data mixes Godot 3 and 4 —
  `KinematicBody2D`, the `export` keyword, `yield` are Godot 3. Check against the installed
  4.6.2's API (its `GodotSharp` package) before writing glue.
- **The physics is this game's core**, and its behaviour (contacts reported, sleeping, a frozen
  body's mode, joints for a sticky cheese) is what 4.6.2 does, not what a forum post says.
  When a plan rests on it, try it once in the game before building around it.
- **System.Text.Json** behaviour (`[JsonRequired]`, unmapped-member handling) is .NET 8's.

## The correction

Before fixing "our" bug in glue code, run against the exact installed version. Before building a
mechanic on a physics behaviour, try that behaviour.
