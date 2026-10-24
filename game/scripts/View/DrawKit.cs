using System.Collections.Generic;
using Godot;
using RettetDasEi.Sim;

namespace RettetDasEi;

/// <summary>
/// The stand-in drawings for every visual key, until a picture lies in art/units/ or art/enemies/.
/// An unknown key draws magenta and warns once (CLAUDE.md, art behind visual keys).
/// Everything is drawn around (0, 0), facing right when <c>face</c> is +1.
/// </summary>
public static class DrawKit
{
    public static readonly Color Ink = new(0.16f, 0.1f, 0.06f);

    static readonly HashSet<string> Warned = new();

    public static void WarnMissing(string kind, string key)
    {
        if (Warned.Add(kind + ":" + key))
            GD.PushWarning($"No drawing for {kind} visual '{key}' — drawn magenta.");
    }

    // ---------------------------------------------------------------- units

    public static void DrawUnit(CanvasItem ci, UnitDef def, Vector2 at, float scale, float face, float hpRatio)
    {
        ci.DrawSetTransform(at, 0f, new Vector2(scale, scale));
        float w = def.Width, h = def.Height;
        var fill = Color.FromHtml(def.Fill);
        var accent = Color.FromHtml(def.Accent);
        var body = new Rect2(-w / 2f, -h / 2f, w, h);
        float eyeY = -h * 0.08f;

        switch (def.Visual)
        {
            case "egg":
                var egg = EggOutline(w, h);
                ci.DrawColoredPolygon(egg, fill);
                ci.DrawPolyline(Closed(egg), accent, 3f, true);
                if (hpRatio < 0.7f)
                    ci.DrawPolyline(new[] { new Vector2(-w * 0.1f, -h * 0.45f), new Vector2(-w * 0.03f, -h * 0.25f), new Vector2(-w * 0.12f, -h * 0.12f), new Vector2(-w * 0.02f, h * 0.02f) }, Ink, 2.5f, true);
                if (hpRatio < 0.4f)
                    ci.DrawPolyline(new[] { new Vector2(w * 0.28f, h * 0.38f), new Vector2(w * 0.2f, h * 0.2f), new Vector2(w * 0.3f, h * 0.08f), new Vector2(w * 0.22f, -h * 0.06f) }, Ink, 2.5f, true);
                eyeY = -h * 0.05f;
                break;
            case "cheese":
            {
                // In its red wax rind all round but the cut face on top: only that face sticks
                // (ADR 003, his word 2026-10-24) — the rind says so without a word.
                var wax = new Color(0.78f, 0.13f, 0.11f);
                Box(ci, body, wax, wax.Darkened(0.4f), 8, 4);
                float rind = Mathf.Clamp(h * 0.16f, 5f, 9f);
                var cut = new Rect2(-w / 2f + rind, -h / 2f, w - 2f * rind, h - rind);
                Box(ci, cut, fill, accent, 5, 0);
                ci.DrawLine(new Vector2(-w / 2f + rind + 3f, -h / 2f + 2.5f), new Vector2(w / 2f - rind - 3f, -h / 2f + 2.5f), fill.Lightened(0.45f), 3f, true);
                ci.DrawCircle(new Vector2(-w * 0.3f, h * 0.12f), h * 0.12f, accent);
                ci.DrawCircle(new Vector2(w * 0.3f, -h * 0.16f), h * 0.09f, accent);
                ci.DrawCircle(new Vector2(w * 0.15f, h * 0.2f), h * 0.08f, accent);
                ci.DrawCircle(new Vector2(-w * 0.12f, h * 0.24f), h * 0.06f, accent);
                break;
            }
            case "toast":
                Box(ci, body, fill, accent, 18, 8);
                break;
            case "pickle":
                Box(ci, body, fill, accent, (int)(h / 2f), 4);
                var bump = fill.Lightened(0.25f);
                for (int i = 0; i < 6; i++)
                    ci.DrawCircle(new Vector2(-w * 0.36f + i * w * 0.145f, (i % 2 == 0 ? 1f : -1f) * h * 0.24f), 2.5f, bump);
                break;
            case "mustard":
                Box(ci, new Rect2(-w * 0.26f, -h / 2f - 14f, w * 0.52f, 18f), accent, Ink, 4, 2);
                Box(ci, body, fill, fill.Darkened(0.25f), 12, 3);
                ci.DrawRect(new Rect2(-w / 2f + 3f, h * 0.05f, w - 6f, h * 0.22f), accent);
                eyeY = -h * 0.2f;
                break;
            case "pepper":
                Box(ci, body, fill, accent, 10, 3);
                Box(ci, new Rect2(-w / 2f, -h / 2f, w, h * 0.3f), accent.Lightened(0.15f), accent, 8, 2);
                for (int i = -1; i <= 1; i++)
                    ci.DrawCircle(new Vector2(i * w * 0.22f, -h / 2f + h * 0.15f), 2.5f, Ink);
                eyeY = h * 0.05f;
                break;
            case "tomato":
                ci.DrawCircle(Vector2.Zero, w / 2f, fill);
                ci.DrawArc(Vector2.Zero, w / 2f - 1.5f, 0f, Mathf.Tau, 40, fill.Darkened(0.3f), 3f, true);
                ci.DrawCircle(new Vector2(-w * 0.18f, -h * 0.22f), w * 0.08f, fill.Lightened(0.45f));
                for (int i = 0; i < 5; i++)
                {
                    var tip = new Vector2(0f, -h / 2f) + Vector2.Up.Rotated(i * Mathf.Tau / 5f) * 10f;
                    ci.DrawLine(new Vector2(0f, -h / 2f), tip, accent, 4f, true);
                }
                eyeY = h * 0.02f;
                break;
            default:
                WarnMissing("unit", def.Visual);
                ci.DrawRect(body, Colors.Magenta);
                break;
        }

        float eyeR = Mathf.Clamp(Mathf.Min(w, h) * 0.15f, 5f, 11f);
        float spacing = Mathf.Clamp(w * 0.17f, eyeR + 2f, 24f);
        Eyes(ci, new Vector2(0f, eyeY), spacing, eyeR, face, angry: false);
        ci.DrawArc(new Vector2(0f, eyeY + eyeR * 1.4f), eyeR * 0.7f, 0.35f, Mathf.Pi - 0.35f, 10, Ink, 2.5f, true);

        if (def.Visual != "egg" && hpRatio < 0.5f)
            ci.DrawPolyline(new[] { new Vector2(w * 0.3f, -h * 0.45f), new Vector2(w * 0.18f, -h * 0.15f), new Vector2(w * 0.3f, h * 0.05f), new Vector2(w * 0.2f, h * 0.3f) }, Ink, 3f, true);

        ci.DrawSetTransform(Vector2.Zero, 0f, Vector2.One);
    }

