using System.Collections.Generic;
using Godot;
using RettetDasEi.Sim;

namespace RettetDasEi;

/// <summary>
/// The cheese's glue (ADR 003): what comes to rest on a gluing piece's top is pinned to it by two
/// joints, so the two move as one. A hard blow on either tears it (the core's <see cref="Glue.Tears"/>),
/// with cheese strings; after a short wait the two can stick again once they rest. Picking a piece
/// up loosens all of its glue. Contacts and joints are Godot's physics; whether it tears is the core's.
/// It is seen (his wish, 2026-10-24): a seam of melted cheese while two are stuck, and a glowing
/// strip on the cheese while a piece in the hand would stick there.
/// </summary>
public partial class GlueBonds : Node2D
{
    // Darker than the cheese's own outline, so a seam reads even between two cheeses.
    static readonly Color SeamDark = new(0.62f, 0.33f, 0.04f);
    static readonly Color SeamLight = new(1f, 0.88f, 0.42f);

    sealed class Bond
    {
        public UnitBody Cheese = null!;
        public UnitBody Piece = null!;
        public PinJoint2D A = null!;
        public PinJoint2D B = null!;
        public float Age;
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
            // The pins alone hold the pair: their contact fighting the pins made a resting glued
            // stack jump (836 px/s measured, 0 without the contact — his recording, 2026-10-24).
            DisableCollision = true,
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

    // ------------------------------------------------------------- the look

    public override void _Process(double delta)
    {
        foreach (var bond in _bonds)
            bond.Age += (float)delta;
        QueueRedraw();
    }

    public override void _Draw()
    {
        // A seam of melted cheese where two are stuck: it grows in, and drips a little.
        float time = Time.GetTicksMsec() / 1000f;
        foreach (var bond in _bonds)
        {
            if (!IsInstanceValid(bond.Cheese) || !IsInstanceValid(bond.Piece))
                continue;
            var (at, along) = Seam(bond.Cheese, bond.Piece);
            float length = Mathf.Min(bond.Piece.Def.Width, bond.Cheese.Def.Width) * 0.6f;
            float grow = Mathf.Clamp(bond.Age / 0.25f, 0f, 1f);
            var half = along * length * 0.5f * grow;
            DrawLine(at - half, at + half, SeamDark, 11f, true);
            DrawLine(at - half, at + half, SeamLight, 6f, true);
            DrawLine(at - half * 0.7f + Vector2.Up * 1.5f, at + half * 0.7f + Vector2.Up * 1.5f, new Color(1f, 1f, 0.9f, 0.9f), 2f, true);
            for (int k = -1; k <= 1; k++)
            {
                var drip = at + along * length * 0.3f * k * grow + Vector2.Down * (6f + 2.5f * Mathf.Sin(time * 2f + k * 1.7f));
                DrawCircle(drip, 4.6f * grow, SeamDark);
                DrawCircle(drip, 3.2f * grow, SeamLight);
            }
        }

        // While a piece is in the hand: where it would stick, the cheese's face glows.
        var held = Main.I.HeldUnit;
        if (held == null)
            return;
        var target = StickTarget(held);
        if (target == null)
            return;
        var (spot, direction) = Seam(target, held);
        float width = Mathf.Min(held.Def.Width, target.Def.Width) * 0.8f;
        float pulse = 0.7f + 0.3f * Mathf.Sin(time * 8f);
        var span = direction * width * 0.5f;
        DrawLine(spot - span, spot + span, new Color(1f, 0.85f, 0.2f, 0.45f * pulse), 26f, true);
        DrawLine(spot - span, spot + span, SeamDark, 11f, true);
        DrawLine(spot - span, spot + span, new Color(1f, 0.96f, 0.6f, pulse), 7f, true);
        // The words beside the strip's right end, off the cheese's face.
        var end = span.X >= 0f ? spot + span : spot - span;
        var label = end + new Vector2(14f, 7f);
        var font = ThemeDB.FallbackFont;
        DrawStringOutline(font, label, "klebt hier", HorizontalAlignment.Left, -1f, 22, 7, new Color(0.35f, 0.2f, 0.05f));
        DrawString(font, label, "klebt hier", HorizontalAlignment.Left, -1f, 22, new Color(1f, 0.9f, 0.45f));
    }

    /// <summary>
    /// The gluing piece a held piece would stick to if let go here: one it would rest on (the same
    /// rule as <see cref="RestsOnTop"/>) with its bottom near that piece's top.
    /// </summary>
    static UnitBody? StickTarget(UnitBody held)
    {
        foreach (var unit in Main.I.Units)
        {
            if (unit == held || unit.Def.Glue == null || unit.IsHeld || unit.IsBroken || !RestsOnTop(unit, held))
                continue;
            float gap = (unit.GlobalPosition.Y - unit.Def.Height / 2f) - (held.GlobalPosition.Y + held.BoundRadius);
            if (gap > -30f && gap < 60f)
                return unit;
        }
        return null;
    }

    /// <summary>The point on the cheese's face nearest the piece, and the direction that face runs in.</summary>
    static (Vector2 At, Vector2 Along) Seam(UnitBody cheese, UnitBody piece)
    {
        var local = cheese.ToLocal(piece.GlobalPosition);
        float halfWidth = cheese.Def.Width / 2f;
        float halfHeight = cheese.Def.Height / 2f;
        Vector2 at;
        Vector2 along;
        if (Mathf.Abs(local.Y) / halfHeight >= Mathf.Abs(local.X) / halfWidth)
        {
            at = new Vector2(Mathf.Clamp(local.X, -halfWidth, halfWidth), Mathf.Sign(local.Y) * halfHeight);
            along = Vector2.Right;
        }
        else
        {
            at = new Vector2(Mathf.Sign(local.X) * halfWidth, Mathf.Clamp(local.Y, -halfHeight, halfHeight));
            along = Vector2.Down;
        }
        return (cheese.ToGlobal(at), cheese.GlobalTransform.BasisXform(along).Normalized());
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
