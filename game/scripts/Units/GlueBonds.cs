using System.Collections.Generic;
using Godot;
using RettetDasEi.Sim;

namespace RettetDasEi;

/// <summary>
/// The cheese's glue (ADR 003): what comes to rest on a gluing piece's top is pinned to it by two
/// joints, so the two move as one. A hard blow on either tears it (the core's <see cref="Glue.Tears"/>),
/// with cheese strings; after a short wait the two can stick again once they rest. Picking a piece
/// up loosens all of its glue. Contacts and joints are Godot's physics; whether it tears is the core's.
/// </summary>
public partial class GlueBonds : Node
{
    sealed class Bond
    {
        public UnitBody Cheese = null!;
        public UnitBody Piece = null!;
        public PinJoint2D A = null!;
        public PinJoint2D B = null!;
    }

    sealed class Cooldown
    {
        public UnitBody Cheese = null!;
        public UnitBody Piece = null!;
        public float Left;
    }

    readonly List<Bond> _bonds = new();
    readonly List<Cooldown> _cooldowns = new();

    public override void _PhysicsProcess(double delta)
    {
        for (int i = _cooldowns.Count - 1; i >= 0; i--)
        {
            _cooldowns[i].Left -= (float)delta;
            if (_cooldowns[i].Left <= 0f)
                _cooldowns.RemoveAt(i);
        }

        foreach (var cheese in Main.I.Units)
        {
            var glue = cheese.Def.Glue;
            if (glue == null || cheese.IsHeld || cheese.IsBroken || cheese.LinearVelocity.Length() > glue.RestSpeed)
                continue;
            foreach (var body in cheese.GetCollidingBodies())
            {
                if (body is not UnitBody piece || piece == cheese || piece.IsHeld || piece.IsBroken)
                    continue;
                if (piece.LinearVelocity.Length() > glue.RestSpeed || !RestsOnTop(cheese, piece))
                    continue;
                if (IsBonded(cheese, piece) || IsCooling(cheese, piece))
                    continue;
                Stick(cheese, piece);
            }
        }
    }

    /// <summary>On its top: above the cheese's centre and over its width — the sides do not stick.</summary>
    static bool RestsOnTop(UnitBody cheese, UnitBody piece) =>
        piece.GlobalPosition.Y < cheese.GlobalPosition.Y
        && Mathf.Abs(piece.GlobalPosition.X - cheese.GlobalPosition.X) <= cheese.Def.Width / 2f;

    void Stick(UnitBody cheese, UnitBody piece)
    {
        // Two pins side by side, so the piece cannot turn about one point.
        float half = Mathf.Min(piece.Def.Width, cheese.Def.Width) * 0.3f;
        var at = new Vector2(piece.GlobalPosition.X, (piece.GlobalPosition.Y + cheese.GlobalPosition.Y) / 2f);
        var bond = new Bond
        {
            Cheese = cheese,
            Piece = piece,
            A = Pin(cheese, piece, at + new Vector2(-half, 0f)),
            B = Pin(cheese, piece, at + new Vector2(half, 0f)),
        };
        _bonds.Add(bond);
        Fx.Crumbs(Main.I.FxRoot, at, new Color(0.98f, 0.82f, 0.25f), 5, 90f);
    }

    PinJoint2D Pin(UnitBody cheese, UnitBody piece, Vector2 at)
    {
        var joint = new PinJoint2D
        {
            Position = at,
            Softness = 0f,
            DisableCollision = false,
            NodeA = cheese.GetPath(),
            NodeB = piece.GetPath(),
        };
        AddChild(joint);
        return joint;
    }

    /// <summary>A blow on <paramref name="unit"/>: every bond it is part of tears if the blow is hard enough.</summary>
    public void Blow(UnitBody unit, float impulse)
    {
        for (int i = _bonds.Count - 1; i >= 0; i--)
        {
            var bond = _bonds[i];
            if (bond.Cheese != unit && bond.Piece != unit)
                continue;
            if (!Glue.Tears(bond.Cheese.Def.Glue!, impulse))
                continue;
            Remove(i, strings: true);
            _cooldowns.Add(new Cooldown { Cheese = bond.Cheese, Piece = bond.Piece, Left = bond.Cheese.Def.Glue!.RegrabSeconds });
        }
    }

    /// <summary>Every bond of <paramref name="unit"/> goes — it is picked up or breaks.</summary>
    public void Loosen(UnitBody unit, bool strings)
    {
        for (int i = _bonds.Count - 1; i >= 0; i--)
            if (_bonds[i].Cheese == unit || _bonds[i].Piece == unit)
                Remove(i, strings);
    }

    void Remove(int index, bool strings)
    {
        var bond = _bonds[index];
        _bonds.RemoveAt(index);
        bond.A.QueueFree();
        bond.B.QueueFree();
        bond.Cheese.Sleeping = false;
        bond.Piece.Sleeping = false;
        if (strings)
            CheeseStrings.Spawn(Main.I.FxRoot, bond.Cheese, bond.Piece);
    }

    bool IsBonded(UnitBody cheese, UnitBody piece)
    {
        foreach (var bond in _bonds)
            if (bond.Cheese == cheese && bond.Piece == piece)
                return true;
        return false;
    }

    bool IsCooling(UnitBody cheese, UnitBody piece)
    {
        foreach (var cooldown in _cooldowns)
            if (cooldown.Cheese == cheese && cooldown.Piece == piece)
                return true;
        return false;
    }
}
