using System.Text.Json.Serialization;

namespace RettetDasEi.Sim;

// The shapes of game/content/*.json. Every field is required: a missing one fails at load
// (balancing is data, never a silent default — CLAUDE.md).

/// <summary>Global tuning, game/content/game.json.</summary>
public sealed class GameDef
{
    [JsonRequired] public int StartGold { get; set; }
    /// <summary>Extra range a shooter gets per pixel it stands above the board.</summary>
    [JsonRequired] public float RangePerHeight { get; set; }
    [JsonRequired] public string EggUnit { get; set; } = "";
    [JsonRequired] public float UnitLinearDamp { get; set; }
    [JsonRequired] public float UnitAngularDamp { get; set; }
    /// <summary>Share of the price a placed unit returns when sold (scaled by its health).</summary>
    [JsonRequired] public float SellRefund { get; set; }
    [JsonRequired] public float LobGravity { get; set; }
}

/// <summary>A piece of food for the tower, game/content/units.json.</summary>
public sealed class UnitDef
{
    [JsonRequired] public string Id { get; set; } = "";
    [JsonRequired] public string Name { get; set; } = "";
    [JsonRequired] public string Description { get; set; } = "";
    [JsonRequired] public bool InShop { get; set; }
    [JsonRequired] public int Cost { get; set; }
    /// <summary>"box", "circle", "capsule" or "egg".</summary>
    [JsonRequired] public string Shape { get; set; } = "";
    [JsonRequired] public float Width { get; set; }
    [JsonRequired] public float Height { get; set; }
    [JsonRequired] public float Mass { get; set; }
    [JsonRequired] public float Friction { get; set; }
    [JsonRequired] public float Hp { get; set; }
    [JsonRequired] public string Fill { get; set; } = "";
    [JsonRequired] public string Accent { get; set; } = "";
    /// <summary>Art key: art/units/&lt;visual&gt;.png if it exists, else the drawing in DrawKit.</summary>
    [JsonRequired] public string Visual { get; set; } = "";
    /// <summary>Absent for pure building blocks.</summary>
    public AttackDef? Attack { get; set; }
}

public sealed class AttackDef
{
    /// <summary>"straight" (flies in a line) or "lob" (flies in an arc and splashes).</summary>
    [JsonRequired] public string Projectile { get; set; } = "";
    [JsonRequired] public string Visual { get; set; } = "";
    [JsonRequired] public float Damage { get; set; }
    [JsonRequired] public float Cooldown { get; set; }
    [JsonRequired] public float Range { get; set; }
    /// <summary>Pixels per second; unused by "lob", which picks its own arc.</summary>
    [JsonRequired] public float Speed { get; set; }
    /// <summary>Radius of the splash; 0 hits one enemy only.</summary>
    [JsonRequired] public float Splash { get; set; }
    /// <summary>Impulse that pushes the shooter back on every shot.</summary>
    [JsonRequired] public float Recoil { get; set; }
}

/// <summary>A kitchen pest, game/content/enemies.json.</summary>
public sealed class EnemyDef
{
    [JsonRequired] public string Id { get; set; } = "";
    [JsonRequired] public string Name { get; set; } = "";
    /// <summary>"waddle", "roll", "hop" (all walk on the counter) or "fly".</summary>
    [JsonRequired] public string Movement { get; set; } = "";
    [JsonRequired] public float Radius { get; set; }
    [JsonRequired] public float Hp { get; set; }
    [JsonRequired] public float Speed { get; set; }
    [JsonRequired] public float Damage { get; set; }
    /// <summary>Impulse a hit gives the piece of food it strikes.</summary>
    [JsonRequired] public float Push { get; set; }
    [JsonRequired] public float AttackCooldown { get; set; }
    [JsonRequired] public int Gold { get; set; }
    [JsonRequired] public string Fill { get; set; } = "";
    [JsonRequired] public string Accent { get; set; } = "";
    /// <summary>Art key: art/enemies/&lt;visual&gt;.png if it exists, else the drawing in DrawKit.</summary>
    [JsonRequired] public string Visual { get; set; } = "";
}

/// <summary>One wave, game/content/waves.json.</summary>
public sealed class WaveDef
{
    [JsonRequired] public int Reward { get; set; }
    [JsonRequired] public List<SpawnGroupDef> Groups { get; set; } = new();
}

public sealed class SpawnGroupDef
{
    [JsonRequired] public string Enemy { get; set; } = "";
    [JsonRequired] public int Count { get; set; }
    /// <summary>Seconds after the wave starts until the first one comes.</summary>
    [JsonRequired] public float Start { get; set; }
    [JsonRequired] public float Interval { get; set; }
    /// <summary>"left", "right" or "both" (alternating).</summary>
    [JsonRequired] public string Side { get; set; } = "";
}
