using System.Collections.Generic;
using Godot;
using RettetDasEi.Sim;

namespace RettetDasEi;

/// <summary>
/// The cheese's glue (ADR 003): what comes to rest on a gluing piece's top is pinned to it by two
/// joints, so the two move as one. A hard blow on either tears it (the core's <see cref="Glue.Tears"/>),
/// with cheese strings; after a short wait the two can stick again once they rest. Picking a piece
/// up loosens all of its glue. Contacts and joints are Godot's physics; whether it tears is the core's.
/// Only the cut face sticks — the cheese's own top, bare of wax — and only while it faces up
/// (his word, 2026-10-24: no gluing at the sides). Sticking shows yellow crumbs; while a piece in
/// the hand would stick, the cut face glows with "klebt hier". (A seam drawn while stuck was tried
/// the same day and taken out at his word.)
/// </summary>
public partial class GlueBonds : Node2D
{
    /// <summary>The cut face counts as facing up while it is tilted less than this from straight up.</summary>
    static readonly float MaxTilt = Mathf.DegToRad(45f);
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
                if (piece.LinearVelocity.Length() > glue.RestSpeed || !RestsOnTop(cheese, piece))
                    continue;
                if (IsBonded(cheese, piece) || IsCooling(cheese, piece))
                    continue;
                Stick(cheese, piece);
            }
        }
    }

    /// <summary>
    /// On its cut face: the cheese's own top faces up (within <see cref="MaxTilt"/>), and the piece
    /// lies over that face — not beside the cheese, not under it. The waxed sides never stick.
    /// </summary>
    static bool RestsOnTop(UnitBody cheese, UnitBody piece)
    {
        var cutFaceNormal = -cheese.GlobalTransform.Y.Normalized();
        if (cutFaceNormal.Dot(Vector2.Up) < Mathf.Cos(MaxTilt))
            return false;
        var local = cheese.ToLocal(piece.GlobalPosition);
        return local.Y < 0f && Mathf.Abs(local.X) <= cheese.Def.Width / 2f;
    }

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

    public override void _Process(double delta) => QueueRedraw();

    public override void _Draw()
    {
        // While a piece is in the hand: where it would stick, the cut face glows.
        var held = Main.I.HeldUnit;
        if (held == null)
            return;
        var target = StickTarget(held);
        if (target == null)
            return;
        float time = Time.GetTicksMsec() / 1000f;
        var (spot, direction) = CutFacePoint(target, held);
        float width = Mathf.Min(held.Def.Width, target.Def.Width) * 0.8f;
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

    /// <summary>The point of the cheese's cut face under the piece, and the direction the face runs in.</summary>
    static (Vector2 At, Vector2 Along) CutFacePoint(UnitBody cheese, UnitBody piece)
    {
        var local = cheese.ToLocal(piece.GlobalPosition);
        float halfWidth = cheese.Def.Width / 2f;
        var at = new Vector2(Mathf.Clamp(local.X, -halfWidth, halfWidth), -cheese.Def.Height / 2f);
        return (cheese.ToGlobal(at), cheese.GlobalTransform.X.Normalized());
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
