using System.Text.Json;
using System.Text.Json.Serialization;

namespace RettetDasEi.Sim;

/// <summary>
/// Loads and checks game/content/*.json once at start. Anything wrong throws with a message naming
/// the file and the field — never a silent default. The files are read through a function, so the
/// game (Godot's FileAccess on res://content/) and the tests (plain files) load the same way.
/// </summary>
public sealed class ContentDb
{
    public const string GameFile = "game.json";
    public const string UnitsFile = "units.json";
    public const string EnemiesFile = "enemies.json";
    public const string WavesFile = "waves.json";

    /// <summary>Every file the loader reads; a JSON file in game/content/ that is not here is a mistake.</summary>
    public static readonly IReadOnlyList<string> Files = new[] { GameFile, UnitsFile, EnemiesFile, WavesFile };

    public GameDef Game { get; }
    public IReadOnlyList<UnitDef> Units { get; }
    public IReadOnlyList<EnemyDef> Enemies { get; }
    public IReadOnlyList<WaveDef> Waves { get; }

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

    ContentDb(GameDef game, List<UnitDef> units, List<EnemyDef> enemies, List<WaveDef> waves)
    {
        Game = game;
        Units = units;
        Enemies = enemies;
        Waves = waves;
    }

    /// <summary>
    /// Reads every file through <paramref name="readFile"/> (given a file name such as
    /// "units.json", it returns the text) and validates them together.
    /// </summary>
    public static ContentDb Load(Func<string, string> readFile)
    {
        var db = new ContentDb(
            Read<GameDef>(readFile, GameFile),
            Read<List<UnitDef>>(readFile, UnitsFile),
            Read<List<EnemyDef>>(readFile, EnemiesFile),
            Read<List<WaveDef>>(readFile, WavesFile));
        db.Validate();
        return db;
    }

    public UnitDef Unit(string id) =>
        Units.FirstOrDefault(u => u.Id == id) ?? throw new InvalidOperationException($"units.json: no unit '{id}'");

    public EnemyDef Enemy(string id) =>
        Enemies.FirstOrDefault(e => e.Id == id) ?? throw new InvalidOperationException($"enemies.json: no enemy '{id}'");

    static T Read<T>(Func<string, string> readFile, string name)
    {
        string text = readFile(name);
        try
        {
            return JsonSerializer.Deserialize<T>(text, Options)
                ?? throw new InvalidOperationException($"{name}: is empty");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"{name}: {ex.Message}", ex);
        }
    }

    void Validate()
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
        Check(Game.FlyPatience >= 0f, "game.json: flyPatience must be 0 or more");
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
            Check(HexColor.IsValid(u.Fill), $"{at}: fill '{u.Fill}' is not a colour");
            Check(HexColor.IsValid(u.Accent), $"{at}: accent '{u.Accent}' is not a colour");
            if (u.Attack is { } a)
            {
                Check(Projectiles.Contains(a.Projectile), $"{at}: attack.projectile must be one of {string.Join(", ", Projectiles)}");
                Check(a.Damage > 0f && a.Cooldown > 0f && a.Range > 0f, $"{at}: attack damage, cooldown and range must be above 0");
                Check(a.Projectile != "straight" || a.Speed > 0f, $"{at}: a straight shot needs a speed above 0");
                Check(a.Splash >= 0f && a.Recoil >= 0f, $"{at}: splash and recoil must be 0 or more");
                Check(a.Pierce >= 1, $"{at}: attack.pierce must be 1 or more");
                Check(a.Projectile == "straight" || a.Pierce == 1, $"{at}: only a straight shot pierces (pierce 1 for a lob)");
                if (a.Puddle is { } p)
                {
                    Check(a.Projectile == "lob", $"{at}: only a lob leaves a puddle");
                    Check(p.Radius > 0f && p.Seconds > 0f, $"{at}: puddle radius and seconds must be above 0");
                    Check(p.Slow > 0f && p.Slow <= 1f, $"{at}: puddle slow must be above 0 and at most 1");
                }
                if (a.Itch is { } itch)
                {
                    Check(itch.Hits >= 1, $"{at}: itch hits must be 1 or more");
                    Check(itch.Pause > 0f, $"{at}: itch pause must be above 0");
                    Check(itch.Push >= 0f, $"{at}: itch push must be 0 or more");
                }
            }
            if (u.Bomb is { } b)
            {
                Check(u.Attack == null, $"{at}: a bomb does not shoot (no attack)");
                Check(b.Damage > 0f && b.Radius > 0f, $"{at}: bomb damage and radius must be above 0");
                Check(b.Push >= 0f, $"{at}: bomb push must be 0 or more");
            }
            if (u.Glue is { } g)
            {
                Check(g.TearImpulse > 0f && g.RestSpeed > 0f, $"{at}: glue tearImpulse and restSpeed must be above 0");
                Check(g.RegrabSeconds >= 0f, $"{at}: glue regrabSeconds must be 0 or more");
            }
            if (u.Shield is { } s)
            {
                Check(s.PushShare >= 0f && s.PushShare <= 1f, $"{at}: shield pushShare must be between 0 and 1");
                Check(s.ThrowBack >= 0f, $"{at}: shield throwBack must be 0 or more");
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
            Check(HexColor.IsValid(e.Fill) && HexColor.IsValid(e.Accent), $"{at}: fill or accent is not a colour");
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
