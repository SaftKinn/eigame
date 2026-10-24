using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using FileAccess = Godot.FileAccess;

namespace RettetDasEi;

/// <summary>
/// Loads and checks game/content/*.json once at start. Anything wrong stops the game with a
/// message naming the file and the field — never a silent default.
/// </summary>
public static class ContentDb
{
    public static GameDef Game { get; private set; } = null!;
    public static List<UnitDef> Units { get; private set; } = new();
    public static List<EnemyDef> Enemies { get; private set; } = new();
    public static List<WaveDef> Waves { get; private set; } = new();

    static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };

    static readonly string[] Shapes = { "box", "circle", "capsule", "egg" };
    static readonly string[] Projectiles = { "straight", "lob" };
    static readonly string[] Movements = { "waddle", "roll", "hop", "fly" };
    static readonly string[] Sides = { "left", "right", "both" };

    public static void Load()
    {
        Game = Read<GameDef>("res://content/game.json");
        Units = Read<List<UnitDef>>("res://content/units.json");
        Enemies = Read<List<EnemyDef>>("res://content/enemies.json");
        Waves = Read<List<WaveDef>>("res://content/waves.json");
        Validate();
    }

    public static UnitDef Unit(string id) =>
        Units.FirstOrDefault(u => u.Id == id) ?? throw new InvalidOperationException($"units.json: no unit '{id}'");

    public static EnemyDef Enemy(string id) =>
        Enemies.FirstOrDefault(e => e.Id == id) ?? throw new InvalidOperationException($"enemies.json: no enemy '{id}'");

    static T Read<T>(string path)
    {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file == null)
            throw new InvalidOperationException($"{path}: cannot open ({FileAccess.GetOpenError()})");
        string text = file.GetAsText();
        try
        {
            return JsonSerializer.Deserialize<T>(text, Options)
                ?? throw new InvalidOperationException($"{path}: is empty");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"{path}: {ex.Message}", ex);
        }
    }

    static void Validate()
    {
        var errors = new List<string>();
        void Check(bool ok, string message)
        {
            if (!ok)
                errors.Add(message);
        }

        Check(Game.StartGold >= 0, "game.json: startGold must be 0 or more");
        Check(Game.RangePerHeight >= 0f, "game.json: rangePerHeight must be 0 or more");
        Check(Game.LobGravity > 0f, "game.json: lobGravity must be above 0");
        Check(Game.SellRefund >= 0f && Game.SellRefund <= 1f, "game.json: sellRefund must be between 0 and 1");

        Check(Units.Select(u => u.Id).Distinct().Count() == Units.Count, "units.json: two units share an id");
        foreach (var u in Units)
        {
            string at = $"units.json '{u.Id}'";
            Check(u.Id.Length > 0, "units.json: a unit without id");
            Check(Shapes.Contains(u.Shape), $"{at}: shape must be one of {string.Join(", ", Shapes)}");
            Check(u.Width > 0f && u.Height > 0f, $"{at}: width and height must be above 0");
            Check(u.Mass > 0f, $"{at}: mass must be above 0");
            Check(u.Hp > 0f, $"{at}: hp must be above 0");
            Check(u.Cost >= 0, $"{at}: cost must be 0 or more");
            Check(Color.HtmlIsValid(u.Fill), $"{at}: fill '{u.Fill}' is not a colour");
            Check(Color.HtmlIsValid(u.Accent), $"{at}: accent '{u.Accent}' is not a colour");
            if (u.Attack is { } a)
            {
                Check(Projectiles.Contains(a.Projectile), $"{at}: attack.projectile must be one of {string.Join(", ", Projectiles)}");
                Check(a.Damage > 0f && a.Cooldown > 0f && a.Range > 0f, $"{at}: attack damage, cooldown and range must be above 0");
                Check(a.Projectile != "straight" || a.Speed > 0f, $"{at}: a straight shot needs a speed above 0");
                Check(a.Splash >= 0f && a.Recoil >= 0f, $"{at}: splash and recoil must be 0 or more");
            }
        }

        var egg = Units.FirstOrDefault(u => u.Id == Game.EggUnit);
        Check(egg != null, $"game.json: eggUnit '{Game.EggUnit}' is not in units.json");
        Check(egg == null || !egg.InShop, "units.json: the egg must not be in the shop");

        Check(Enemies.Select(e => e.Id).Distinct().Count() == Enemies.Count, "enemies.json: two enemies share an id");
        foreach (var e in Enemies)
        {
            string at = $"enemies.json '{e.Id}'";
            Check(Movements.Contains(e.Movement), $"{at}: movement must be one of {string.Join(", ", Movements)}");
            Check(e.Radius > 0f && e.Hp > 0f && e.Speed > 0f, $"{at}: radius, hp and speed must be above 0");
            Check(e.AttackCooldown > 0f, $"{at}: attackCooldown must be above 0");
            Check(Color.HtmlIsValid(e.Fill) && Color.HtmlIsValid(e.Accent), $"{at}: fill or accent is not a colour");
        }

        Check(Waves.Count > 0, "waves.json: no waves");
        for (int i = 0; i < Waves.Count; i++)
        {
            string at = $"waves.json wave {i + 1}";
            Check(Waves[i].Groups.Count > 0, $"{at}: no groups");
            foreach (var g in Waves[i].Groups)
            {
                Check(Enemies.Any(e => e.Id == g.Enemy), $"{at}: unknown enemy '{g.Enemy}'");
                Check(g.Count > 0 && g.Interval >= 0f && g.Start >= 0f, $"{at}: count must be above 0, start and interval 0 or more");
                Check(Sides.Contains(g.Side), $"{at}: side must be one of {string.Join(", ", Sides)}");
            }
        }

        if (errors.Count > 0)
            throw new InvalidOperationException("Content is broken:\n" + string.Join("\n", errors));
    }
}
