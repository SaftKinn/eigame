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
    /// <summary>How far a piece snapped to a cheese's side sits from it.</summary>
    const float SnapGap = 1f;
    /// <summary>How close, edge to edge, a held piece must come for "klebt hier" and the snap.</summary>
    const float Reach = 12f;
    /// <summary>How much of a face a piece must lie against to stick there (not corner to corner).</summary>
    const float MinContact = 8f;

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
    /// Where a piece meets the cheese, edge to edge (his screenshot, 2026-10-26: an upright cheese
    /// beside a flat one counted as "on top" when judged by the centres). In the cheese's own frame
    /// the face is the one across the wider gap between the two boxes — a side (<c>Side</c>) or the
    /// top or bottom. <c>Gap</c>: between the piece's near edge and that face, negative when they
    /// overlap. <c>At</c> and <c>Length</c>: the middle and length of the stretch of the face the
    /// piece lies against; <c>Along</c>: the direction the face runs in; <c>Out</c>: out of it.
    /// </summary>
    static (Vector2 At, Vector2 Along, float Length, bool Side, float Gap, Vector2 Out) Face(UnitBody cheese, UnitBody piece)
    {
        var local = cheese.ToLocal(piece.GlobalPosition);
        var axisX = cheese.GlobalTransform.X.Normalized();
        var axisY = cheese.GlobalTransform.Y.Normalized();
        float halfWidth = cheese.Def.Width / 2f, halfHeight = cheese.Def.Height / 2f;
        float extentX = HalfExtent(piece, axisX), extentY = HalfExtent(piece, axisY);
        float gapX = Mathf.Abs(local.X) - halfWidth - extentX;
        float gapY = Mathf.Abs(local.Y) - halfHeight - extentY;
        if (gapX > gapY)
        {
            float low = Mathf.Max(-halfHeight, local.Y - extentY), high = Mathf.Min(halfHeight, local.Y + extentY);
            var at = new Vector2(Mathf.Sign(local.X) * halfWidth, (low + high) / 2f);
            return (cheese.ToGlobal(at), axisY, Mathf.Max(high - low, 0f), true, gapX, axisX * Mathf.Sign(local.X));
        }
        float left = Mathf.Max(-halfWidth, local.X - extentX), right = Mathf.Min(halfWidth, local.X + extentX);
        var onFace = new Vector2((left + right) / 2f, Mathf.Sign(local.Y) * halfHeight);
        return (cheese.ToGlobal(onFace), axisX, Mathf.Max(right - left, 0f), false, gapY, axisY * Mathf.Sign(local.Y));
    }

    /// <summary>Half the extent of a body's box along a direction, however the body is turned.</summary>
    static float HalfExtent(UnitBody body, Vector2 direction) =>
        Mathf.Abs(direction.Dot(body.GlobalTransform.X.Normalized())) * body.Def.Width / 2f
        + Mathf.Abs(direction.Dot(body.GlobalTransform.Y.Normalized())) * body.Def.Height / 2f;

    /// <summary>A piece at one of the cheese's sides: <see cref="Face"/>'s gap and outward direction. Null above or below it.</summary>
    static (float Gap, Vector2 Out)? Beside(UnitBody cheese, UnitBody piece)
    {
        var face = Face(cheese, piece);
        return face.Side ? (face.Gap, face.Out) : null;
    }

    /// <summary>
    /// The side glue's magnet (his word, 2026-10-26: beside a cheese a piece just fell down): let
    /// go beside a cheese's side, the held one — the piece or the cheese — moves flush against it.
    /// The pair and the move, or null when the held one would not stick at a side.
    /// </summary>
    public (UnitBody Cheese, UnitBody Piece, Vector2 Move)? SideSnap(UnitBody held)
    {
        if (StickPair(held) is not { } pair || Beside(pair.Cheese, pair.Piece) is not { } beside)
            return null;
        var move = beside.Out * (SnapGap - beside.Gap);
        return (pair.Cheese, pair.Piece, held == pair.Piece ? move : -move);
    }

    /// <summary>After a snap: the pair sticks at once, without waiting to rest, if still flush.</summary>
    public void StickBeside(UnitBody cheese, UnitBody piece)
    {
        if (Beside(cheese, piece) is { } beside && Mathf.Abs(beside.Gap - SnapGap) < 4f && !IsBonded(cheese, piece))
            Stick(cheese, piece);
    }

    void Stick(UnitBody cheese, UnitBody piece)
    {
        // Two pins along the face they touch at, so the piece cannot turn about one point.
        var (at, along, length, _, _, _) = Face(cheese, piece);
        float half = Mathf.Max(length * 0.3f, 2f);
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
        if (StickPair(held) is not { } pair)
            return;
        float time = Time.GetTicksMsec() / 1000f;
        var (spot, direction, length, _, _, _) = Face(pair.Cheese, pair.Piece);
        float width = length * 0.8f;
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
    /// What would stick if the held one were let go here: the nearest pair of a cheese and a piece,
    /// either of them the held one (a held piece near a cheese, or a held cheese near anything),
    /// edge to edge within <see cref="Reach"/> (his word, 2026-10-26: precise, brought really close
    /// together) and lying against at least <see cref="MinContact"/> of a face.
    /// </summary>
    static (UnitBody Cheese, UnitBody Piece)? StickPair(UnitBody held)
    {
        (UnitBody Cheese, UnitBody Piece)? nearest = null;
        float nearestGap = float.MaxValue;
        foreach (var unit in Main.I.Units)
        {
            if (unit == held || unit.IsHeld || unit.IsBroken)
                continue;
            var (cheese, piece) = unit.Def.Glue != null ? (unit, held) : (held, unit);
            if (cheese.Def.Glue == null)
                continue;
            var face = Face(cheese, piece);
            float gap = face.Gap;
            if (gap > -Reach && gap < Reach && face.Length >= MinContact && Mathf.Abs(gap) < nearestGap)
            {
                nearest = (cheese, piece);
                nearestGap = Mathf.Abs(gap);
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