    // -------------------------------------------------------------- enemies

    public static void DrawEnemy(CanvasItem ci, EnemyDef def, float face, float spin, float flap)
    {
        float r = def.Radius;
        var fill = Color.FromHtml(def.Fill);
        var accent = Color.FromHtml(def.Accent);

        switch (def.Visual)
        {
            case "ant":
            {
                for (int i = -1; i <= 1; i++)
                {
                    float x = i * r * 0.45f;
                    ci.DrawLine(new Vector2(x, r * 0.1f), new Vector2(x - face * r * 0.3f, r * 0.95f), accent, 3f, true);
                }
                ci.DrawColoredPolygon(Ellipse(new Vector2(-face * r * 0.75f, 0f), r * 0.62f, r * 0.5f), fill);
                ci.DrawColoredPolygon(Ellipse(Vector2.Zero, r * 0.38f, r * 0.32f), fill);
                var head = new Vector2(face * r * 0.75f, -r * 0.18f);
                ci.DrawLine(head, head + new Vector2(face * r * 0.45f, -r * 0.8f), accent, 2.5f, true);
                ci.DrawLine(head, head + new Vector2(face * r * 0.1f, -r * 0.9f), accent, 2.5f, true);
                ci.DrawCircle(head, r * 0.45f, fill);
                Eyes(ci, head + new Vector2(face * r * 0.08f, -r * 0.05f), r * 0.2f, r * 0.17f, face, angry: true);
                break;
            }
            case "meatball":
            {
                ci.DrawCircle(Vector2.Zero, r, fill);
                ci.DrawArc(Vector2.Zero, r - 1.5f, 0f, Mathf.Tau, 40, accent, 3f, true);
                Vector2[] spots = { new(0.42f, -0.3f), new(-0.45f, 0.22f), new(0.12f, 0.52f), new(-0.2f, -0.55f), new(0.55f, 0.3f) };
                foreach (var s in spots)
                    ci.DrawCircle((s * r).Rotated(spin), r * 0.12f, accent);
                ci.DrawCircle(new Vector2(-r * 0.35f, -r * 0.4f), r * 0.15f, fill.Lightened(0.3f));
                Eyes(ci, new Vector2(face * r * 0.15f, -r * 0.12f), r * 0.28f, r * 0.22f, face, angry: true);
                break;
            }
            case "fly":
            {
                var wing = new Color(1f, 1f, 1f, 0.6f);
                float wingY = r * (0.25f + 0.55f * flap);
                ci.DrawColoredPolygon(Ellipse(new Vector2(-face * r * 0.2f, -r * 0.7f), r * 0.85f, wingY), wing);
                ci.DrawColoredPolygon(Ellipse(Vector2.Zero, r, r * 0.72f), fill);
                ci.DrawLine(new Vector2(-r * 0.3f, -r * 0.6f), new Vector2(-r * 0.3f, r * 0.6f), accent.Darkened(0.4f), 2f, true);
                ci.DrawCircle(new Vector2(face * r * 0.58f, -r * 0.15f), r * 0.42f, accent);
                ci.DrawCircle(new Vector2(face * r * 0.5f, -r * 0.3f), r * 0.12f, Colors.White);
                break;
            }
            case "pudding":
            {
                var shape = new Vector2[32];
                for (int i = 0; i < shape.Length; i++)
                {
                    float a = i * Mathf.Tau / shape.Length;
                    float wobble = 1f + Mathf.Sin(a * 3f + spin * 2f) * 0.04f;
                    shape[i] = new Vector2(Mathf.Cos(a) * r * wobble, Mathf.Min(Mathf.Sin(a) * r * 0.9f * wobble, r * 0.7f));
                }
                ci.DrawColoredPolygon(shape, new Color(fill, 0.9f));
                ci.DrawPolyline(Closed(shape), accent, 3f, true);
                ci.DrawColoredPolygon(Ellipse(new Vector2(-r * 0.38f, -r * 0.42f), r * 0.22f, r * 0.12f), new Color(1f, 1f, 1f, 0.45f));
                Eyes(ci, new Vector2(face * r * 0.12f, -r * 0.1f), r * 0.26f, r * 0.18f, face, angry: true);
                break;
            }
            default:
                WarnMissing("enemy", def.Visual);
                ci.DrawCircle(Vector2.Zero, r, Colors.Magenta);
                break;
        }
    }

