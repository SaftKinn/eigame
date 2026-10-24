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
                        "range": 400, "speed": 900, "splash": 0, "recoil": 20, "pierce": 3 }
          },
          {
            "id": "lobber", "name": "Werfer", "description": "Pfütze.", "inShop": true, "cost": 40,
            "shape": "box", "width": 48, "height": 124, "mass": 1, "friction": 0.9, "hp": 45,
            "fill": "#F4C430", "accent": "#C0392B", "visual": "mustard",
            "attack": { "projectile": "lob", "visual": "mustard", "damage": 8, "cooldown": 2,
                        "range": 500, "speed": 0, "splash": 75, "recoil": 15, "pierce": 1,
                        "puddle": { "radius": 70, "seconds": 4, "slow": 0.4 } }
          },
          {
            "id": "pepper", "name": "Pfeffer", "description": "Niesen.", "inShop": true, "cost": 30,
            "shape": "box", "width": 42, "height": 70, "mass": 0.6, "friction": 0.9, "hp": 30,
            "fill": "#ECE7DC", "accent": "#4A4A4A", "visual": "pepper",
            "attack": { "projectile": "straight", "visual": "pepper", "damage": 4, "cooldown": 0.35,
                        "range": 360, "speed": 1100, "splash": 0, "recoil": 6, "pierce": 1,
                        "itch": { "hits": 3, "pause": 1.5, "push": 30 } }
          },
          {
            "id": "bomb", "name": "Bombe", "description": "Platzt.", "inShop": true, "cost": 15,
            "shape": "circle", "width": 64, "height": 64, "mass": 0.9, "friction": 0.8, "hp": 35,
            "fill": "#E74C3C", "accent": "#2E8B3A", "visual": "tomato",
            "bomb": { "damage": 50, "radius": 100, "push": 400 }
          },
          {
            "id": "sticky", "name": "Kleber", "description": "Klebt.", "inShop": true, "cost": 20,
            "shape": "box", "width": 150, "height": 56, "mass": 4, "friction": 1, "hp": 140,
            "fill": "#F2C94C", "accent": "#C99A1E", "visual": "cheese",
            "glue": { "tearImpulse": 400, "restSpeed": 20, "regrabSeconds": 1 }
          },
          {
            "id": "wall", "name": "Wand", "description": "Federt.", "inShop": true, "cost": 15,
            "shape": "box", "width": 92, "height": 84, "mass": 1.5, "friction": 1, "hp": 70,
            "fill": "#F3D39B", "accent": "#B9773A", "visual": "toast",
            "shield": { "pushShare": 0.25, "throwBack": 90 }
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
