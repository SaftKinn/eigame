using Godot;

namespace RettetDasEi;

/// <summary>
/// Glue torn loose: three strings of cheese stretch between the two pieces and fade (ADR 003 —
/// how the tear is seen without frames).
/// </summary>
public partial class CheeseStrings : Node2D
{
    const float Life = 0.55f;
    static readonly Color Cheese = new(0.98f, 0.82f, 0.3f);

    Node2D? _a;
    Node2D? _b;
    Vector2 _offsetA;
    Vector2 _offsetB;
    Vector2 _atA;
    Vector2 _atB;
    float _t;

    public static void Spawn(Node parent, Node2D a, Node2D b)
    {
        var middle = (a.GlobalPosition + b.GlobalPosition) / 2f;
        var strings = new CheeseStrings
        {
            _a = a,
            _b = b,
            _offsetA = middle - a.GlobalPosition,
            _offsetB = middle - b.GlobalPosition,
            _atA = middle,
            _atB = middle,
            TopLevel = true,
            ZIndex = 4,
        };
        parent.AddChild(strings);
    }

    public override void _Process(double delta)
    {
        _t += (float)delta;
        if (_t >= Life)
        {
            QueueFree();
            return;
        }
        if (_a != null && IsInstanceValid(_a))
            _atA = _a.GlobalPosition + _offsetA;
        if (_b != null && IsInstanceValid(_b))
            _atB = _b.GlobalPosition + _offsetB;
        QueueRedraw();
    }

    public override void _Draw()
    {
        float fade = 1f - _t / Life;
        var color = new Color(Cheese, fade);
        var points = new Vector2[8];
        for (int s = -1; s <= 1; s++)
        {
            var from = _atA + new Vector2(s * 14f, 0f);
            var to = _atB + new Vector2(s * 10f, 0f);
            float sag = 10f + 18f * _t / Life;
            for (int i = 0; i < points.Length; i++)
            {
                float p = i / (points.Length - 1f);
                points[i] = from.Lerp(to, p) + new Vector2(0f, Mathf.Sin(p * Mathf.Pi) * sag);
            }
            DrawPolyline(points, color, 3f * fade + 1f, true);
        }
    }
}