    // ---------------------------------------------------------- projectiles

    public static Color ProjectileColor(string visual) => visual switch
    {
        "slice" => new Color(0.55f, 0.77f, 0.29f),
        "pepper" => new Color(0.2f, 0.2f, 0.2f),
        "seed" => new Color(0.97f, 0.9f, 0.63f),
        "mustard" => new Color(0.96f, 0.77f, 0.19f),
        _ => Colors.Magenta,
    };

    public static void DrawProjectile(CanvasItem ci, string visual, float spin)
    {
        switch (visual)
        {
            case "slice":
                ci.DrawCircle(Vector2.Zero, 11f, new Color(0.4f, 0.62f, 0.2f));
                ci.DrawCircle(Vector2.Zero, 8f, new Color(0.78f, 0.9f, 0.6f));
                for (int i = 0; i < 5; i++)
                    ci.DrawCircle(new Vector2(4.5f, 0f).Rotated(spin + i * Mathf.Tau / 5f), 1.6f, new Color(0.95f, 0.95f, 0.8f));
                break;
            case "pepper":
                for (int i = 0; i < 3; i++)
                    ci.DrawCircle(new Vector2(4f, 0f).Rotated(spin + i * Mathf.Tau / 3f), 3f, ProjectileColor(visual));
                break;
            case "seed":
                ci.DrawColoredPolygon(Ellipse(Vector2.Zero, 7f, 4f), ProjectileColor(visual));
                ci.DrawPolyline(Closed(Ellipse(Vector2.Zero, 7f, 4f)), new Color(0.7f, 0.6f, 0.3f), 1.5f, true);
                break;
            case "mustard":
                ci.DrawCircle(Vector2.Zero, 11f, ProjectileColor(visual));
                ci.DrawCircle(new Vector2(-3f, -4f), 3.5f, new Color(1f, 0.95f, 0.7f));
                break;
            default:
                WarnMissing("projectile", visual);
                ci.DrawCircle(Vector2.Zero, 8f, Colors.Magenta);
                break;
        }
    }

