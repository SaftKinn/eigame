using System;
using Godot;
using RettetDasEi.Sim;
using FileAccess = Godot.FileAccess;

namespace RettetDasEi;

/// <summary>
/// The game's content, loaded once from res://content/ through the rules core's loader (ADR 002).
/// Only Godot's FileAccess can read res:// — in an exported game the files are inside the .pck.
/// </summary>
public static class GameContent
{
    public static ContentDb Db { get; private set; } = null!;

    public static void Load() => Db = ContentDb.Load(ReadFile);

    static string ReadFile(string name)
    {
        string path = $"res://content/{name}";
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file == null)
            throw new InvalidOperationException($"{path}: cannot open ({FileAccess.GetOpenError()})");
        return file.GetAsText();
    }
}
