using RettetDasEi.Sim;

namespace RettetDasEi.Sim.Tests;

/// <summary>
/// The tests' own small content — never the shipped files, so tuning never breaks a logic test.
/// A test changes one file's text (<see cref="Load"/>'s arguments) to build exactly its scenario.
/// </summary>
public static class TestContent
{
    public const string Game = """
        {
          "startGold": 100,
          "rangePerHeight": 0.5,
          "eggUnit": "egg",
          "unitLinearDamp": 0.05,
          "unitAngularDamp": 0.8,
          "sellRefund": 0.5,
          "lobGravity": 980
        }
        """;

    public const string Units = """
        [
          {
            "id": "egg", "name": "Das Ei", "description": "Muss überleben.", "inShop": false, "cost": 0,
            "shape": "egg", "width": 90, "height": 62, "mass": 0.8, "friction": 0.9, "hp": 40,
            "fill": "#FFF6E6", "accent": "#D9C29A", "visual": "egg"
          },
          {
            "id": "block", "name": "Block", "description": "Ein Baustein.", "inShop": true, "cost": 20,
            "shape": "box", "width": 100, "height": 50, "mass": 2, "friction": 1, "hp": 100,
            "fill": "#F2C94C", "accent": "#C99A1E", "visual": "cheese"
          },
          {
            "id": "shooter", "name": "Schütze", "description": "Schießt.", "inShop": true, "cost": 30,
            "shape": "capsule", "width": 120, "height": 44, "mass": 1, "friction": 0.9, "hp": 50,
            "fill": "#6BAA3A", "accent": "#3F7A1E", "visual": "pickle",
            "attack": { "projectile": "straight", "visual": "slice", "damage": 10, "cooldown": 1,
                        "range": 400, "speed": 900, "splash": 0, "recoil": 20 }
          }
        ]
        """;

    public const string Enemies = """
        [
          {
            "id": "ant", "name": "Ameise", "movement": "waddle", "radius": 20, "hp": 20, "speed": 70,
            "damage": 5, "push": 100, "attackCooldown": 1, "gold": 4,
            "fill": "#4A2E22", "accent": "#2A1810", "visual": "ant"
          },
          {
            "id": "fly", "name": "Fliege", "movement": "fly", "radius": 16, "hp": 10, "speed": 120,
            "damage": 4, "push": 80, "attackCooldown": 1, "gold": 6,
            "fill": "#B88A5A", "accent": "#D63A2F", "visual": "fly"
          }
        ]
        """;

    /// <summary>Two waves: three ants from both sides; then a fly from the left and an ant from the right.</summary>
    public const string Waves = """
        [
          { "reward": 30, "groups": [ { "enemy": "ant", "count": 3, "start": 1, "interval": 2, "side": "both" } ] },
          { "reward": 0, "groups": [
              { "enemy": "fly", "count": 1, "start": 0.5, "interval": 1, "side": "left" },
              { "enemy": "ant", "count": 1, "start": 0, "interval": 1, "side": "right" } ] }
        ]
        """;

    public static ContentDb Load(string? game = null, string? units = null, string? enemies = null, string? waves = null)
    {
        var files = new Dictionary<string, string>
        {
            [ContentDb.GameFile] = game ?? Game,
            [ContentDb.UnitsFile] = units ?? Units,
            [ContentDb.EnemiesFile] = enemies ?? Enemies,
            [ContentDb.WavesFile] = waves ?? Waves,
        };
        return ContentDb.Load(name => files[name]);
    }
}