    // -------------------------------------------------------------- helpers

    public static void Eyes(CanvasItem ci, Vector2 center, float spacing, float r, float face, bool angry)
    {
        for (int side = -1; side <= 1; side += 2)
        {
            var p = center + new Vector2(side * spacing, 0f);
            ci.DrawCircle(p, r, Colors.White);
            ci.DrawArc(p, r, 0f, Mathf.Tau, 20, Ink, Mathf.Max(1.5f, r * 0.2f), true);
            var pupil = p + new Vector2(face * r * 0.35f, r * 0.1f);
            ci.DrawCircle(pupil, r * 0.5f, Ink);
            ci.DrawCircle(pupil + new Vector2(-r * 0.18f, -r * 0.2f), r * 0.16f, Colors.White);
            if (angry)
                ci.DrawLine(p + new Vector2(side * r * 1.05f, -r * 1.55f), p + new Vector2(-side * r * 0.8f, -r * 1.0f), Ink, Mathf.Max(2f, r * 0.35f), true);
        }
    }

    public static void Box(CanvasItem ci, Rect2 rect, Color fill, Color border, int radius, int borderWidth)
    {
        var style = new StyleBoxFlat { BgColor = fill, BorderColor = border };
        style.SetCornerRadiusAll(radius);
        style.SetBorderWidthAll(borderWidth);
        ci.DrawStyleBox(style, rect);
    }

    public static Vector2[] Ellipse(Vector2 center, float rx, float ry, int points = 28)
    {
        var result = new Vector2[points];
        for (int i = 0; i < points; i++)
        {
            float a = i * Mathf.Tau / points;
            result[i] = center + new Vector2(Mathf.Cos(a) * rx, Mathf.Sin(a) * ry);
        }
        return result;
    }

    /// <summary>An egg lying on its side, the broad end to the right.</summary>
    public static Vector2[] EggOutline(float w, float h, int points = 40)
    {
        var result = new Vector2[points];
        for (int i = 0; i < points; i++)
        {
            float a = i * Mathf.Tau / points;
            float c = Mathf.Cos(a);
            result[i] = new Vector2(c * w / 2f, Mathf.Sin(a) * h / 2f * (0.88f + 0.12f * c));
        }
        return result;
    }

    public static Vector2[] Closed(Vector2[] points)
    {
        var result = new Vector2[points.Length + 1];
        points.CopyTo(result, 0);
        result[^1] = points[0];
        return result;
    }
}
