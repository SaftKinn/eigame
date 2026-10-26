using System.Collections.Generic;
using Godot;
using RettetDasEi.Sim;

namespace RettetDasEi;

/// <summary>
/// The cheese's glue (ADR 003): what comes to rest touching a gluing piece is pinned to it by two
/// joints, so the two move as one. A hard blow on either tears it (the core's <see cref="Glue.Tears"/>),
/// with cheese strings; after a short wait the two can stick again once they rest. Picking a piece
/// up loosens all of its glue. Contacts and joints are Godot's physics; whether it tears is the core's.
/// Every face sticks — top, bottom and sides, however the cheese lies (his word, 2026-10-26: the
/// wax rind of 2026-10-24 is gone). Sticking shows yellow crumbs; while a piece in the hand would
/// stick, the face it would stick to glows with "klebt hier". (A seam drawn while stuck was tried
/// on 2026-10-24 and taken out at his word.)
/// </summary>
public partial class GlueBonds : Node2D
{
    static readonly Color Rim = new(0.62f, 0.33f, 0.04f);

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
                if (piece.LinearVelocity.Length() > glue.RestSpeed)
                    continue;
                if (IsBonded(cheese, piece) || IsCooling(cheese, piece))
                    continue;
                Stick(cheese, piece);
            }
        }
    }

    /// <summary>
    /// The face of the cheese the piece lies at (in the cheese's own frame: top or bottom when the
    /// piece is more above or below than beside it, else left or right), the point on that face
    /// opposite the piece, the direction the face runs in, and the face's length.
    /// </summary>
    static (Vector2 At, Vector2 Along, float Length) Face(UnitBody cheese, UnitBody piece)
    {
        var local = cheese.ToLocal(piece.GlobalPosition);
        float halfWidth = cheese.Def.Width / 2f, halfHeight = cheese.Def.Height / 2f;
        if (Mathf.Abs(local.Y) / halfHeight >= Mathf.Abs(local.X) / halfWidth)
        {
            var at = new Vector2(Mathf.Clamp(local.X, -halfWidth, halfWidth), Mathf.Sign(local.Y) * halfHeight);
            return (cheese.ToGlobal(at), cheese.GlobalTransform.X.Normalized(), cheese.Def.Width);
        }
        var side = new Vector2(Mathf.Sign(local.X) * halfWidth, Mathf.Clamp(local.Y, -halfHeight, halfHeight));
        return (cheese.ToGlobal(side), cheese.GlobalTransform.Y.Normalized(), cheese.Def.Height);
    }

    void Stick(UnitBody cheese, UnitBody piece)
    {
        // Two pins along the face they touch at, so the piece cannot turn about one point.
        var (at, along, length) = Face(cheese, piece);
        var x = piece.GlobalTransform.X.Normalized();
        var y = piece.GlobalTransform.Y.Normalized();
        float pieceAlong = Mathf.Abs(along.Dot(x)) * piece.Def.Width + Mathf.Abs(along.Dot(y)) * piece.Def.Height;
        float half = Mathf.Min(pieceAlong, length) * 0.3f;
        var bond = new Bond
        {
            Cheese = cheese,
            Piece = piece,
            A = Pin(cheese, piece, at - along * half),
            B = Pin(cheese, piece, at + along * half),
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

    public override void _Process(double delta) => QueueRedraw();

    public override void _Draw()
    {
        // While a piece is in the hand: where it would stick, the face glows.
        var held = Main.I.HeldUnit;
        if (held == null)
            return;
        var target = StickTarget(held);
        if (target == null)
            return;
        float time = Time.GetTicksMsec() / 1000f;
        var (spot, direction, length) = Face(target, held);
        float width = Mathf.Min(held.Def.Width, length) * 0.8f;
        float pulse = 0.7f + 0.3f * Mathf.Sin(time * 8f);
        var span = direction * width * 0.5f;
        DrawLine(spot - span, spot + span, new Color(1f, 0.85f, 0.2f, 0.45f * pulse), 26f, true);
        DrawLine(spot - span, spot + span, Rim, 11f, true);
        DrawLine(spot - span, spot + span, new Color(1f, 0.96f, 0.6f, pulse), 7f, true);
        // The words beside the strip's right end, off the cheese's face.
        var end = span.X >= 0f ? spot + span : spot - span;
        var label = end + new Vector2(14f, 7f);
        var font = ThemeDB.FallbackFont;
        DrawStringOutline(font, label, "klebt hier", HorizontalAlignment.Left, -1f, 22, 7, new Color(0.35f, 0.2f, 0.05f));
        DrawString(font, label, "klebt hier", HorizontalAlignment.Left, -1f, 22, new Color(1f, 0.9f, 0.45f));
    }

    /// <summary>
    /// The gluing piece a held piece would stick to if let go here: the nearest one whose box comes
    /// within reach of the held piece's edge, on any side.
    /// </summary>
    static UnitBody? StickTarget(UnitBody held)
    {
        UnitBody? nearest = null;
        float nearestGap = float.MaxValue;
        foreach (var unit in Main.I.Units)
        {
            if (unit == held || unit.Def.Glue == null || unit.IsHeld || unit.IsBroken)
                continue;
            var local = unit.ToLocal(held.GlobalPosition);
            float outX = Mathf.Max(Mathf.Abs(local.X) - unit.Def.Width / 2f, 0f);
            float outY = Mathf.Max(Mathf.Abs(local.Y) - unit.Def.Height / 2f, 0f);
            float gap = Mathf.Sqrt(outX * outX + outY * outY) - held.BoundRadius;
            if (gap > -30f && gap < 60f && gap < nearestGap)
            {
                nearest = unit;
                nearestGap = gap;
            }
        }
        return nearest;
    }

    // Both ways round: two cheeses side by side each find the other, and are one bond.
    bool IsBonded(UnitBody a, UnitBody b)
    {
        foreach (var bond in _bonds)
            if ((bond.Cheese == a && bond.Piece == b) || (bond.Cheese == b && bond.Piece == a))
                return true;
        return false;
    }

    bool IsCooling(UnitBody a, UnitBody b)
    {
        foreach (var cooldown in _cooldowns)
            if ((cooldown.Cheese == a && cooldown.Piece == b) || (cooldown.Cheese == b && cooldown.Piece == a))
                return true;
        return false;
    }
}
