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
    /// <summary>Absent for pure building blocks — and for a bomb, which does not shoot (ADR 003).</summary>
    public AttackDef? Attack { get; set; }
    // The jobs of ADR 003: each is optional, a unit has the ones its entry names.
    /// <summary>Bursts at a pest's touch (Tomate).</summary>
    public BombDef? Bomb { get; set; }
    /// <summary>What comes to rest on its top sticks to it (Käse).</summary>
    public GlueDef? Glue { get; set; }
    /// <summary>A pest striking it is thrown back, and it takes a share of the push (Toast).</summary>
    public ShieldDef? Shield { get; set; }
}

/// <summary>Bursts once at any touch with a pest: damages pests, pushes own food — damages none.</summary>
public sealed class BombDef
{
    [JsonRequired] public float Damage { get; set; }
    /// <summary>Pixels from the bomb's centre to a pest's edge or a piece's bounding circle.</summary>
    [JsonRequired] public float Radius { get; set; }
    /// <summary>Impulse on own food at the centre, falling to 0 at the edge of the radius.</summary>
    [JsonRequired] public float Push { get; set; }
}

public sealed class GlueDef
{
    /// <summary>A blow (a pest's push or a bomb's) at least this strong tears the glue of the piece it hits.</summary>
    [JsonRequired] public float TearImpulse { get; set; }
    /// <summary>Both pieces slower than this (pixels per second) count as come to rest.</summary>
    [JsonRequired] public float RestSpeed { get; set; }
    /// <summary>Seconds after a tear before the same two can stick again.</summary>
    [JsonRequired] public float RegrabSeconds { get; set; }
}

public sealed class ShieldDef
{
    /// <summary>0..1, the share of a pest's push the shield takes.</summary>
    [JsonRequired] public float PushShare { get; set; }
    /// <summary>Pixels the striking pest is thrown back.</summary>
    [JsonRequired] public float ThrowBack { get; set; }
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
    /// <summary>How many pests one straight shot hits on its way (1: the first only).</summary>
    [JsonRequired] public int Pierce { get; set; }
    /// <summary>A lob's blob leaves this on the ground (Senf).</summary>
    public PuddleDef? Puddle { get; set; }
    /// <summary>Every hit itches; enough of them make the pest sneeze (Pfeffer).</summary>
    public ItchDef? Itch { get; set; }
}

/// <summary>Walkers in it move slower; flyers pass over it. Puddles do not stack — the strongest counts.</summary>
public sealed class PuddleDef
{
    /// <summary>Half its width on the ground, in pixels.</summary>
    [JsonRequired] public float Radius { get; set; }
    [JsonRequired] public float Seconds { get; set; }
    /// <summary>0..1, the speed a walker keeps in it (0.4 = 40 %).</summary>
    [JsonRequired] public float Slow { get; set; }
}

public sealed class ItchDef
{
    /// <summary>Hits until a sneeze; then the itch starts again.</summary>
    [JsonRequired] public int Hits { get; set; }
    /// <summary>Seconds the sneezing pest stands still and does not attack.</summary>
    [JsonRequired] public float Pause { get; set; }
    /// <summary>Pixels the sneeze throws it back.</summary>
    [JsonRequired] public float Push { get; set; }
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
